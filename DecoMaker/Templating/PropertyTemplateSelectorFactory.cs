using System.Collections.Generic;

namespace DecoMaker.Templating
{
    /// <summary>
    /// Factory for creating <see cref="PropertyTemplateSelector"/>.
    /// </summary>
    internal class PropertyTemplateSelectorFactory : IPropertyTemplateSelectorFactory
    {
        /// <inheritdoc />
        public IPropertyTemplateSelector Create(IEnumerable<PropertyTemplate> propertyTemplates)
        {
            return new PropertyTemplateSelector(propertyTemplates);
        }
    }
}
