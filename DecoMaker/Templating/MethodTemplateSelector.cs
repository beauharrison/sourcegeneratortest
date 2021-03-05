using System;
using System.Collections.Generic;
using System.Linq;

namespace DecoMaker.Templating
{

    /// <summary>
    /// Selects the best method template for a method being decorated.
    /// </summary>
    internal class MethodTemplateSelector : IMethodTemplateSelector
    {
        private readonly IEnumerable<MethodTemplate> _Templates;

        /// <summary>
        /// The constructor for this selector.
        /// </summary>
        /// <param name="templates">The templates available for selection.</param>
        public MethodTemplateSelector(IEnumerable<MethodTemplate> templates)
        {
            _Templates = templates ?? throw new ArgumentNullException(nameof(templates));
        }

        /// <inheritdoc />
        public MethodTemplate Select(string methodName, string returnType, string[] paramTypes, bool isAsync)
        {
            var ordered = _Templates
                .Where(template => IsReturnMatch(template, returnType) && IsParamMatch(template, paramTypes) && template.Async == isAsync)
                .OrderByDescending(item => item.MatchCondition, new MethodMatchConditionComparer());

            return ordered.FirstOrDefault(template => template.Name == methodName) ?? ordered.FirstOrDefault();
        }

        private bool IsReturnMatch(MethodTemplate template, string returnType)
        {
            MethodTemplateMatchCondition condition = template.MatchCondition;

            return condition.ReturnTypeRule switch
            {
                ReturnTypeRule.Any => true,
                ReturnTypeRule.Specified => condition.ReturnType == returnType,
                _ => throw new NotImplementedException(condition.ReturnTypeRule.ToString())
            };
        }

        private bool IsParamMatch(MethodTemplate template, string[] paramTypes)
        {
            MethodTemplateMatchCondition condition = template.MatchCondition;

            return condition.ParamTypeRule switch
            {
                ParamTypeRule.Any => true,
                ParamTypeRule.Specified => paramTypes.SequenceEqual(condition.Params.Keys, StringComparer.Ordinal),
                _ => throw new NotImplementedException(condition.ParamTypeRule.ToString())
            };
        }
    }

}
