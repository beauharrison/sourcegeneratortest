﻿using CodeGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DecoMaker
{
    [Generator]
    internal class DecoratorGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            if (!Debugger.IsAttached) { Debugger.Launch(); }

            context.RegisterForSyntaxNotifications(() => new ClassToBeDecoratedSyntaxReciever());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var processedClasses = new List<string>();

            ClassToBeDecoratedSyntaxReciever syntaxReceiver = (ClassToBeDecoratedSyntaxReciever) context.SyntaxReceiver;

            foreach ((ClassDeclarationSyntax classDeclaration, AttributeSyntax[] decorateAttributes) in syntaxReceiver.Classes)
            {
                if (processedClasses.Contains(classDeclaration.Identifier.Text))
                    continue;

                processedClasses.Add(classDeclaration.Identifier.Text);

                SemanticModel semanticModel = context.Compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                ISymbol classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

                // Filter out false positives returned by the syntax reciever.
                AttributeSyntax[] validDecorateAttributes = decorateAttributes
                    .Where(attrSyn => IsDecorateAttribute(semanticModel, attrSyn))
                    .ToArray();

                if (validDecorateAttributes.Length == 0) continue;


                foreach (AttributeSyntax decorateAttribute in validDecorateAttributes)
                {
                    ClassTemplate template = TemplateParser.Parse(semanticModel, classSymbol, decorateAttribute);
                    GenerateDecoratorUsingTemplate(context, classDeclaration, classSymbol, template);
                }                
            }
        }

        private void GenerateDecoratorUsingTemplate(
            GeneratorExecutionContext context, 
            ClassDeclarationSyntax classDeclaration,
            ISymbol classSymbol, 
            ClassTemplate template)
        {
            Dictionary<string, string> constraints = classDeclaration.ConstraintClauses.ToDictionary(
                constraintClause => constraintClause.Name.Identifier.Text,
                constraintClause => constraintClause.Constraints.ToString());

            IEnumerable<CodeGenGeneric> generics = classDeclaration.TypeParameterList?.Parameters.Select(parameter => GenerateMethodParameter(parameter, constraints));

            string decoratorName = $"{classDeclaration.Identifier.Text}{template.Label}Decorator";

            var generatedClass = new CodeGenClass(
                decoratorName,
                Scope.Public,
                ClassType.Normal,
                genericTypes: generics,
                derivedFrom: template.ImplementationType != null ? new[] { template.ImplementationType } : null);

            generatedClass.Comment = new CodeGenComment($@"Generated {template.Label} decorator for the {classSymbol} class. Auto-generated on {DateTimeOffset.Now}");

            // Constructor
            generatedClass.Constructors.Add(new CodeGenConstructor(
                decoratorName,
                Scope.Public,
                new[] { $"{classDeclaration.Identifier.Text} decorated" },
                "_Decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));"));

            // Decorated variable
            generatedClass.Variables.Add(new CodeGenVariable(
                "_Decorated",
                classDeclaration.Identifier.Text,
                Scope.Private,
                readOnly: true));

            // TODO method generation
            //GenerateDecoratorMethods(classDeclaration, generatedClass, semanticModel);

            var generatedNamespace = new CodeGenNamespace($"{classSymbol.ContainingNamespace.Name}.Decorators");
            generatedNamespace.Content.Add(generatedClass);

            var generatedCodeString = generatedNamespace.GenerateCode();

            var sourceText = SourceText.From(generatedCodeString, Encoding.UTF8);
            context.AddSource($"{decoratorName}.cs", sourceText);
        }

        private bool IsDecorateAttribute(SemanticModel semanticModel, AttributeSyntax attributeSyntax)
        {
            TypeInfo attributeType = semanticModel.GetTypeInfo(attributeSyntax.Name);
            return SymbolEqualityComparer.Default.Equals(attributeType.Type, semanticModel.Compilation.GetTypeByMetadataName(typeof(DecorateAttribute).FullName));
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