using CodeGen;
using DecoMaker.Common;
using DecoMaker.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DecoMaker.Generation
{
    [Generator]
    internal class DecoratorGenerator : ISourceGenerator
    {       
        private readonly DecoratorNamespaceFactory _NamespaceFactory;

        public DecoratorGenerator()
        {
            var methodTemplateSelectorFactory = new MethodTemplateSelectorFactory();
            var propertyTemplateSelectorFactory = new PropertyTemplateSelectorFactory();

            var methodFactory = new DecoratorMethodFactory(methodTemplateSelectorFactory);
            var propertyFactory = new DecoratorPropertyFactory(propertyTemplateSelectorFactory);
            var classFactory = new DecoratorClassFactory(methodFactory, propertyFactory);
            var extensionFactory = new DecoratorExtensionClassFactory();

            _NamespaceFactory = new DecoratorNamespaceFactory(classFactory, extensionFactory);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            //if (!Debugger.IsAttached) { Debugger.Launch(); }

            context.RegisterForSyntaxNotifications(() => new TypeToBeDecoratedSyntaxReciever());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var processedClasses = new List<string>();

            TypeToBeDecoratedSyntaxReciever syntaxReceiver = (TypeToBeDecoratedSyntaxReciever) context.SyntaxReceiver;

            foreach (TypeToBeDecorated typeToBeDecorated in syntaxReceiver.Classes)
            {
                if (processedClasses.Contains(typeToBeDecorated.TypeDeclaration.Identifier.Text))
                    continue;

                processedClasses.Add(typeToBeDecorated.TypeDeclaration.Identifier.Text);

                SemanticModel semanticModel = context.Compilation.GetSemanticModel(typeToBeDecorated.TypeDeclaration.SyntaxTree);
                ISymbol typeSymbol = semanticModel.GetDeclaredSymbol(typeToBeDecorated.TypeDeclaration);

                // Filter out false positives returned by the syntax reciever.
                AttributeSyntax[] validDecorateAttributes = typeToBeDecorated.DecorateAttributes
                    .Where(attrSyn => IsDecorateAttribute(semanticModel, attrSyn))
                    .ToArray();

                if (validDecorateAttributes.Length == 0) continue;

                DecoratorNamespaceInformation[] factoryInformations = validDecorateAttributes
                    .Select(decorateAttribute =>
                    {
                        ClassTemplate template = TemplateParser.Parse(semanticModel, typeSymbol, decorateAttribute);
                        return GetDecoratorNamespaceFactoryInformation(typeToBeDecorated.TypeDeclaration, typeSymbol, template, semanticModel);
                    })
                    .ToArray();

                foreach (DecoratorNamespaceInformation factoryInformation in factoryInformations)
                {
                    GenerateDecoratorUsingTemplate(context, factoryInformation);
                }
            }
        }

        private void GenerateDecoratorUsingTemplate(
            GeneratorExecutionContext context,
            DecoratorNamespaceInformation factoryInformation)
        {
            CodeGenNamespace generatedNamespace = _NamespaceFactory.Create(factoryInformation);
            string generatedCodeString = generatedNamespace.GenerateCode();

            SourceText sourceText = SourceText.From(generatedCodeString, Encoding.UTF8);
            context.AddSource($"{factoryInformation.DecoratorName}.cs", sourceText);
        }

        private DecoratorNamespaceInformation GetDecoratorNamespaceFactoryInformation(
            TypeDeclarationSyntax typeDeclaration,
            ISymbol classSymbol,
            ClassTemplate template,
            SemanticModel semanticModel)
        {
            string decoratorName = $"{typeDeclaration.Identifier.Text}{template.Label}Decorator";
            DecoratorClassInformation classInformation = GetDecoratorClassFactoryInformation(typeDeclaration, template, semanticModel);

            return new DecoratorNamespaceInformation(
                classSymbol.ContainingNamespace.Name,
                decoratorName,
                classInformation);
        }

        private DecoratorClassInformation GetDecoratorClassFactoryInformation(
            TypeDeclarationSyntax typeDeclaration, 
            ClassTemplate template, 
            SemanticModel semanticModel)
        {
            IEnumerable<string> genericTypes = typeDeclaration.TypeParameterList?.Parameters.Select(parameter => parameter.Identifier.Text);

            Dictionary<string, string> typeConstraints = typeDeclaration.ConstraintClauses.ToDictionary(
                constraintClause => constraintClause.Name.Identifier.Text,
                constraintClause => constraintClause.Constraints.ToString());

            string decoratorType = typeDeclaration.Identifier.Text + typeDeclaration.TypeParameterList?.ToString();
            string derivedFrom = typeDeclaration is InterfaceDeclarationSyntax ? decoratorType : null;

            return new DecoratorClassInformation(
                genericTypes,
                typeConstraints,
                template,
                decoratorType,
                derivedFrom,
                GetMethodFactoryInformations(typeDeclaration, semanticModel).ToArray(),
                GetPropertyFactoryInformation(typeDeclaration, semanticModel).ToArray());
        }

        private IEnumerable<DecoratorMethodInformation> GetMethodFactoryInformations(TypeDeclarationSyntax typeDeclaration, SemanticModel semanticModel)
        {
            IEnumerable<MethodDeclarationSyntax> methods = typeDeclaration.Members.OfType<MethodDeclarationSyntax>();

            foreach (MethodDeclarationSyntax method in methods)
            {
                IEnumerable<string> genericTypes = method.TypeParameterList?.Parameters.Select(parameter => parameter.Identifier.Text);

                Dictionary<string, string> typeConstraints = method.ConstraintClauses.ToDictionary(
                    constraintClause => constraintClause.Name.Identifier.Text,
                    constraintClause => string.Join(", ", GetConstraintNames(constraintClause, semanticModel)));

                TypeInfo returnTypeInfo = semanticModel.GetTypeInfo(method.ReturnType);

                IEnumerable<MethodParameter> parameterTypes = method.ParameterList.Parameters
                    .Select(parameter => new MethodParameter(
                        semanticModel.GetTypeInfo(parameter.Type).Type.ToString(), 
                        parameter.Identifier.Text));

                yield return new DecoratorMethodInformation(
                    method.Identifier.Text,
                    genericTypes,
                    typeConstraints,
                    returnTypeInfo.Type.ToString(),
                    method.IsAsync(),
                    parameterTypes);
            }
        }

        private IEnumerable<DecoratorPropertyInformation> GetPropertyFactoryInformation(
            TypeDeclarationSyntax typeDeclaration,
            SemanticModel semanticModel)
        {
            IEnumerable<PropertyDeclarationSyntax> properties = typeDeclaration.Members.OfType<PropertyDeclarationSyntax>();
            
            foreach (PropertyDeclarationSyntax property in properties)
            {
                TypeInfo propType = semanticModel.GetTypeInfo(property.Type);

                yield return new DecoratorPropertyInformation(
                    property.Identifier.Text,
                    propType.Type.ToString(),
                    property.HasSetter());
            }
        }

        private bool IsDecorateAttribute(SemanticModel semanticModel, AttributeSyntax attributeSyntax)
        {
            TypeInfo attributeType = semanticModel.GetTypeInfo(attributeSyntax.Name);
            return SymbolEqualityComparer.Default.Equals(attributeType.Type, semanticModel.Compilation.GetTypeByMetadataName(typeof(DecorateAttribute).FullName));
        }

        private IEnumerable<string> GetConstraintNames(TypeParameterConstraintClauseSyntax constraintClause, SemanticModel semanticModel)
        {
            foreach (var constraint in constraintClause.Constraints)
            {
                yield return constraint switch
                {
                    TypeConstraintSyntax typeConstraint => semanticModel.GetTypeInfo(typeConstraint.Type).Type.ToString(),
                    _ => constraint.ToString()
                };
            }
        }
    }
}
