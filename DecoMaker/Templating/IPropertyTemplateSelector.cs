namespace DecoMaker.Templating
{
    /// <summary>
    /// Selects the best property template for a property being decorated.
    /// </summary>
    internal interface IPropertyTemplateSelector
    {
        /// <summary>
        /// Select the best property template for a property.
        /// </summary>
        /// <param name="propertyName">The name of the property being decorated.</param>
        /// <param name="propertyType">The type of the property.</param>
        /// <param name="hasSetter">If the property has a setter.</param>
        /// <returns>The selected best method template, or null if there was none.</returns>
        PropertyTemplate Select(string propertyName, string propertyType, bool hasSetter);
    }

}
