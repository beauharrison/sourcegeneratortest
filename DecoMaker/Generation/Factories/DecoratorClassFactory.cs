using CodeGen;
using DecoMaker.Templating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DecoMaker.Generation
{
    internal class DecoratorClassFactory : IDecoratorFactory<DecoratorClassInformation, CodeGenClass>
    {
        private readonly IDecoratorFactory<DecoratorMethodInformation, CodeGenMethod> _MethodFactory;
        private readonly IDecoratorFactory<DecoratorPropertyInformation, CodeGenProperty> _PropertyFactory;

        public DecoratorClassFactory(
            IDecoratorFactory<DecoratorMethodInformation, CodeGenMethod> methodFactory,
            IDecoratorFactory<DecoratorPropertyInformation, CodeGenProperty> propertyFactory)
        {
            _MethodFactory = methodFactory ?? throw new ArgumentNullException(nameof(methodFactory));
            _PropertyFactory = propertyFactory ?? throw new ArgumentNullException(nameof(propertyFactory));
        }

        public CodeGenClass Create(DecoratorClassInformation factoryInformation)
        {
            IEnumerable<CodeGenGeneric> genericTypes = factoryInformation.GenericTypes?.Select(
                parameter => FactoryHelpers.GenerateMethodParameter(parameter, factoryInformation.TypeConstraints));

            string[] derivedFrom = !string.IsNullOrEmpty(factoryInformation.DerviedFrom) 
                ? new[] { factoryInformation.DerviedFrom } 
                : null;

            var generatedDecoratorClass = new CodeGenClass(
                factoryInformation.ParentInformation.DecoratorName,
                Scope.Public,
                ClassType.Normal,
                genericTypes,
                derivedFrom);

            generatedDecoratorClass.Comment = new CodeGenComment($@"Generated {factoryInformation.Template.Label} decorator for the {factoryInformation.DecoratedType } type. Auto-generated on {DateTimeOffset.Now}");

            var constructorParams = new List<string>(new[] { $"{factoryInformation.DecoratedType} decorated" });
            var constructorBodyBuilder = new StringBuilder();

            constructorBodyBuilder.AppendLine("_Decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));");

            if (factoryInformation.Template.ConstructorParams != null)
            {
                foreach (ConstructorParam constructorParam in factoryInformation.Template.ConstructorParams)
                {
                    constructorParams.Add($"{constructorParam.Type} {constructorParam.Name}");
                    constructorBodyBuilder.AppendLine($"_{constructorParam.Name} = {constructorParam.Name};");

                    generatedDecoratorClass.Variables.Add(new CodeGenVariable(
                        $"_{constructorParam.Name}",
                        constructorParam.Type,
                        Scope.Private,
                        readOnly: true));
                }
            }

            // Constructor
            generatedDecoratorClass.Constructors.Add(new CodeGenConstructor(
                factoryInformation.ParentInformation.DecoratorName,
                Scope.Public,
                constructorParams,
                constructorBodyBuilder.ToString()));

            // Decorated variable
            generatedDecoratorClass.Variables.Add(new CodeGenVariable(
                "_Decorated",
                factoryInformation.DecoratedType,
                Scope.Private,
                readOnly: true));

            foreach (DecoratorMethodInformation methodInformation in factoryInformation.MethodInformation)
            {
                CodeGenMethod method = _MethodFactory.Create(methodInformation);
                generatedDecoratorClass.Methods.Add(method);
            }

            foreach (DecoratorPropertyInformation propertyInformation in factoryInformation.PropertyInformation)
            {
                CodeGenProperty property = _PropertyFactory.Create(propertyInformation);
                generatedDecoratorClass.Properties.Add(property);
            }

            return generatedDecoratorClass;
        }
    }
}
