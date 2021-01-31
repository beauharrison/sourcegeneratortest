using CodeGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Generators.Learning
{
    public static class GeneratorExtensions
    {
        public static void Add(this GeneratorExecutionContext context, ICodeGenElement codeElement, string fileName)
        {
            string generatedCodeString = codeElement.GenerateCode();

            var sourceText = SourceText.From(generatedCodeString, Encoding.UTF8);
            context.AddSource(fileName, sourceText);
        }

        public static string GetFQName(this TypeInfo typeInfo)
        {
            var symbolDisplayFormat = new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

            return typeInfo.Type.ToDisplayString(symbolDisplayFormat);
        }

    }
}
