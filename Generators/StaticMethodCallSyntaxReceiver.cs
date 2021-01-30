using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Generators
{
    internal class StaticMethodCallSyntaxReceiver : ISyntaxReceiver
    {
        private readonly string _Namespace;
        private readonly string _Class;
        private readonly string _Method;

        public StaticMethodCallSyntaxReceiver(string @namespace, string @class, string method)
        {
            _Namespace = @namespace;
            _Class = @class;
            _Method = method;
        }

        public List<(InvocationExpressionSyntax Invocation, ExpressionSyntax[] Args)> Calls 
            = new List<(InvocationExpressionSyntax Invocation, ExpressionSyntax[])>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is InvocationExpressionSyntax invocation &&
                invocation.Expression is MemberAccessExpressionSyntax methodMemberAccess)
            {
                ArgumentSyntax[] arguments = invocation.ArgumentList.Arguments.ToArray();
                SyntaxNode[] children = methodMemberAccess.ChildNodes().ToArray();

                if (children.Length == 2 &&
                    children[0] is MemberAccessExpressionSyntax classMemberAccess &&
                    children[1] is IdentifierNameSyntax methodIdentifier &&
                    classMemberAccess.Expression is IdentifierNameSyntax @namespace)
                {
                    string methodName = methodIdentifier.Identifier.ValueText;
                    string className = classMemberAccess.Name.Identifier.ValueText;
                    string namespaceName = @namespace.Identifier.ValueText;

                    if (string.Equals(namespaceName, _Namespace) && string.Equals(className, _Class) && string.Equals(methodName, _Method))
                    {
                        Calls.Add((
                            invocation,
                            arguments.Select(argEx => argEx.Expression).ToArray()
                        ));
                    }
                }
            }
        }
    }
}
