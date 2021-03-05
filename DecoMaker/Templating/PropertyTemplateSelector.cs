using System;
using System.Collections.Generic;
using System.Linq;

namespace DecoMaker.Templating
{
    /// <summary>
    /// Selects the best property template for a property being decorated.
    /// </summary>
    internal class PropertyTemplateSelector : IPropertyTemplateSelector
    {
        private readonly IEnumerable<PropertyTemplate> _Templates;

        /// <summary>
        /// The constructor for this selector.
        /// </summary>
        /// <param name="templates">The templates available for selection.</param>
        public PropertyTemplateSelector(IEnumerable<PropertyTemplate> templates)
        {
            _Templates = templates ?? throw new ArgumentNullException(nameof(templates));
        }

        /// <inheritdoc />
        public PropertyTemplate Select(string propertyName, string propertyType, bool hasSetter)
        {
            var ordered = _Templates
                .Where(template => IsTypeMatch(template, propertyType) && (!hasSetter || !string.IsNullOrWhiteSpace(template.SetterBody)))
                .OrderByDescending(item => item.MatchCondition, new PropertyMatchConditionComparer());

            return ordered.FirstOrDefault(template => template.Name == propertyName) ?? ordered.FirstOrDefault();
        }

        private bool IsTypeMatch(PropertyTemplate template, string propertyType)
        {
            PropertyTemplateMatchCondition condition = template.MatchCondition;

            return condition.PropertyTypeRule switch
            {
                PropertyTypeRule.Any => true,
                PropertyTypeRule.Specified => condition.Type == propertyType,
                _ => throw new NotImplementedException(condition.PropertyTypeRule.ToString())
            };
        }
    }

}
