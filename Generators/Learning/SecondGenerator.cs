using CodeGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Generators.Learning
{
    [Generator]
    public class SecondGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var generatedClass = new CodeGenClass("XmlPrinter", Scope.Public, ClassType.Static);

            var xmlFiles = context.AdditionalFiles.Where(f => f.Path.EndsWith(".xml"));

            foreach (var xmlFile in xmlFiles)
            {
                var itemElements = XDocument.Load(xmlFile.Path)?.Root?.Elements("item");

                if (itemElements == null) continue;

                foreach (var itemElement in itemElements)
                {
                    string name = itemElement.Attribute("name")?.Value;
                    string value = itemElement.Value;

                    if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(value))
                    {
                        var generatedMethod = new CodeGenMethod(
                            $"Print{name}",
                            null,
                            Scope.Public,
                            MethodType.Static,
                            null,
                            null,
                            $@"
Console.WriteLine($""Hello, {value}"");
");

                        generatedClass.Methods.Add(generatedMethod);
                    }
                }
            }

            var generatedNamespace = new CodeGenNamespace("GeneratedNamespace");
            generatedNamespace.Usings.Add("System");
            generatedNamespace.Content.Add(generatedClass);

            var generatedCodeString = generatedNamespace.GenerateCode();

            var sourceText = SourceText.From(generatedCodeString, Encoding.UTF8);
            context.AddSource("XmlPrinter.cs", sourceText);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            //if (!Debugger.IsAttached)
            //{
            //    Debugger.Launch();
            //}
#endif 
        }
    }
}
