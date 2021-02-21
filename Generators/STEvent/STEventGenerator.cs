using CodeGen;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Generators.STEvent
{
    [Generator]
    public class STEventGenerator : StaticClassGeneratorBase
    {
        private const string NamespaceName = "GeneratedNamespace";
        private const string ClassName = "STEvent";
        private const string MethodName = "Register";

        public STEventGenerator() : base(NamespaceName, ClassName, "Strongly-typed pub-sub event system.")
        {
        }

        public override void Initialize(GeneratorInitializationContext context)
        {
            //if (!Debugger.IsAttached) { Debugger.Launch(); }

            context.RegisterForSyntaxNotifications(() => new StaticMethodCallSyntaxReceiver(NamespaceName, ClassName, MethodName, 1, 1, 1, 10));
        }

        protected override void GenerateClassMethods(GeneratorExecutionContext context, CodeGenNamespace @namespace, CodeGenClass @class)
        {
            StaticMethodCallSyntaxReceiver syntaxReceiver = (StaticMethodCallSyntaxReceiver) context.SyntaxReceiver;
            
            var generatedHandlers = new HashSet<string>();

            GenerateStub(@class);
            
            foreach (StaticMethodCall call in syntaxReceiver.Calls)
            {
                SemanticModel semanticModel = context.Compilation.GetSemanticModel(call.Invocation.SyntaxTree);

                Dictionary<string, string> genericTypes = call.GenericTypes
                    .Select(gt => semanticModel.GetTypeInfo(gt))
                    .ToDictionary(typeInfo => typeInfo.Type.ToString(), typeInfo => typeInfo.Type.Name);

                string handlerName = $"{string.Join(string.Empty, genericTypes.Values)}Handler";
                string[] argList = genericTypes.Keys.Select((type, i) => $"{type} arg{i}").ToArray();
                string[] argNameList = genericTypes.Select((type, i) => $"arg{i}").ToArray();

                if (generatedHandlers.Contains(handlerName))
                    continue;

                generatedHandlers.Add(handlerName);

                // delegate
                @namespace.Content.Add(new CodeGenDelegate(
                    handlerName,
                    null,
                    Scope.Public,
                    null,
                    argList));

                // private delegate variable
                @class.Variables.Add(new CodeGenVariable(
                    $"_{handlerName}",
                    handlerName,
                    Scope.Private,
                    VariableType.Static));

                // register method
                @class.Methods.Add(new CodeGenMethod(
                    MethodName,
                    null,
                    Scope.Public,
                    MethodType.Static,
                    genericTypes.Keys.Select((gt, i) => new CodeGenGeneric($"TPayload{i}", gt)),
                    new[] { $"Action<{string.Join(", ", genericTypes.Keys)}> handler" },
                    $"_{handlerName} += new {handlerName}(handler);"));

                // notify method
                @class.Methods.Add(new CodeGenMethod(
                    "Notify",
                    null,
                    Scope.Public,
                    MethodType.Static,
                    null,
                    argList,
                    $"_{handlerName}?.Invoke({string.Join(", ", argNameList)});"));
            }

            @namespace.Usings.Add("System");
        }

        private void GenerateStub(CodeGenClass @class)
        {
            @class.Methods.Add(new CodeGenMethod(
                MethodName,
                null,
                Scope.Public,
                MethodType.Static,
                new[] { new CodeGenGeneric("TPayload") },
                new[] { "Action<TPayload> handler" },
                @"// Stub. Does nothing."));
        }
    }
}
