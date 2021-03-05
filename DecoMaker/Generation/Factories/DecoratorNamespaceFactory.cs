using CodeGen;
using System;

namespace DecoMaker.Generation
{
    internal class DecoratorNamespaceFactory : IDecoratorFactory<DecoratorNamespaceInformation, CodeGenNamespace>
    {
        private readonly IDecoratorFactory<DecoratorClassInformation, CodeGenClass> _ClassFactory;

        public DecoratorNamespaceFactory(IDecoratorFactory<DecoratorClassInformation, CodeGenClass> classFactory)
        {
            _ClassFactory = classFactory ?? throw new ArgumentNullException(nameof(classFactory));
        }

        public CodeGenNamespace Create(DecoratorNamespaceInformation factoryInformation)
        {
            var generatedNamespace = new CodeGenNamespace($"{factoryInformation.DecoratedClassNamespace}.Decorators");
            
            generatedNamespace.Content.Add(_ClassFactory.Create(factoryInformation.ClassInformaiton));
            generatedNamespace.Usings.Add("System");

            return generatedNamespace;
        }
    }
}
