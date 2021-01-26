using CodeGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Generators
{
    [Generator]
    public class FirstGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var @class = new CodeGenClass("GeneratedClass", Scope.Public, ClassType.Normal, comment: "this is auto generated!");

            @class.Methods.Add(new CodeGenMethod("GeneratedMethod", null, Scope.Public, MethodType.Static, null, null, @"System.Console.WriteLine(""Hello World 123"");"));

            var @namespace = new CodeGenNamespace("GeneratedNamespace");
            @namespace.Content.Add(@class);

            string codeString = @namespace.GenerateCode();

//            string codeString = @"
//namespace GeneratedNamespace
//{
//    public class GeneratedClass
//    {
//        public static void GeneratedMethod()
//        {
//            System.Console.WriteLine(""Hello World once more"");
//        }
//    }
//}
//";

            context.AddSource("myGeneratedFile.cs", SourceText.From(codeString, Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
