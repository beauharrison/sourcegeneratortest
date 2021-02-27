using System;
using System.Collections.Generic;
using System.Linq;

namespace DecoMaker
{
    internal class MethodTemplateSelector
    {
        private readonly IEnumerable<MethodTemplate> _Templates;

        public MethodTemplateSelector(IEnumerable<MethodTemplate> templates)
        {
            _Templates = templates ?? throw new ArgumentNullException(nameof(templates));
        }

        public MethodTemplate Pick(string returnType, string[] paramTypes)
        {
            var ordered = _Templates
                .Where(template => IsReturnMatch(template, returnType) && IsParamMatch(template, paramTypes))
                .OrderByDescending(item => item.MatchOnCondition, new MethodMatchOnConditionComparer());

            return ordered.FirstOrDefault();
        }

        private bool IsReturnMatch(MethodTemplate template, string returnType)
        {
            MethodMatchOnCondition condition = template.MatchOnCondition;

            return condition.ReturnTypeMatchOn switch
            {
                ReturnTypeMatchOn.Any => true,
                ReturnTypeMatchOn.Specified => condition.ReturnType == returnType,
                _ => throw new NotImplementedException(condition.ReturnTypeMatchOn.ToString())
            };
        }

        private bool IsParamMatch(MethodTemplate template, string[] paramTypes)
        {
            MethodMatchOnCondition condition = template.MatchOnCondition;

            return condition.ParamTypeMatchOn switch
            {
                ParamTypeMatchOn.Any => true,
                ParamTypeMatchOn.Specified => paramTypes.SequenceEqual(condition.Params.Keys, StringComparer.Ordinal),
                _ => throw new NotImplementedException(condition.ParamTypeMatchOn.ToString())
            };
        }
    }

}
