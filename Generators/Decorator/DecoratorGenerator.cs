using CodeGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Generators.STEvent
{
    public class DecorateAttribute : Attribute
    {
        public DecorateAttribute(Type deriveFrom = null)
        {

        }
    }

    [Generator]
    internal class DecoratorGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            //if (!Debugger.IsAttached) { Debugger.Launch(); }

            context.RegisterForSyntaxNotifications(() => new ClassWithAttributeSyntaxReciever(typeof(DecorateAttribute)));
        }

        public void Execute(GeneratorExecutionContext context)
        {
            ClassWithAttributeSyntaxReciever syntaxReceiver = (ClassWithAttributeSyntaxReciever) context.SyntaxReceiver;

            foreach ((ClassDeclarationSyntax classDeclaration, AttributeSyntax decorateAttribute) in syntaxReceiver.Classes)
            {
                SemanticModel semanticModel = context.Compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                ISymbol classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

                TypeInfo attrType = semanticModel.GetTypeInfo(decorateAttribute.Name);
                AttributeData decorateAttributeData = classSymbol.GetAttributes().First(attrData => attrData.AttributeClass.Equals(attrType.Type, SymbolEqualityComparer.Default));

                INamedTypeSymbol decoratedTypeSymbol = decorateAttributeData.ConstructorArguments.FirstOrDefault().Value as INamedTypeSymbol;

                Dictionary<string, string> constraints = classDeclaration.ConstraintClauses.ToDictionary(
                    constraintClause => constraintClause.Name.Identifier.Text,
                    constraintClause => constraintClause.Constraints.ToString());

                IEnumerable<CodeGenGeneric> generics = classDeclaration.TypeParameterList?.Parameters.Select(parameter => GenerateMethodParameter(parameter, constraints));

                string decoratorName = $"{classDeclaration.Identifier.Text}Decorator";
                string decoratedType = decoratedTypeSymbol?.ToString() ?? classSymbol.ToString();

                var generatedClass = new CodeGenClass(
                    decoratorName,
                    Scope.Public,
                    ClassType.Normal,
                    genericTypes: generics,
                    derivedFrom: decoratedTypeSymbol != null ? new[] { decoratedTypeSymbol.ToString() } : null);

                generatedClass.Comment = new CodeGenComment($@"Class: {decoratorName}
Description: 
Decorator for {classSymbol}
Auto-generated on {DateTime.Now}");

                // Constructor
                generatedClass.Constructors.Add(new CodeGenConstructor(
                    decoratorName,
                    Scope.Public,
                    new[] { $"{decoratedType} decorated" },
                    "_Decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));"));

                // Decorated variable
                generatedClass.Variables.Add(new CodeGenVariable(
                    "_Decorated",
                    decoratedType,
                    Scope.Private,
                    readOnly: true));

                GenerateDecoratorMethods(classDeclaration, generatedClass, semanticModel);

                var generatedNamespace = new CodeGenNamespace($"{classSymbol.ContainingNamespace.Name}.Decorators");
                generatedNamespace.Content.Add(generatedClass);
                generatedNamespace.Usings.Add("System");

                var generatedCodeString = generatedNamespace.GenerateCode();

                var sourceText = SourceText.From(generatedCodeString, Encoding.UTF8);
                context.AddSource($"{decoratorName}.cs", sourceText);
            }
        }

        private void GenerateDecoratorMethods(ClassDeclarationSyntax classDeclaration, CodeGenClass decorator, SemanticModel semanticModel)
        {
            var methods = classDeclaration.Members.OfType<MethodDeclarationSyntax>()
                .Where(method => method.Modifiers.Any(m => m.Text == "public"));

            foreach (MethodDeclarationSyntax method in methods)
            {
                Dictionary<string, string> constraints = method.ConstraintClauses.ToDictionary(
                    constraintClause => constraintClause.Name.Identifier.Text,
                    constraintClause => string.Join(", ", GetConstraintNames(constraintClause, semanticModel)));

                IEnumerable<CodeGenGeneric> generics = method.TypeParameterList?.Parameters.Select(parameter => GenerateMethodParameter(parameter, constraints));
                IEnumerable<string> parameters = method.ParameterList.Parameters.Select(parameter => $"{semanticModel.GetTypeInfo(parameter.Type).Type} {parameter.Identifier.Text}");

                var returnTypeInfo = semanticModel.GetTypeInfo(method.ReturnType);

                decorator.Methods.Add(new CodeGenMethod(
                    method.Identifier.Text,
                    returnTypeInfo.Type.ToString(),
                    Scope.Public,
                    MethodType.Normal,
                    generics,
                    parameters,
                    GenerateMethodBody(method, classDeclaration, decorator.Name)));
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

        private string GenerateMethodBody(MethodDeclarationSyntax method, ClassDeclarationSyntax classDeclaration, string decoratorName)
        {
            // void return type is a PredefinedNameSyntax
            bool hasReturn = method.ReturnType is not PredefinedTypeSyntax predefinedType || predefinedType.Keyword.Text != "void";

            var bodyBuilder = new StringBuilder();

            bodyBuilder.AppendLine("try");
            bodyBuilder.AppendLine("{");

            IEnumerable<string> paramNameList = method.ParameterList.Parameters.Select(parameter => parameter.Identifier.Text);

            bodyBuilder.Append("\t");

            if (hasReturn) bodyBuilder.Append("return ");

            bodyBuilder.AppendLine($"_Decorated.{method.Identifier.Text}({string.Join(", ", paramNameList)});");

            bodyBuilder.AppendLine("}");
            bodyBuilder.AppendLine("catch (Exception e)");
            bodyBuilder.AppendLine("{");
            bodyBuilder.AppendLine($"\tConsole.WriteLine($\"ERROR: '{method.Identifier.Text}' in '{decoratorName}': {{e}}\");");
            bodyBuilder.AppendLine("\tthrow;");
            bodyBuilder.AppendLine("}");

            return bodyBuilder.ToString();
        }

        private CodeGenGeneric GenerateMethodParameter(TypeParameterSyntax parameter, Dictionary<string, string> constraints)
        {
            constraints.TryGetValue(parameter.Identifier.Text, out string constraint);
            return new CodeGenGeneric(parameter.Identifier.Text, constraint);
        }
    }
}
