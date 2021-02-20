using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Generators
{
    internal class StaticMethodCall
    {
        public InvocationExpressionSyntax Invocation { get; set; }

        public string Method { get; set; }

        public ExpressionSyntax[] Arguments { get; set; }

        public TypeSyntax[] GenericTypes { get; set; }
    }
}
