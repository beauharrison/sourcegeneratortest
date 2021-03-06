using DecoMaker.Templating;
using System;
using System.Collections.Generic;

namespace DecoMaker.Generation
{
    internal class DecoratorPropertyInformation
    {
        public DecoratorPropertyInformation(string propertyName, string propertyType, bool hasSetter)
        {
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            PropertyType = propertyType ?? throw new ArgumentNullException(nameof(propertyType));
            HasSetter = hasSetter;
        }

        public DecoratorClassInformation ParentInformation { get; private set; }

        public string PropertyName { get; }

        public string PropertyType { get; }

        public bool HasSetter { get; }

        public IEnumerable<PropertyTemplate> Templates => ParentInformation.Template.PropertyTemplates;
        
        public void SetParent(DecoratorClassInformation parentInfo)
        {
            if (ParentInformation == null)
                ParentInformation = parentInfo;
        }
    }
}
