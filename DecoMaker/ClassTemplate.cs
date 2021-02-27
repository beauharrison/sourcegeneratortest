namespace DecoMaker
{
    internal class ClassTemplate
    {
        public string Label { get; set; }

        public string ImplementationType { get; set; }

        public MethodTemplate[] TemplateMethods { get; set; }
        
        public TemplateProperty[] TemplateProperties { get; set; }


    }
}
