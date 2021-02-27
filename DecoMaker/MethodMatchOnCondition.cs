namespace DecoMaker
{
    internal class MethodMatchOnCondition
    {
        public ReturnTypeMatchOn ReturnTypeMatchOn { get; set; }

        public bool Async { get; set; }

        public string ReturnType { get; set; }

        public ParamTypeMatchOn ParamTypeMatchOn { get; set; }

        public (string Type, string Name)[] Params { get; set; }
    }
}
