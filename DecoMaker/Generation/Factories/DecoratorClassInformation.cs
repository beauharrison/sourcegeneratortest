using DecoMaker.Templating;
using System;
using System.Collections.Generic;

namespace DecoMaker.Generation
{
    internal class DecoratorClassInformation
    {
        public DecoratorClassInformation(
            IEnumerable<string> genericTypes, 
            IDictionary<string, string> typeConstraints,
            ClassTemplate template, 
            string decoratedType,
            string derviedFrom,
            IEnumerable<DecoratorMethodInformation> methodInformation, 
            IEnumerable<DecoratorPropertyInformation> propertyInformation)
        {
            GenericTypes = genericTypes;
            TypeConstraints = typeConstraints ?? throw new ArgumentNullException(nameof(typeConstraints));
            Template = template ?? throw new ArgumentNullException(nameof(template));
            DecoratedType = decoratedType ?? throw new ArgumentNullException(nameof(decoratedType));
            DerviedFrom = derviedFrom;
            MethodInformation = methodInformation ?? throw new ArgumentNullException(nameof(methodInformation));
            PropertyInformation = propertyInformation ?? throw new ArgumentNullException(nameof(propertyInformation));

            foreach (var methodInfo in MethodInformation)
            {
                methodInfo.SetParent(this);
            }

            foreach (var propertyInfo in PropertyInformation)
            {
                propertyInfo.SetParent(this);
            }
        }

        public DecoratorNamespaceInformation ParentInformation { get; private set; }

        public IEnumerable<string> GenericTypes { get; }

        public IDictionary<string, string> TypeConstraints { get; }

        public ClassTemplate Template { get; }

        public string DecoratedType { get; }

        public string DerviedFrom { get; }

        public IEnumerable<DecoratorMethodInformation> MethodInformation { get; }

        public IEnumerable<DecoratorPropertyInformation> PropertyInformation { get; }

        public void SetParent(DecoratorNamespaceInformation parentInfo)
        {
            if (ParentInformation == null) 
                ParentInformation = parentInfo;
        }
    }
}
