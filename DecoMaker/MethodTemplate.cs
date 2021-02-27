namespace DecoMaker
{
    internal class MethodTemplate
    {
        public string Name { get; set; }

        public bool Async { get; set; }

        public MethodMatchOnCondition MatchOnCondition { get; set; }

        public string Body { get; set; }
    }
}
