using System.Collections.Generic;

namespace DecoMaker.Templating
{
    internal class MethodMatchConditionComparer : IComparer<MethodTemplateMatchCondition>
    {
        public int Compare(MethodTemplateMatchCondition x, MethodTemplateMatchCondition y)
        {
            if (x.ReturnTypeRule != ReturnTypeRule.Any && y.ReturnTypeRule == ReturnTypeRule.Any)
                return 1;

            if (x.ReturnTypeRule == ReturnTypeRule.Any && y.ReturnTypeRule != ReturnTypeRule.Any)
                return -1;

            if (x.ParamTypeRule != ParamTypeRule.Any && y.ParamTypeRule == ParamTypeRule.Any)
                return 1;

            if (x.ParamTypeRule == ParamTypeRule.Any && y.ParamTypeRule != ParamTypeRule.Any)
                return -1;

            return 0;
        }
    }
}
