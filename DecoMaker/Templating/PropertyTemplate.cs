namespace DecoMaker.Templating
{
    internal class PropertyTemplate
    {
        /// <summary>
        /// The name of the property template.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The match condition for this template.
        /// </summary>
        public PropertyTemplateMatchCondition MatchCondition { get; set; }

        /// <summary>
        /// The getter body of the property template.
        /// </summary>
        public string GetterBody { get; set; }

        /// <summary>
        /// The setter body of the property template.
        /// </summary>
        public string SetterBody { get; set; }
    }
}
