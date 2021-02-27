using System.Collections.Generic;

namespace DecoMaker
{
    internal class MethodMatchOnCondition
    {
        public ReturnTypeMatchOn ReturnTypeMatchOn { get; set; }

        public string ReturnType { get; set; }

        public ParamTypeMatchOn ParamTypeMatchOn { get; set; }

        public IDictionary<string, string> Params { get; set; }
    }
}
