using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DecoMaker
{
    internal class TemplateParser
    {
        public static ClassTemplate Parse(
            SemanticModel semanticModel,
            ISymbol decoratedClassSymbol,
            AttributeSyntax attributeSyntax)
        {
            string templateName = GetTemplateName(decoratedClassSymbol, attributeSyntax);
            string implementationType = GetImplementationType(semanticModel, attributeSyntax);

            ClassDeclarationSyntax templateClassDeclaration = GetTemplateSyntax(semanticModel, attributeSyntax);

            TemplateMethod[] templateMethods = templateClassDeclaration.Members
                .OfType<MethodDeclarationSyntax>()
                .Select(methodDeclaration => ParseMethod(semanticModel, methodDeclaration))
                .ToArray();

            TemplateProperty[] templateProperties = templateClassDeclaration.Members
                .OfType<PropertyDeclarationSyntax>()
                .Select(propertyDeclaration => ParseProperty(semanticModel, propertyDeclaration))
                .ToArray();

            return new ClassTemplate
            {
                Label = templateName,
                ImplementationType = implementationType,
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
                
        private static string GetImplementationType(SemanticModel semanticModel, AttributeSyntax attributeSyntax)
        {
            if (attributeSyntax.ArgumentList.Arguments.Count < 3) return null;

            var typeOfExpression = (TypeOfExpressionSyntax) attributeSyntax.ArgumentList.Arguments[2].Expression;
            var typeOfTypeNameSyntax = (IdentifierNameSyntax) typeOfExpression.Type;

            return semanticModel.GetSymbolInfo(typeOfTypeNameSyntax).Symbol.ToString();
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

        private static TemplateMethod ParseMethod(SemanticModel semanticModel, MethodDeclarationSyntax methodDeclaration)
        {
            var matchOnCondition = new MethodMatchOnCondition();

            PopulateReturnTypeMatchConditions(matchOnCondition, semanticModel, methodDeclaration);
            PopulateParameterTypeMatchConditions(matchOnCondition, semanticModel, methodDeclaration);

            string body = methodDeclaration.ExpressionBody?.Expression != null
                ? methodDeclaration.ExpressionBody.Expression.ToString()
                : methodDeclaration.Body.ToString();

            return new TemplateMethod
            {
                Name = methodDeclaration.Identifier.Text,
                MatchOnCondition = matchOnCondition,
                Body = body
            };
        }

        private static TemplateProperty ParseProperty(SemanticModel semanticModel, PropertyDeclarationSyntax propertyDeclaration)
        {
            var matchOnCondition = new PropertyMatchOnCondition();

            var templateProperty = new TemplateProperty
            {
                Name = propertyDeclaration.Identifier.Text,
                MatchOnCondition = matchOnCondition
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
            MethodMatchOnCondition matchOnCondition, 
            SemanticModel semanticModel, 
            MethodDeclarationSyntax methodDeclaration)
        {
            if (methodDeclaration.ReturnType is not QualifiedNameSyntax
            {
                Left: QualifiedNameSyntax
                {
                    Left: QualifiedNameSyntax
                    {
                        Left: IdentifierNameSyntax
                        {
                            Identifier: { Text: "Decorated" }
                        },
                        Right: IdentifierNameSyntax
                        {
                            Identifier: { Text: "Method" }
                        }
                    },
                    Right: IdentifierNameSyntax
                    {
                        Identifier: { Text: "Return" or "AsyncReturn" } returnKind
                    }
                }
            } returnType)
            {
                throw new Exception("Unexpected template method return type.");
            }

            matchOnCondition.Async = returnKind.Text == "AsyncReturn";

            // Get return type
            if (returnType.Right is GenericNameSyntax { Identifier: { Text: "Of" } } of)
            {
                TypeSyntax type = of.TypeArgumentList.Arguments.FirstOrDefault() ?? throw new Exception("Something went wrong!");
                matchOnCondition.ReturnType = semanticModel.GetTypeInfo(type).Type.ToString();
            }
            else if (returnType.Right is IdentifierNameSyntax { Identifier: { Text: "Void" } })
            {
                matchOnCondition.ReturnTypeMatchOn = ReturnTypeMatchOn.Void;
            }
            else if (returnType.Right is IdentifierNameSyntax { Identifier: { Text: "Any" } })
            {
                matchOnCondition.ReturnTypeMatchOn = ReturnTypeMatchOn.Any;
            }
            else
            {
                throw new Exception("Unexpected template method return type.");
            }
        }

        private static void PopulateParameterTypeMatchConditions(
            MethodMatchOnCondition matchOnCondition,
            SemanticModel semanticModel,
            MethodDeclarationSyntax methodDeclaration)
        {
            var paramTypes = new List<(string Type, string Name)>();

            if (methodDeclaration.ParameterList.Parameters.Count < 1)
            {
                throw new Exception("Template method needs at least one paramter. Use `Decorated.Method.Param.Any`, or `Decorated.Method.Param.None` to specifically match on 0 parameter method.");
            }

            foreach (ParameterSyntax parameter in methodDeclaration.ParameterList.Parameters)
            {
                if (parameter.Type is not QualifiedNameSyntax
                    {
                        Left: QualifiedNameSyntax
                        {
                            Left: QualifiedNameSyntax
                            {
                                Left: IdentifierNameSyntax
                                {
                                    Identifier: { Text: "Decorated" }
                                },
                                Right: IdentifierNameSyntax
                                {
                                    Identifier: { Text: "Method" }
                                }
                            },
                            Right: IdentifierNameSyntax
                            {
                                Identifier: { Text: "Param" }
                            }
                        }
                    } paramType)
                {
                    throw new Exception("Unexpected template method parameter type.");
                }

                if (paramType.Right is GenericNameSyntax { Identifier: { Text: "Of" } } of)
                {
                    TypeSyntax type = of.TypeArgumentList.Arguments.FirstOrDefault() ?? throw new Exception("Something went wrong!");
                    paramTypes.Add((semanticModel.GetTypeInfo(type).Type.ToString(), parameter.Identifier.Text));
                }
                else if (paramType.Right is IdentifierNameSyntax { Identifier: { Text: "Any" } } any)
                {
                    matchOnCondition.ParamTypeMatchOn = ParamTypeMatchOn.Any;
                    break;
                }
                else if (paramType.Right is IdentifierNameSyntax { Identifier: { Text: "None" } } none)
                {
                    matchOnCondition.ParamTypeMatchOn = ParamTypeMatchOn.None;
                    break;
                }
                else
                {
                    throw new Exception("Unexpected template method parameter type.");
                }
            }

            matchOnCondition.ParamTypeMatchOn = ParamTypeMatchOn.Specified;
            matchOnCondition.Params = paramTypes.ToArray();
        }
    }
}
