using System;

namespace DecoMaker.Generation
{
    internal class DecoratorNamespaceInformation
    {
        public DecoratorNamespaceInformation(
            string decoratedClassNamespace, 
            string decoratorName, 
            DecoratorClassInformation classInformaiton)
        {
            DecoratedClassNamespace = decoratedClassNamespace ?? throw new ArgumentNullException(nameof(decoratedClassNamespace));
            DecoratorName = decoratorName ?? throw new ArgumentNullException(nameof(decoratorName));
            ClassInformaiton = classInformaiton ?? throw new ArgumentNullException(nameof(classInformaiton));

            ClassInformaiton.SetParent(this);
        }

        public string DecoratedClassNamespace { get; }

        public string DecoratorName { get; }

        public DecoratorClassInformation ClassInformaiton { get; }
    }
}
