using System.Collections.Generic;

namespace DecoMaker.Templating
{
    /// <summary>
    /// Factory for creating <see cref="IPropertyTemplateSelector"/>.
    /// </summary>
    internal interface IPropertyTemplateSelectorFactory
    {
        /// <summary>
        /// Create the <see cref="IPropertyTemplateSelector"/>
        /// </summary>
        /// <param name="propertyTemplates">The property templates.</param>
        /// <returns>The selector.</returns>
        IPropertyTemplateSelector Create(IEnumerable<PropertyTemplate> propertyTemplates);
    }

}
