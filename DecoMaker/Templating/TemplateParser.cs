using DecoMaker.Common;
using DecoMaker.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DecoMaker.Templating
{
    /// <summary>
    /// Parses template data from the template classes compiler syntax definitions.
    /// </summary>
    internal static class TemplateParser
    {
        /// <summary>
        /// Parse template data from a template class complier syntax definition.
        /// </summary>
        /// <param name="semanticModel">The semantic model to retrieve syntax from.</param>
        /// <param name="decoratedClassSymbol">Symbol of the class to be decorated.</param>
        /// <param name="attributeSyntax">Syntax of the <see cref="DecorateAttribute"/> targeting a template.</param>
        /// <returns>The class template.</returns>
        public static ClassTemplate Parse(
            SemanticModel semanticModel,
            ISymbol decoratedClassSymbol,
            AttributeSyntax attributeSyntax)
        {
            string templateName = GetTemplateName(decoratedClassSymbol, attributeSyntax);

            ClassDeclarationSyntax templateClassDeclaration = GetTemplateSyntax(semanticModel, attributeSyntax);

            ConstructorParam[] constructorParams = templateClassDeclaration.Members
                .OfType<ConstructorDeclarationSyntax>()
                .FirstOrDefault()
               ?.ParameterList
                .Parameters
                .Select(param => new ConstructorParam
                {
                    Name = param.Identifier.Text,
                    Type = param.Type.ToString()
                })
                .ToArray();

            MethodTemplate[] templateMethods = templateClassDeclaration.Members
                .OfType<MethodDeclarationSyntax>()
                .Where(methodDeclaration => methodDeclaration.IsPublic())
                .Select(methodDeclaration => ParseMethod(semanticModel, methodDeclaration))
                .ToArray();

            PropertyTemplate[] templateProperties = templateClassDeclaration.Members
                .OfType<PropertyDeclarationSyntax>()
                .Where(methodDeclaration => methodDeclaration.IsPublic())
                .Select(propertyDeclaration => ParseProperty(semanticModel, propertyDeclaration))
                .ToArray();

            return new ClassTemplate
            {
                Label = templateName,
                ConstructorParams = constructorParams,
                TemplateMethods = templateMethods,
                TemplateProperties = templateProperties
            };
        }

        private static string GetTemplateName(ISymbol decoratedClassSymbol, AttributeSyntax attributeSyntax)
        {
            AttributeData matchingAttribute = decoratedClassSymbol.GetAttributes()
                .FirstOrDefault(attrData => attrData.ApplicationSyntaxReference.GetSyntax().IsEquivalentTo(attributeSyntax));

            string name = matchingAttribute?.ConstructorArguments.FirstOrDefault().Value?.ToString();
            return !string.IsNullOrWhiteSpace(name) ? name : throw new Exception("Valid name not proivded in Decorate attribute");
        }

        private static ClassDeclarationSyntax GetTemplateSyntax(SemanticModel semanticModel, AttributeSyntax attributeSyntax)
        {
            var typeOfExpression = (TypeOfExpressionSyntax) attributeSyntax.ArgumentList.Arguments[1].Expression;
            var typeOfTypeNameSyntax = (IdentifierNameSyntax) typeOfExpression.Type;

            SymbolInfo templateTypeSymbol = semanticModel.GetSymbolInfo(typeOfTypeNameSyntax);

            TextSpan templateSourceSpan = templateTypeSymbol.Symbol.Locations.Length == 1
                ? templateTypeSymbol.Symbol.Locations[0].SourceSpan
                : throw new Exception("Template defined in more than one location. Partial classes as templates are not permitted.");

            SyntaxNode templateSyntax = semanticModel.SyntaxTree.GetRoot().FindNode(templateSourceSpan);

            if (templateSyntax is not ClassDeclarationSyntax classTemplateSyntax)
                throw new Exception("Template was not a class");

            return classTemplateSyntax;
        }

        private static MethodTemplate ParseMethod(SemanticModel semanticModel, MethodDeclarationSyntax methodDeclaration)
        {
            var matchCondition = new MethodTemplateMatchCondition();

            PopulateReturnTypeMatchConditions(matchCondition, semanticModel, methodDeclaration);
            PopulateParameterTypeMatchConditions(matchCondition, semanticModel, methodDeclaration);

            string body = methodDeclaration.ExpressionBody?.Expression != null
                ? $"return {methodDeclaration.ExpressionBody.Expression}" 
                : methodDeclaration.Body.ToString();

            return new MethodTemplate
            {
                Name = methodDeclaration.Identifier.Text,
                Async = methodDeclaration.IsAsync(),
                MatchCondition = matchCondition,
                Body = body
            };
        }

        private static PropertyTemplate ParseProperty(SemanticModel semanticModel, PropertyDeclarationSyntax propertyDeclaration)
        {
            var matchOnCondition = new PropertyTemplateMatchCondition();

            var templateProperty = new PropertyTemplate
            {
                Name = propertyDeclaration.Identifier.Text,
                MatchCondition = matchOnCondition
            };

            if (propertyDeclaration.ExpressionBody != null)
            {
                templateProperty.GetterBody = propertyDeclaration.ExpressionBody.Expression.ToString();
            }
            else
            {
                AccessorDeclarationSyntax getterAccessor = propertyDeclaration.AccessorList.Accessors.FirstOrDefault(acc => acc.Keyword.Text == "get");
                AccessorDeclarationSyntax setterAccessor = propertyDeclaration.AccessorList.Accessors.FirstOrDefault(acc => acc.Keyword.Text == "set");

                templateProperty.GetterBody = getterAccessor?.Body?.ToString();
                templateProperty.SetterBody = setterAccessor?.Body?.ToString();
            }

            return templateProperty;
        }

        private static void PopulateReturnTypeMatchConditions(
            MethodTemplateMatchCondition matchOnCondition, 
            SemanticModel semanticModel, 
            MethodDeclarationSyntax methodDeclaration)
        {
            if (methodDeclaration.ReturnType is QualifiedNameSyntax
                {
                    Left: QualifiedNameSyntax
                    {
                        Left: QualifiedNameSyntax
                        {
                            Left: IdentifierNameSyntax { Identifier: { Text: "Decorated" } },
                            Right: IdentifierNameSyntax { Identifier: { Text: "Method" } }
                        },
                        Right: IdentifierNameSyntax { Identifier: { Text: "Return" } }
                    },
                    Right: IdentifierNameSyntax { Identifier: { Text: "Any" } }
                } anyReturnType)
            {
                matchOnCondition.ReturnTypeRule = ReturnTypeRule.Any;
            }
            else
            {
                matchOnCondition.ReturnTypeRule = ReturnTypeRule.Specified;
                matchOnCondition.ReturnType = semanticModel.GetTypeInfo(methodDeclaration.ReturnType).Type.ToString();
            }
        }

        private static void PopulateParameterTypeMatchConditions(
            MethodTemplateMatchCondition matchOnCondition,
            SemanticModel semanticModel,
            MethodDeclarationSyntax methodDeclaration)
        {
            var paramTypes = new Dictionary<string, string>();

            foreach (ParameterSyntax parameter in methodDeclaration.ParameterList.Parameters)
            {
                if (parameter.Type is not QualifiedNameSyntax
                    {
                        Left: QualifiedNameSyntax
                        {
                            Left: QualifiedNameSyntax
                            {
                                Left: IdentifierNameSyntax { Identifier: { Text: "Decorated" } },
                                Right: IdentifierNameSyntax { Identifier: { Text: "Method" } }
                            },
                            Right: IdentifierNameSyntax { Identifier: { Text: "Param" } }
                        },
                        Right: IdentifierNameSyntax { Identifier: { Text: "Any" } }
                    })
                {
                    matchOnCondition.ParamTypeRule = ParamTypeRule.Any;
                    return;
                }
                else
                {
                    paramTypes.Add(semanticModel.GetTypeInfo(parameter.Type).Type.ToString(), parameter.Identifier.Text);
                }
            }

            matchOnCondition.ParamTypeRule = ParamTypeRule.Specified;
            matchOnCondition.Params = paramTypes;
        }
    }
}
