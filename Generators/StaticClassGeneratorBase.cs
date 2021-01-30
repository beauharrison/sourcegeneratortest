using CodeGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Text;

namespace Generators
{
    public abstract class StaticClassGeneratorBase : ISourceGenerator
    {
        private readonly string _Namespace;
        private readonly string _ClassName;
        private readonly string _ClassDescription;

        public StaticClassGeneratorBase(string @namespace, string className, string classDescription)
        {
            _Namespace = @namespace;
            _ClassName = className;
            _ClassDescription = classDescription;
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var generatedClass = new CodeGenClass(
                _ClassName, 
                Scope.Public, 
                ClassType.Static);

            generatedClass.Comment = new CodeGenComment($@"Class: {_ClassName}
Description: {_ClassDescription}
Auto-generated on {DateTime.Now}");

            GenerateClassMethods(context, generatedClass);

            var generatedNamespace = new CodeGenNamespace(_Namespace);
            generatedNamespace.Content.Add(generatedClass);

            ModifyNamespace(context, generatedNamespace);

            var generatedCodeString = generatedNamespace.GenerateCode();

            var sourceText = SourceText.From(generatedCodeString, Encoding.UTF8);
            context.AddSource($"{_ClassName}.cs", sourceText);
        }

        public abstract void Initialize(GeneratorInitializationContext context);

        protected abstract void GenerateClassMethods(GeneratorExecutionContext context, CodeGenClass @class);

        protected virtual void ModifyNamespace(GeneratorExecutionContext context, CodeGenNamespace @namespace)
        {
        }
    }
}
