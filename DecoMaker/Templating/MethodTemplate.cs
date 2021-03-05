namespace DecoMaker.Templating
{
    /// <summary>
    /// Template of a method for use in a decorator.
    /// </summary>
    internal class MethodTemplate
    {
        /// <summary>
        /// The name of the method template.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// If the generated decorator method should be called asynchronously.
        /// </summary>
        public bool Async { get; set; }

        /// <summary>
        /// The match condition for this template.
        /// </summary>
        public MethodTemplateMatchCondition MatchCondition { get; set; }

        /// <summary>
        /// The body of the method template.
        /// </summary>
        public string Body { get; set; }
    }
}
