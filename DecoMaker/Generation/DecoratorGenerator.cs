using CodeGen;
using DecoMaker.Common;
using DecoMaker.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DecoMaker.Generation
{
    [Generator]
    internal class DecoratorGenerator : ISourceGenerator
    {
        private static Regex MethodInvokeRegex = new Regex("Decorated\\.Method\\.Invoke(?:<.*>)?\\(\\)", RegexOptions.Compiled);
        private static Regex VoidReturnMethodInvokeRegex = new Regex("return Decorated\\.Method\\.Invoke(?:<.*>)?\\(\\)", RegexOptions.Compiled);

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

                foreach (AttributeSyntax decorateAttribute in validDecorateAttributes)
                {
                    ClassTemplate template = TemplateParser.Parse(semanticModel, typeSymbol, decorateAttribute);
                    GenerateDecoratorUsingTemplate(context, typeToBeDecorated.TypeDeclaration, typeSymbol, template, semanticModel);
                }
            }
        }

        private void GenerateDecoratorUsingTemplate(
            GeneratorExecutionContext context,
            TypeDeclarationSyntax typeDeclaration,
            ISymbol classSymbol,
            ClassTemplate template,
            SemanticModel semanticModel)
        {
            string decoratorName = $"{typeDeclaration.Identifier.Text}{template.Label}Decorator";

            var generatedDecoratorClass = GenerateDecoratorClass(decoratorName, typeDeclaration, template, classSymbol, semanticModel);


            var generatedNamespace = new CodeGenNamespace($"{classSymbol.ContainingNamespace.Name}.Decorators");
            generatedNamespace.Content.Add(generatedDecoratorClass);
            generatedNamespace.Usings.Add("System");

            var generatedCodeString = generatedNamespace.GenerateCode();

            var sourceText = SourceText.From(generatedCodeString, Encoding.UTF8);
            context.AddSource($"{decoratorName}.cs", sourceText);
        }

        private CodeGenClass GenerateDecoratorClass(
            string decoratorName,
            TypeDeclarationSyntax typeDeclaration, 
            ClassTemplate template, 
            ISymbol classSymbol, 
            SemanticModel semanticModel)
        {
            Dictionary<string, string> constraints = typeDeclaration.ConstraintClauses.ToDictionary(
                constraintClause => constraintClause.Name.Identifier.Text,
                constraintClause => constraintClause.Constraints.ToString());

            IEnumerable<CodeGenGeneric> generics = typeDeclaration.TypeParameterList?.Parameters.Select(parameter => GenerateMethodParameter(parameter, constraints));

            var generatedDecoratorClass = new CodeGenClass(
                decoratorName,
                Scope.Public,
                ClassType.Normal,
                genericTypes: generics,
                derivedFrom: typeDeclaration is InterfaceDeclarationSyntax ? new[] { typeDeclaration.Identifier.Text } : null);

            generatedDecoratorClass.Comment = new CodeGenComment($@"Generated {template.Label} decorator for the {classSymbol} class. Auto-generated on {DateTimeOffset.Now}");

            var constructorParams = new List<string>(new[] { $"{typeDeclaration.Identifier.Text} decorated" });
            var constructorBodyBuilder = new StringBuilder();

            constructorBodyBuilder.AppendLine("_Decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));");

            if (template.ConstructorParams != null)
            {
                foreach (ConstructorParam constructorParam in template.ConstructorParams)
                {
                    constructorParams.Add($"{constructorParam.Type} {constructorParam.Name}");
                    constructorBodyBuilder.AppendLine($"_{constructorParam.Name} = {constructorParam.Name};");

                    generatedDecoratorClass.Variables.Add(new CodeGenVariable(
                        $"_{constructorParam.Name}",
                        constructorParam.Type,
                        Scope.Private,
                        readOnly: true));
                }
            }

            // Constructor
            generatedDecoratorClass.Constructors.Add(new CodeGenConstructor(
                decoratorName,
                Scope.Public,
                constructorParams,
                constructorBodyBuilder.ToString()));

            // Decorated variable
            generatedDecoratorClass.Variables.Add(new CodeGenVariable(
                "_Decorated",
                typeDeclaration.Identifier.Text,
                Scope.Private,
                readOnly: true));

            GenerateDecoratorMethods(typeDeclaration, generatedDecoratorClass, semanticModel, template);
            GenerateDecoratorProperties(typeDeclaration, generatedDecoratorClass, semanticModel, template);

            return generatedDecoratorClass;
        }

        private bool IsDecorateAttribute(SemanticModel semanticModel, AttributeSyntax attributeSyntax)
        {
            TypeInfo attributeType = semanticModel.GetTypeInfo(attributeSyntax.Name);
            return SymbolEqualityComparer.Default.Equals(attributeType.Type, semanticModel.Compilation.GetTypeByMetadataName(typeof(DecorateAttribute).FullName));
        }

        private void GenerateDecoratorMethods(
            TypeDeclarationSyntax typeDeclaration,
            CodeGenClass decorator,
            SemanticModel semanticModel,
            ClassTemplate template)
        {
            IEnumerable<MethodDeclarationSyntax> methods = typeDeclaration.Members.OfType<MethodDeclarationSyntax>();

            if (typeDeclaration is ClassDeclarationSyntax)
                methods = methods.Where(method => method.IsPublic());

            IMethodTemplateSelector selector = new MethodTemplateSelector(template.TemplateMethods);

            foreach (MethodDeclarationSyntax method in methods)
            {
                Dictionary<string, string> constraints = method.ConstraintClauses.ToDictionary(
                    constraintClause => constraintClause.Name.Identifier.Text,
                    constraintClause => string.Join(", ", GetConstraintNames(constraintClause, semanticModel)));

                IEnumerable<CodeGenGeneric> generics = method.TypeParameterList?.Parameters.Select(parameter => GenerateMethodParameter(parameter, constraints));
                IEnumerable<(ParameterSyntax Parameter, TypeInfo TypeInfo)> parameterTypes = method.ParameterList.Parameters.Select(parameter => (Parameter: parameter, TypeInfo: semanticModel.GetTypeInfo(parameter.Type)));

                IEnumerable<string> parameters = parameterTypes.Select(parameter => $"{parameter.TypeInfo.Type} {parameter.Parameter.Identifier.Text}");
                TypeInfo returnTypeInfo = semanticModel.GetTypeInfo(method.ReturnType);

                MethodTemplate matchingTemplate = selector.Select(
                    method.Identifier.Text,
                    returnTypeInfo.Type.ToString(),
                    parameterTypes.Select(parameter => parameter.TypeInfo.Type.ToString()).ToArray(),
                    method.IsAsync());

                string body = matchingTemplate != null
                    ? GenerateDecoratorMethodBodyFromCondition(matchingTemplate, method)
                    : GenerateUndecoratedMethodBody(method);

                decorator.Methods.Add(new CodeGenMethod(
                    method.Identifier.Text,
                    returnTypeInfo.Type.ToString(),
                    Scope.Public,
                    MethodType.Normal,
                    generics,
                    parameters,
                    body,
                    matchingTemplate?.Async ?? false));
            }
        }

        private void GenerateDecoratorProperties(
            TypeDeclarationSyntax typeDeclaration,
            CodeGenClass decorator,
            SemanticModel semanticModel,
            ClassTemplate template)
        {
            IEnumerable<PropertyDeclarationSyntax> properties = typeDeclaration.Members.OfType<PropertyDeclarationSyntax>();

            if (typeDeclaration is ClassDeclarationSyntax)
                properties = properties.Where(method => method.IsPublic());

            PropertyTemplateSelector selector = new PropertyTemplateSelector(template.TemplateProperties);

            foreach (PropertyDeclarationSyntax property in properties)
            {   
                TypeInfo propType = semanticModel.GetTypeInfo(property.Type);
                bool hasSetter = property.HasSetter();

                PropertyTemplate matchingTemplate = selector.Select(
                    property.Identifier.Text,
                    propType.Type.ToString(),
                    hasSetter);

                string getterBody = matchingTemplate != null
                    ? GenerateDecoratorPropertyGetterBodyFromCondition(matchingTemplate, property)
                    : GenerateUndecoratedPropertyGetterBody(property);

                string setterBody = hasSetter 
                    ? matchingTemplate != null
                        ? GenerateDecoratorPropertySetterBodyFromCondition(matchingTemplate, property)
                        : GenerateUndecoratedPropertySetterBody(property)
                    : null;

                decorator.Properties.Add(new CodeGenProperty(
                    property.Identifier.Text,
                    propType.Type.ToString(),
                    Scope.Public,
                    getterBody,
                    setterBody));
            }
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

        private string GenerateDecoratorMethodBodyFromCondition(MethodTemplate template, MethodDeclarationSyntax method)
        {
            IEnumerable<string> paramNameList = method.ParameterList.Parameters.Select(parameter => parameter.Identifier.Text);
            string invocation = $"_Decorated.{method.Identifier.Text}({string.Join(", ", paramNameList)})";

            string body = template.Body.Clone() as string;

            if (method.ReturnType.ToString() == "void" && template.MatchCondition.ReturnTypeRule == ReturnTypeRule.Any)
            {
                body = VoidReturnMethodInvokeRegex.Replace(body, invocation);
            }
            else
            {
                body = MethodInvokeRegex.Replace(body, invocation);
            }

            return CleanBody(body);
        }

        private string GenerateUndecoratedMethodBody(MethodDeclarationSyntax method)
        {
            IEnumerable<string> paramNameList = method.ParameterList.Parameters.Select(parameter => parameter.Identifier.Text);
            return $"_Decorated.{method.Identifier.Text}({string.Join(", ", paramNameList)});";
        }

        private string GenerateDecoratorPropertyGetterBodyFromCondition(PropertyTemplate template, PropertyDeclarationSyntax property)
        {
            string body = template.GetterBody.Clone() as string;
            body = body.Replace("Decorated.Property.Any.Value", $"_Decorated.{property.Identifier.Text}");

            return CleanBody(body);
        }

        private string GenerateUndecoratedPropertyGetterBody(PropertyDeclarationSyntax property)
        {
            return $"return _Decorated.{property.Identifier.Text};";
        }

        private string GenerateDecoratorPropertySetterBodyFromCondition(PropertyTemplate template, PropertyDeclarationSyntax property)
        {
            string body = template.SetterBody.Clone() as string;
            body = body.Replace("Decorated.Property.Any.Value", $"_Decorated.{property.Identifier.Text}");

            return CleanBody(body);

        }

        private string GenerateUndecoratedPropertySetterBody(PropertyDeclarationSyntax property)
        {
            return $"_Decorated.{property.Identifier.Text} = value;";
        }

        private CodeGenGeneric GenerateMethodParameter(TypeParameterSyntax parameter, Dictionary<string, string> constraints)
        {
            constraints.TryGetValue(parameter.Identifier.Text, out string constraint);
            return new CodeGenGeneric(parameter.Identifier.Text, constraint);
        }
    }
}
