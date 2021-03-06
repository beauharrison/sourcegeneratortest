using CodeGen;
using DecoMaker.Templating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DecoMaker.Generation
{
    internal class DecoratorMethodFactory : IDecoratorFactory<DecoratorMethodInformation, CodeGenMethod>
    {
        private static Regex MethodInvokeRegex = new Regex("Decorated\\.Method\\.Invoke(?:<.*>)?\\(\\)", RegexOptions.Compiled);
        private static Regex VoidReturnMethodInvokeRegex = new Regex("return Decorated\\.Method\\.Invoke(?:<.*>)?\\(\\)", RegexOptions.Compiled);
        
        private readonly IMethodTemplateSelectorFactory _SelectorFactory;

        public DecoratorMethodFactory(IMethodTemplateSelectorFactory selectorFactory)
        {
            _SelectorFactory = selectorFactory ?? throw new ArgumentNullException(nameof(selectorFactory));
        }

        public CodeGenMethod Create(DecoratorMethodInformation factoryInformation)
        {
            IEnumerable<CodeGenGeneric> genericTypes = factoryInformation.GenericTypes?.Select(
                parameter => FactoryHelpers.GenerateMethodParameter(parameter, factoryInformation.TypeConstraints)); 
            
            IEnumerable<string> parameters = factoryInformation.Parameters?.Select(parameter => $"{parameter.Type} {parameter.Name}");

            MethodTemplate matchingTemplate = _SelectorFactory
                .Create(factoryInformation.Templates)
                .Select(
                    factoryInformation.MethodName,
                    factoryInformation.ReturnType,
                    factoryInformation.Parameters?.Select(parameter => parameter.Type)?.ToArray(),
                    factoryInformation.IsAsync);

            string body = matchingTemplate != null
                ? GenerateDecoratorMethodBodyFromCondition(
                    factoryInformation.MethodName, 
                    factoryInformation.ReturnType, 
                    matchingTemplate, 
                    factoryInformation.Parameters)
                : GenerateUndecoratedMethodBody(
                    factoryInformation.MethodName,
                    factoryInformation.Parameters);

            return new CodeGenMethod(
                factoryInformation.MethodName,
                factoryInformation.ReturnType,
                Scope.Public,
                MethodType.Normal,
                genericTypes,
                parameters,
                body,
                matchingTemplate?.Async ?? false);
        }

        private string GenerateDecoratorMethodBodyFromCondition(
            string methodName, 
            string returnType,
            MethodTemplate template, 
            IEnumerable<MethodParameter> parameters)
        {
            IEnumerable<string> paramNameList = parameters?.Select(parameter => parameter.Name) ?? new string[0];
            string invocation = $"_Decorated.{methodName}({string.Join(", ", paramNameList)})";

            string body = template.Body.Clone() as string;

            if (returnType == "void" && template.MatchCondition.ReturnTypeRule == ReturnTypeRule.Any)
            {
                body = VoidReturnMethodInvokeRegex.Replace(body, invocation);
            }
            else
            {
                body = MethodInvokeRegex.Replace(body, invocation);
            }

            return FactoryHelpers.CleanBody(body);
        }

        private string GenerateUndecoratedMethodBody(string methodName, IEnumerable<MethodParameter> parameters)
        {
            IEnumerable<string> paramNameList = parameters?.Select(parameter => parameter.Name) ?? new string[0];
            return $"_Decorated.{methodName}({string.Join(", ", paramNameList)});";
        }
    }
}
