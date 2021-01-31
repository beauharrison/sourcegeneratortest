//using CodeGen;
//using Generators.Sql;
//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;

//namespace Generators.Sql
//{
//    [Generator]
//    public class SqlQueryGenerator : StaticClassGeneratorBase
//    {
//        private const string NamespaceName = "GeneratedNamespace";
//        private const string ClassName = "SqlQuery";
//        private const string MethodName = "Select";

//        private static string[] SupportedTypes = new[] { "Generators.Sql.SqlWhere" };

//        public SqlQueryGenerator() : base(NamespaceName, ClassName, "Construct sql query string from code.")
//        {
//        }

//        protected override void GenerateClassMethods(GeneratorExecutionContext context, CodeGenClass @class)
//        {
//            StaticMethodCallSyntaxReceiver syntaxReceiver = (StaticMethodCallSyntaxReceiver) context.SyntaxReceiver;

//            var foundTypes = new HashSet<string>();


//            @class.Methods.Add(new CodeGenMethod(
//                MethodName,
//                "object",
//                Scope.Public,
//                MethodType.Static,
//                null,
//                new[] { "string name", "params ISqlComponent[] components" },
//                @"// Stub method for intellisense.
//return null;"));

//            foreach ((InvocationExpressionSyntax Invocation, ExpressionSyntax[] Args) call in syntaxReceiver.Calls)
//            {
//                try
//                {
//                    if (call.Args.Length < 2) continue;

//                    SemanticModel semanticModel = context.Compilation.GetSemanticModel(call.Invocation.SyntaxTree);
//                    string[] types = call.Args.Select(exp => semanticModel.GetTypeInfo(exp).Type.ToString()).ToArray();

//                    string[] componentTypes = types.Skip(1).ToArray();

//                    // validate
//                    if (types[0] != "string") continue;
//                    if (!componentTypes.All(t => SupportedTypes.Contains(t))) continue;

//                    @class.Methods.Add(new CodeGenMethod(
//                        MethodName,
//                        "(string Query, Dictionary<string, object> Parameters)",
//                        Scope.Public,
//                        MethodType.Static,
//                        null,
//                        new[] { "string name" }.Concat(componentTypes.Select((type, i) => $"{type} c{i}")).ToArray(),
//                        GenerateBody(componentTypes)));
//                }
//                catch
//                {
//                }
//            }
//        }

//        protected override void ModifyNamespace(GeneratorExecutionContext context, CodeGenNamespace @namespace)
//        {
//            @namespace.Usings.Add("System");
//            @namespace.Usings.Add("Generators.Sql");
//            @namespace.Usings.Add("System.Text");
//            @namespace.Usings.Add("System.Collections.Generic");
//        }

//        public override void Initialize(GeneratorInitializationContext context)
//        {
//            //if (!Debugger.IsAttached) { Debugger.Launch(); }

//            context.RegisterForSyntaxNotifications(() => new StaticMethodCallSyntaxReceiver(NamespaceName, ClassName, MethodName));
//        }

//        private string GenerateBody(string[] sqlComponentTypes)
//        {
//            var builder = new StringBuilder();

//            builder.AppendLine("var builder = new StringBuilder();");
//            builder.AppendLine("var parameters = new Dictionary<string, object>();");
//            builder.AppendLine(@"builder.Append($""SELECT * FROM[{name}]"");");
//            builder.AppendLine();

//            var cCount = 0;
//            int pCount = 0;
//            foreach (var modelType in sqlComponentTypes)
//            {
//                builder.AppendLine(@$"builder.Append($"" WHERE [{{c{cCount}.Field}}] {{c{cCount}.Op.ToSymbol()}} @p{pCount}"");");
//                builder.AppendLine(@$"parameters.Add(""p{pCount}"", c{cCount}.Value);");

//                cCount++;
//                pCount++;
//            }

//            builder.AppendLine("return (builder.ToString(), parameters);");

//            return builder.ToString();
//        }
//    }
//}
