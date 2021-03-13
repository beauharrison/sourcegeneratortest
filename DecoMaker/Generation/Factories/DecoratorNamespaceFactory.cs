using CodeGen;
using System;

namespace DecoMaker.Generation
{
    internal class DecoratorNamespaceFactory : IDecoratorFactory<DecoratorNamespaceInformation, CodeGenNamespace>
    {
        private readonly IDecoratorFactory<DecoratorClassInformation, CodeGenClass> _ClassFactory;
        private readonly IDecoratorFactory<DecoratorClassInformation, CodeGenClass> _ExtensionFactory;

        public DecoratorNamespaceFactory(
            IDecoratorFactory<DecoratorClassInformation, CodeGenClass> classFactory,
            IDecoratorFactory<DecoratorClassInformation, CodeGenClass> extensionFactory)
        {
            _ClassFactory = classFactory ?? throw new ArgumentNullException(nameof(classFactory));
            _ExtensionFactory = extensionFactory ?? throw new ArgumentNullException(nameof(extensionFactory));
        }

        public CodeGenNamespace Create(DecoratorNamespaceInformation factoryInformation)
        {
            var generatedNamespace = new CodeGenNamespace($"{factoryInformation.DecoratedClassNamespace}.Decorators");
            
            generatedNamespace.Content.Add(_ClassFactory.Create(factoryInformation.ClassInformaiton));
            generatedNamespace.Content.Add(_ExtensionFactory.Create(factoryInformation.ClassInformaiton));
            generatedNamespace.Usings.Add("System");

            return generatedNamespace;
        }
    }
}
