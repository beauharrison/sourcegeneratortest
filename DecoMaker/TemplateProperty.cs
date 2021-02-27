namespace DecoMaker
{
    internal class TemplateProperty
    {
        public string Name { get; set; }

        public PropertyMatchOnCondition MatchOnCondition { get; set; }

        public string GetterBody { get; set; }

        public string SetterBody { get; set; }
    }
}
