namespace DecoMaker
{
    internal class TemplateMethod
    {
        public string Name { get; set; }

        public MethodMatchOnCondition MatchOnCondition { get; set; }

        public string Body { get; set; }
    }

    internal class ClassTemplate
    {
        public string Label { get; set; }

        public string ImplementationType { get; set; }

        public TemplateMethod[] TemplateMethods { get; set; }
        
        public TemplateProperty[] TemplateProperties { get; set; }
    }
}
