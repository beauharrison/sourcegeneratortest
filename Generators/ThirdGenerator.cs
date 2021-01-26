using CodeGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Generators
{

    [Generator]
    public class ThirdGenerator : StaticClassGeneratorBase
    {
        private const string NamespaceName = "GeneratedNamespace";
        private const string ClassName = "Conditional";
        private const string MethodName = "DoSomething";

        public ThirdGenerator() : base(NamespaceName, ClassName, "Conditional generation")
        {
        }

        public override void Initialize(GeneratorInitializationContext context)
        {
            //#if DEBUG
            //            if (!Debugger.IsAttached)
            //            {
            //                Debugger.Launch();
            //            }
            //#endif

            context.RegisterForSyntaxNotifications(() => new MySyntaxReceiver(NamespaceName, ClassName, MethodName));
        }

        protected override void GenerateClassMethods(GeneratorExecutionContext context, CodeGenClass @class)
        {
            MySyntaxReceiver syntaxReceiver = (MySyntaxReceiver) context.SyntaxReceiver;

            var foundTypes = new HashSet<string>();

            if (!syntaxReceiver.ArgumentsToGenerateFor.Any())
            {
                @class.Methods.Add(new CodeGenMethod(
                    MethodName,
                    "object",
                    Scope.Public,
                    MethodType.Static,
                    null,
                    new[] { "object arg" },
                    @"// Stub method for intellisence.
return arg;"));

                return;
            }

            foreach (ExpressionSyntax argExpression in syntaxReceiver.ArgumentsToGenerateFor)
            {
                SemanticModel semanticModel = context.Compilation.GetSemanticModel(argExpression.SyntaxTree);
                string argumentType = semanticModel.GetTypeInfo(argExpression).Type.ToString();

                if (argumentType == null || foundTypes.Contains(argumentType))
                    continue;

                foundTypes.Add(argumentType);

                @class.Methods.Add(new CodeGenMethod(
                    MethodName,
                    argumentType.ToString(),
                    Scope.Public,
                    MethodType.Static,
                    null,
                    new[] { $"{argumentType} arg" },
                    "return arg;"));
            }
        }
    }

    internal class MySyntaxReceiver : ISyntaxReceiver
    {
        private readonly string _Namespace;
        private readonly string _Class;
        private readonly string _Method;

        public MySyntaxReceiver(string @namespace, string @class, string method)
        {
            _Namespace = @namespace;
            _Class = @class;
            _Method = method;
        }

        public List<ExpressionSyntax> ArgumentsToGenerateFor = new List<ExpressionSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is InvocationExpressionSyntax invocation &&
                invocation.Expression is MemberAccessExpressionSyntax methodMemberAccess)
            {
                ArgumentSyntax[] arguments = invocation.ArgumentList.Arguments.ToArray();
                SyntaxNode[] children = methodMemberAccess.ChildNodes().ToArray();

                if (children.Length == 2 &&
                    arguments.Length == 1 &&
                    children[0] is MemberAccessExpressionSyntax classMemberAccess &&
                    children[1] is IdentifierNameSyntax methodIdentifier &&
                    classMemberAccess.Expression is IdentifierNameSyntax @namespace)
                {
                    string methodName = methodIdentifier.Identifier.ValueText;
                    string className = classMemberAccess.Name.Identifier.ValueText;
                    string namespaceName = @namespace.Identifier.ValueText;

                    if (string.Equals(namespaceName, _Namespace) && string.Equals(className, _Class) && string.Equals(methodName, _Method))
                    {
                        ArgumentsToGenerateFor.Add(arguments[0].Expression);
                    }
                }
            }
        }
    }
}
