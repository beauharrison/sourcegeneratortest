using System.Collections.Generic;

namespace Generators.DI
{
    public class DIRegistration
    {
        public DIRegistrationType Type { get; set; }

        public string IdentifierType { get; set; }

        public string ImplementationType { get; set; }

        public string DirectGetMethodName { get; set; }

        public IEnumerable<string> ConstructorParamTypes { get; set; }
    }
}
