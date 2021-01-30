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

            context.RegisterForSyntaxNotifications(() => new StaticMethodCallSyntaxReceiver(NamespaceName, ClassName, MethodName));
        }

        protected override void GenerateClassMethods(GeneratorExecutionContext context, CodeGenClass @class)
        {
            StaticMethodCallSyntaxReceiver syntaxReceiver = (StaticMethodCallSyntaxReceiver) context.SyntaxReceiver;

            var foundTypes = new HashSet<string>();

            // No calls? Generate stub for intellisence.
            if (!syntaxReceiver.Calls.Any())
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

            foreach ((InvocationExpressionSyntax Invocation, ExpressionSyntax[] Args) call in syntaxReceiver.Calls)
            {
                if (call.Args.Length != 1) continue;

                var argExpression = call.Args[0];
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
}
