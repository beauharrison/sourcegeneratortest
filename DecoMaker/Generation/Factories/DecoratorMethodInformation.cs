using System.Collections.Generic;

namespace DecoMaker.Generation
{
    internal class DecoratorMethodInformation
    {
        public DecoratorClassInformation ParentInformation { get; }

        public string MethodName { get; }

        public IEnumerable<string> GenericTypes { get; }

        public IDictionary<string, string> TypeConstraints { get; }

        public string ReturnType { get; }

        public bool IsAsync { get; }

        public IEnumerable<MethodParameter> Parameters { get; }
    }

    internal class MethodParameter
    {
        public string Type { get; }

        public string Name { get; }
    }
}
