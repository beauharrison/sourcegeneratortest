using CodeGen;
using DecoMaker.Templating;
using System.Collections.Generic;
using System.Linq;

namespace DecoMaker.Generation
{
    internal class DecoratorExtensionClassFactory : IDecoratorFactory<DecoratorClassInformation, CodeGenClass>
    {
        public CodeGenClass Create(DecoratorClassInformation factoryInformation)
        {
            var generatedExtensionClass = new CodeGenClass(
                $"{factoryInformation.ParentInformation.DecoratorName}Extensions",
                Scope.Public,
                ClassType.Static);

            IEnumerable<CodeGenGeneric> genericTypes = factoryInformation.GenericTypes?.Select(
                parameter => FactoryHelpers.GenerateMethodParameter(parameter, factoryInformation.TypeConstraints));

            var extensionParams = new List<string>(new[] { $"this {factoryInformation.DecoratedType} decorated" });
            var decoratedConstructorParams = new List<string>(new[] { "decorated" });

            if (factoryInformation.Template.ConstructorParams != null)
            {
                foreach (ConstructorParam constructorParam in factoryInformation.Template.ConstructorParams)
                {
                    extensionParams.Add($"{constructorParam.Type} {constructorParam.Name}");
                    decoratedConstructorParams.Add(constructorParam.Name);
                }
            }

            string genericTypesString = factoryInformation.GenericTypes.ToTypeParamList();
            string paramNames = string.Join(", ", decoratedConstructorParams);
            string body = $"return new {factoryInformation.ParentInformation.DecoratorName}{genericTypesString}({paramNames});";

            string returnType = factoryInformation.DerviedFrom != null 
                ? factoryInformation.DerviedFrom
                : $"{factoryInformation.ParentInformation.DecoratorName}{genericTypesString}";

            var generatedExtensionMethod = new CodeGenMethod(
                $"DecorateWith{factoryInformation.Template.Label}",
                returnType,
                Scope.Public,
                MethodType.Static,
                genericTypes,
                extensionParams,
                body);

            generatedExtensionClass.Methods.Add(generatedExtensionMethod);
            return generatedExtensionClass;
        }
    }
}
