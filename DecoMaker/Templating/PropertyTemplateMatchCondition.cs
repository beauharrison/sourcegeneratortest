namespace DecoMaker.Templating
{
    /// <summary>
    /// Condition to be satisfied by a property being decorated when determing the tempmlate to use to generate that properties decorator.
    /// </summary>
    internal class PropertyTemplateMatchCondition
    {        
        /// <summary>
        /// The rule for the condition regarding the property type.
        /// </summary>
        public PropertyTypeRule PropertyTypeRule { get; set; }

        /// <summary>
        /// The return type for when <see cref="PropertyTypeRule"/> is <see cref="PropertyTypeRule.Specified"/>.
        /// </summary>
        public string Type { get; set; }
    }
}
