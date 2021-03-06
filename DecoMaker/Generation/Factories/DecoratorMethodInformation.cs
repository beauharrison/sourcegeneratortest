using DecoMaker.Templating;
using System;
using System.Collections.Generic;

namespace DecoMaker.Generation
{
    internal class DecoratorMethodInformation
    {
        public DecoratorMethodInformation(
            string methodName, 
            IEnumerable<string> genericTypes, 
            IDictionary<string, string> typeConstraints, 
            string returnType, 
            bool isAsync, 
            IEnumerable<MethodParameter> parameters)
        {
            MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
            GenericTypes = genericTypes;
            TypeConstraints = typeConstraints;
            ReturnType = returnType;
            IsAsync = isAsync;
            Parameters = parameters;
        }

        public DecoratorClassInformation ParentInformation { get; private set; }

        public string MethodName { get; }

        public IEnumerable<string> GenericTypes { get; }

        public IDictionary<string, string> TypeConstraints { get; }

        public string ReturnType { get; }

        public bool IsAsync { get; }

        public IEnumerable<MethodParameter> Parameters { get; }

        public IEnumerable<MethodTemplate> Templates => ParentInformation.Template.MethodTemplates;

        public void SetParent(DecoratorClassInformation parentInfo)
        {
            if (ParentInformation == null)
                ParentInformation = parentInfo;
        }
    }

    internal class MethodParameter
    {
        public MethodParameter(string type, string name)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public string Type { get; }

        public string Name { get; }
    }
}
