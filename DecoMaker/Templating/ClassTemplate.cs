using System;

namespace DecoMaker.Templating
{
    /// <summary>
    /// Template of a class used when generating decorators.
    /// </summary>
    internal class ClassTemplate
    {
        /// <summary>
        /// The label used in the decorator name.
        /// </summary>
        public string Label { get; set; }

        public ConstructorParam[] ConstructorParams { get; set; }

        /// <summary>
        /// Templates for methods in the decorator.
        /// </summary>
        public MethodTemplate[] MethodTemplates { get; set; }

        /// <summary>
        /// Templates for properties in the decorator.
        /// </summary>
        public PropertyTemplate[] PropertyTemplates { get; set; }
    }
}
