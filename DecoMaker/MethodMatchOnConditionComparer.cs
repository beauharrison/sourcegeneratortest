using System.Collections.Generic;

namespace DecoMaker
{
    internal class MethodMatchOnConditionComparer : IComparer<MethodMatchOnCondition>
    {
        public int Compare(MethodMatchOnCondition x, MethodMatchOnCondition y)
        {
            if (x.ReturnTypeMatchOn != ReturnTypeMatchOn.Any && y.ReturnTypeMatchOn == ReturnTypeMatchOn.Any)
                return 1;

            if (x.ReturnTypeMatchOn == ReturnTypeMatchOn.Any && y.ReturnTypeMatchOn != ReturnTypeMatchOn.Any)
                return -1;

            if (x.ParamTypeMatchOn != ParamTypeMatchOn.Any && y.ParamTypeMatchOn == ParamTypeMatchOn.Any)
                return 1;

            if (x.ParamTypeMatchOn == ParamTypeMatchOn.Any && y.ParamTypeMatchOn != ParamTypeMatchOn.Any)
                return -1;

            return 0;
        }
    }

}
