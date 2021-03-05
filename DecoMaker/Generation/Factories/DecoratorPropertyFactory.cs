﻿using CodeGen;
using DecoMaker.Templating;
using System;

namespace DecoMaker.Generation
{
    internal class DecoratorPropertyFactory : IDecoratorFactory<DecoratorPropertyInformation, CodeGenProperty>
    {
        private readonly IPropertyTemplateSelector _PropertyTemplateSelector;

        public DecoratorPropertyFactory(IPropertyTemplateSelector propertyTemplateSelector)
        {
            _PropertyTemplateSelector = propertyTemplateSelector ?? throw new ArgumentNullException(nameof(propertyTemplateSelector));
        }

        public CodeGenProperty Create(DecoratorPropertyInformation factoryInformation)
        {
            PropertyTemplate matchingTemplate = _PropertyTemplateSelector.Select(
                factoryInformation.PropertyName,
                factoryInformation.PropertyType,
                factoryInformation.HasSetter);

            string getterBody = matchingTemplate != null
                ? GenerateDecoratorPropertyGetterBodyFromCondition(factoryInformation.PropertyName, matchingTemplate)
                : GenerateUndecoratedPropertyGetterBody(factoryInformation.PropertyName);

            string setterBody = factoryInformation.HasSetter
                ? matchingTemplate != null
                    ? GenerateDecoratorPropertySetterBodyFromCondition(factoryInformation.PropertyName, matchingTemplate)
                    : GenerateUndecoratedPropertySetterBody(factoryInformation.PropertyName)
                : null;

            return new CodeGenProperty(
                factoryInformation.PropertyName,
                factoryInformation.PropertyType,
                Scope.Public,
                getterBody,
                setterBody);
        }

        private string GenerateDecoratorPropertyGetterBodyFromCondition(string propertyName, PropertyTemplate template)
        {
            string body = template.GetterBody.Clone() as string;
            body = body.Replace("Decorated.Property.Any.Value", $"_Decorated.{propertyName}");

            return FactoryHelpers.CleanBody(body);
        }

        private string GenerateUndecoratedPropertyGetterBody(string propertyName)
        {
            return $"return _Decorated.{propertyName};";
        }

        private string GenerateDecoratorPropertySetterBodyFromCondition(string propertyName, PropertyTemplate template)
        {
            string body = template.SetterBody.Clone() as string;
            body = body.Replace("Decorated.Property.Any.Value", $"_Decorated.{propertyName}");

            return FactoryHelpers.CleanBody(body);
        }

        private string GenerateUndecoratedPropertySetterBody(string propertyName)
        {
            return $"_Decorated.{propertyName} = value;";
        }
    }
}
