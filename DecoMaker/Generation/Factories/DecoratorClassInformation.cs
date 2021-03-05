using DecoMaker.Templating;
using System.Collections.Generic;

namespace DecoMaker.Generation
{
    internal class DecoratorClassInformation
    {
        public DecoratorNamespaceInformation ParentInformation { get; }

        public IEnumerable<string> GenericTypes { get; }

        public IDictionary<string, string> TypeConstraints { get; }

        public string DerviedFrom { get; }

        public ClassTemplate Template { get; }

        public string DecoratedTypeName { get; }

        public IEnumerable<DecoratorMethodInformation> MethodInformation { get; }

        public IEnumerable<DecoratorPropertyInformation> PropertyInformation { get; }
    }
}
