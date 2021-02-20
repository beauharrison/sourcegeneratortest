using CodeGen;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Generators.DI
{
    [Generator]
    public class DependencyInjectionGenerator : StaticClassGeneratorBase
    {
        private const string NamespaceName = "GeneratedDependencyInjection";
        private const string ClassName = "DIService";
        private const string TransientMethodName = "AddTransient";
        private const string SharedMethodName = "AddShared";
        private const string SingletonMethodName = "AddSingleton";

        public DependencyInjectionGenerator() : base(NamespaceName, ClassName, "Service which provides dependency injection.")
        {
        }

        public override void Initialize(GeneratorInitializationContext context)
        {
            //if (!Debugger.IsAttached) { Debugger.Launch(); }

            context.RegisterForSyntaxNotifications(() => new StaticMethodCallSyntaxReceiver(
                NamespaceName, 
                ClassName, 
                new[] { TransientMethodName, SharedMethodName, SingletonMethodName }, 
                maxArgCount: 0,
                minGenericsCount: 1,
                maxGenericsCount: 2));
        }

        protected override void GenerateClassMethods(GeneratorExecutionContext context, CodeGenNamespace @namespace, CodeGenClass @class)
        {
            StaticMethodCallSyntaxReceiver syntaxReceiver = (StaticMethodCallSyntaxReceiver) context.SyntaxReceiver;

            // singleton store
            @class.Variables.Add(new CodeGenVariable("_SingletonLock", "object", Scope.Private, VariableType.Static, true, "new object()"));
            @class.Variables.Add(new CodeGenVariable("_Singletons", "IDictionary<Type, object>", Scope.Private, VariableType.Static, true, "new Dictionary<Type, object>()"));

            // static methods
            AddRegistrationMethods(@class);
            AddGetTMethod(@class);

            // get registrations
            IDictionary<string, DIRegistration> registrations = ProcessCalls(context, syntaxReceiver.Calls);

            // Function dictionary used by get method
            AddGetFunctionsDictionary(@class, registrations);

            foreach (DIRegistration registration in registrations.Values)
            {
                // Build stack contining the initialization of this registered resource, including any dependencies.
                Stack<DIDependencyInitialization> initStack = BuildInitStack(registration, registrations);

                // Use init stack to generate body
                string body = BuildTypedGetMethodBody(initStack);

                var method = new CodeGenMethod(
                    registration.DirectGetMethodName,
                    registration.IdentifierType,
                    Scope.Private,
                    MethodType.Static,
                    null,
                    null,
                    body);

                @class.Methods.Add(method);
            }

            ModifyNamespace(@namespace);
        }

        private void ModifyNamespace(CodeGenNamespace @namespace)
        {
            @namespace.Usings.Add("System");
            @namespace.Usings.Add("System.Collections.Generic");
        }

        private string BuildTypedGetMethodBody(Stack<DIDependencyInitialization> initStack)
        {
            bool involvesSingleton = false;
            var builder = new StringBuilder();

            while (initStack.Any())
            {
                DIDependencyInitialization nextInit = initStack.Pop();
                string initParams = string.Join(", ", nextInit.ConstructorParams);

                if (nextInit.Registration.Type == DIRegistrationType.Singleton)
                {
                    involvesSingleton = true;
                    builder.AppendLine($@"
{nextInit.Registration.IdentifierType} {nextInit.Variable};
if (_Singletons.TryGetValue(typeof({nextInit.Registration.IdentifierType}), out object {nextInit.Variable}Object))
{{
    {nextInit.Variable} = ({nextInit.Registration.IdentifierType}) {nextInit.Variable}Object;
}}
else
{{
    {nextInit.Variable} = new {nextInit.Registration.ImplementationType}({initParams});
    _Singletons[typeof({nextInit.Registration.IdentifierType})] = {nextInit.Variable};
}}");
                }
                else
                {
                    builder.AppendLine($"{nextInit.Registration.IdentifierType} {nextInit.Variable} = new {nextInit.Registration.ImplementationType}({initParams});");
                }
            }

            builder.Append($"return result;");

            if (involvesSingleton)
            {
                var innerBody = builder.ToString().Replace(Environment.NewLine, $"{Environment.NewLine}    ");

                return $@"lock (_SingletonLock)
{{
    {innerBody}
}}";
            }
            else
            {
                return builder.ToString();
            }
        }

        private Stack<DIDependencyInitialization> BuildInitStack(DIRegistration registration, IDictionary<string, DIRegistration> registrations)
        {
            var initStack = new Stack<DIDependencyInitialization>();
            var initQueue = new Queue<(string Variable, DIRegistration Registration)>();

            var sharedDependencies = new Dictionary<string, string>();

            initQueue.Enqueue((null, registration));

            int depInitId = 0;

            while (initQueue.Any())
            {
                if (initStack.Count > 100)
                {
                    var foramtedQueue = string.Join(" < ", initStack.Select(i => i.Registration.IdentifierType));
                    throw new Exception($"Dependnecy depth exceeded limit for registerd resource {registration.IdentifierType}:{registration.ImplementationType}. Possible recursive usage of transient resources. Dependency initialization chain: {foramtedQueue}");
                }

                (string nextInitVariable, DIRegistration nextInitReg) = initQueue.Dequeue();

                var initParams = new List<string>();
                foreach (string ctorParamType in nextInitReg.ConstructorParamTypes)
                {
                    if (!registrations.TryGetValue(ctorParamType, out DIRegistration paramTypeReg))
                        throw new Exception($"Unregisterd type {ctorParamType} used in constuctor of {registration.IdentifierType}:{registration.ImplementationType}");

                    string depInitVariable;

                    if (paramTypeReg.Type == DIRegistrationType.Shared)
                    {
                        if (!sharedDependencies.TryGetValue(ctorParamType, out depInitVariable))
                        {                        
                            depInitVariable = $"d{depInitId++}";
                            sharedDependencies.Add(ctorParamType, depInitVariable);
                            initQueue.Enqueue((depInitVariable, paramTypeReg));
                        }
                    }
                    else
                    {
                        depInitVariable = $"d{depInitId++}";
                        initQueue.Enqueue((depInitVariable, paramTypeReg));
                    }

                    initParams.Add(depInitVariable);
                }

                initStack.Push(new DIDependencyInitialization
                {
                    Registration = nextInitReg,
                    ConstructorParams = initParams.ToArray(),
                    Variable = nextInitVariable ?? "result",
                });
            }

            return initStack;
        }

        private void AddGetFunctionsDictionary(CodeGenClass @class, IDictionary<string, DIRegistration> registrations)
        {
            var assignmentBuilder = new StringBuilder();

            assignmentBuilder.AppendLine("new Dictionary<Type, Func<object>>");
            assignmentBuilder.AppendLine("{");

            foreach (DIRegistration registration in registrations.Values)
            {
                assignmentBuilder.AppendLine($"    [typeof({registration.IdentifierType})] = {registration.DirectGetMethodName},");
            }

            assignmentBuilder.AppendLine("}");

            @class.Variables.Add(new CodeGenVariable(
               "_GetFunctions",
               "IDictionary<Type, Func<object>>",
               Scope.Private,
               VariableType.Static,
               true,
               assignmentBuilder.ToString()));
        }

        private IDictionary<string, DIRegistration> ProcessCalls(GeneratorExecutionContext context, IEnumerable<StaticMethodCall> calls)
        {
            var registrations = new Dictionary<string, DIRegistration>();
            int callId = 0;

            foreach (StaticMethodCall call in calls)
            {
                SemanticModel semanticModel = context.Compilation.GetSemanticModel(call.Invocation.SyntaxTree);

                (TypeInfo, SymbolInfo)[] genericTypeInfos = call.GenericTypes
                    .Select(gt => (semanticModel.GetTypeInfo(gt), semanticModel.GetSymbolInfo(gt)))
                    .ToArray();

                (TypeInfo identifierTypeInfo, SymbolInfo identifierSymbolInfo) = genericTypeInfos.First();
                (TypeInfo implementationTypeInfo, SymbolInfo implementationSymbolInfo) = genericTypeInfos.Last();

                // Get constructor parameters
                var namedImplementaitonSymbol = (implementationSymbolInfo.Symbol ?? implementationSymbolInfo.CandidateSymbols.FirstOrDefault()) as INamedTypeSymbol;
                IEnumerable<IParameterSymbol> constructorParamsSymbols = namedImplementaitonSymbol?.Constructors.FirstOrDefault()?.Parameters;
                IEnumerable<string> constructorParams = constructorParamsSymbols?.Select(p => p.Type.ToString()) ?? Enumerable.Empty<string>();

                DIRegistrationType regType = call.Method switch
                {
                    TransientMethodName => DIRegistrationType.Transient,
                    SharedMethodName => DIRegistrationType.Shared,
                    SingletonMethodName => DIRegistrationType.Singleton,
                    _ => throw new Exception("Unexpected registration type.")
                };

                registrations[identifierTypeInfo.Type.ToString()] = new DIRegistration
                {
                    Type = regType,
                    IdentifierType = identifierTypeInfo.Type.ToString(),
                    ImplementationType = implementationTypeInfo.Type.ToString(),
                    DirectGetMethodName = $"Get{callId++}",
                    ConstructorParamTypes = constructorParams
                };
            }

            return registrations;
        }

        private void AddRegistrationMethods(CodeGenClass @class)
        {
            var transientMethod = new CodeGenMethod(
                TransientMethodName,
                null,
                Scope.Public,
                MethodType.Static,
                new[] { new CodeGenGeneric("TIdentifier"), new CodeGenGeneric("TImplementation") },
                null,
                null);

            transientMethod.Comment = new CodeGenComment(
                "Register a resource for transient dependency injection.",
                new Dictionary<string, string>
                {
                    ["TInterface"] = "Type which identifies the resources and used to access by.",
                    ["TImplementaion"] = "Concreate type of the resource."
                });

            @class.Methods.Add(transientMethod);

            var sharedMethod = new CodeGenMethod(
                SharedMethodName,
                null,
                Scope.Public,
                MethodType.Static,
                new[] { new CodeGenGeneric("TIdentifier"), new CodeGenGeneric("TImplementation") },
                null,
                null);

            sharedMethod.Comment = new CodeGenComment(
                "Register a resource for shared dependency injection.",
                new Dictionary<string, string>
                {
                    ["TInterface"] = "Type which identifies the resources and used to access by.",
                    ["TImplementaion"] = "Concreate type of the resource."
                });

            @class.Methods.Add(sharedMethod);

            var singletonMethod = new CodeGenMethod(
                SingletonMethodName,
                null,
                Scope.Public,
                MethodType.Static,
                new[] { new CodeGenGeneric("TIdentifier"), new CodeGenGeneric("TImplementation") },
                null,
                null);

            singletonMethod.Comment = new CodeGenComment(
                "Register a resource for singleton dependency injection.",
                new Dictionary<string, string>
                {
                    ["TIdentifier"] = "Type which identifies the resources and used to access by.",
                    ["TImplementaion"] = "Concreate type of the resource."
                });

            @class.Methods.Add(singletonMethod);
        }

        private void AddGetTMethod(CodeGenClass @class)
        {
            var getMethod = new CodeGenMethod(
                "Get",
                "TIdentifier",
                Scope.Public,
                MethodType.Static,
                new[] { new CodeGenGeneric("TIdentifier") },
                null,
                $@"if (_GetFunctions.TryGetValue(typeof(TIdentifier), out Func<object> getFunc))
{{
    return (TIdentifier) getFunc();
}}

return default;");

            getMethod.Comment = new CodeGenComment(
                "Get a registered resource.",
                new Dictionary<string, string>
                {
                    ["TIdentifier"] = "Type which identifies the resources",
                },
                returnComment: "The requested resource, or a default value if not found.");

            @class.Methods.Add(getMethod);

        }
    }
}
