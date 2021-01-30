using CodeGen;
using Generators.ApiDefModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Generators
{
    [Generator]
    public class ApiControllerGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var defFiles = context.AdditionalFiles.Where(f => f.Path.EndsWith("ControllerDefinition.json"));

            foreach (AdditionalText defFile in defFiles)
            {
                ApiDefinition definition;

                string definitionString = File.ReadAllText(defFile.Path);
                definition = JsonConvert.DeserializeObject<ApiDefinition>(definitionString);

                var controllerClass = GenerateController(definition);
                var modelClasses = GenerateModels(definition);

                if (string.IsNullOrWhiteSpace(definition.Name)) throw new ArgumentException("Invalid definition name");

                string controllerNamespaceName = $"{definition.Namespace}.Controllers";
                string modelNamespaceName = $"{definition.Namespace}.Models";

                // Controller
                var controllerNamespace = new CodeGenNamespace(controllerNamespaceName);

                controllerNamespace.Content.Add(controllerClass);
                controllerNamespace.Usings.Add("System");
                controllerNamespace.Usings.Add("System.Threading.Tasks");
                controllerNamespace.Usings.Add("Microsoft.AspNetCore.Mvc");
                controllerNamespace.Usings.Add("Microsoft.Extensions.DependencyInjection");
                controllerNamespace.Usings.Add(definition.Namespace);
                controllerNamespace.Usings.Add(modelNamespaceName);

                context.Add(controllerNamespace, $"{definition.Name}Controller.cs");

                // Models
                foreach (CodeGenClass modelClass in modelClasses)
                {
                    var modelNamespace = new CodeGenNamespace(modelNamespaceName);
                    modelNamespace.Content.Add(modelClass);
                    context.Add(modelNamespace, $"{modelClass.Name}.cs");
                }
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            //if (!Debugger.IsAttached) { Debugger.Launch(); }
        }

        private CodeGenClass GenerateController(ApiDefinition definition)
        {
            if (definition is null) throw new ArgumentNullException(nameof(definition));

            var controllerClass = new CodeGenClass(
                    $"{definition.Name}Controller",
                    Scope.Public,
                    ClassType.Normal,
                    new[] { "ControllerBase" });

            // class comment
            if (!string.IsNullOrWhiteSpace(definition.Description))
                controllerClass.Comment = new CodeGenComment(definition.Description);

            // ApiController attribute
            controllerClass.Attributes.Add(new CodeGenAttribute("ApiController"));


            // Route attribute
            string controllerRoute = !string.IsNullOrWhiteSpace(definition.ControllerRoute) ? $"\"{definition.ControllerRoute}\"" : @"""[controller]""";
            controllerClass.Attributes.Add(new CodeGenAttribute("Route", controllerRoute));

            // Constructor and DI injected variables.
            var constructorStatements = new List<string>();
            var constructorParam = new List<string>();
            var constructorParamComments = new Dictionary<string, string>();
                       
            constructorParam.Add("IServiceProvider serviceProvider");
            constructorParamComments.Add("serviceProvider", "The service provider to access validators and handlers.");
            constructorStatements.Add($"_ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider))");

            controllerClass.Variables.Add(new CodeGenVariable(
                "_ServiceProvider",
                "IServiceProvider",
                Scope.Private,
                readOnly: true));

            //foreach (ApiMethod method in definition.Methods)
            //{
            //    if (string.IsNullOrWhiteSpace(method.Name)) throw new ArgumentException("Invalid dependency name");

            //    string methodValidatorName = $"{method.Name}Validator";
            //    string methodHandlerName = $"{method.Name}Handler";

            //    constructorParam.Add($"{methodValidatorName} {methodValidatorName}");
            //    constructorParam.Add($"{methodHandlerName} {methodHandlerName}");

            //    constructorStatements.Add($"this.{methodValidatorName} = {methodValidatorName};");
            //    constructorStatements.Add($"this.{methodHandlerName} = {methodHandlerName};");

            //    constructorParamComments.Add(methodValidatorName, $"Validator for the {method.Name} method.");
            //    constructorParamComments.Add(methodHandlerName, $"Handler for the {method.Name} method.");

            //    controllerClass.Variables.Add(new CodeGenVariable(
            //        dependency.Name,
            //        dependency.Type,
            //        Scope.Private));
            //}

            foreach (ApiMethod apiMethod in definition.Methods)
            {
                controllerClass.Methods.Add(GenerateMethod(apiMethod));
            }

            var constructor = new CodeGenConstructor(
                controllerClass.Name,
                Scope.Public,
                null,
                constructorParam,
                string.Join(Environment.NewLine, constructorStatements));

            constructor.Comment = new CodeGenComment(
                "Constructor for this controller.",
                paramComments: constructorParamComments);

            controllerClass.Constructors.Add(constructor);

            return controllerClass;
        }

        private CodeGenMethod GenerateMethod(ApiMethod apiMethod)
        {
            if (apiMethod is null) throw new ArgumentNullException(nameof(apiMethod));

            var methodParams = new List<string>();
            var methodParamComments = new Dictionary<string, string>();

            foreach (ApiType qp in apiMethod.QueryParams)
            {
                methodParams.Add($"[FromQuery] {ResolveModelType(qp.Type)} {qp.Name}");

                if (!string.IsNullOrWhiteSpace(qp.Description))
                {
                    methodParamComments.Add(qp.Name, qp.Description);
                }
            }

            if (apiMethod.RequestBodyType != null)
            {
                methodParams.Add($"[FromBody] {ResolveModelType(apiMethod.RequestBodyType.Type)} requestBody");
                methodParamComments.Add("requestBody", "The body of the request.");

            }

            string returnComment = apiMethod.ResponseBodyType != null
                ? @$"Response containing the content of type <see cref=""{ResolveModelType(apiMethod.ResponseBodyType.Type)}""/>"
                : null;

            var method = new CodeGenMethod(
                apiMethod.Name,
                apiMethod.Async
                    ? "Task<IActionResult>"
                    : "IActionResult",
                Scope.Public,
                MethodType.Normal,
                null,
                methodParams,
                GenerateMethodBody(apiMethod));

            method.Comment = new CodeGenComment(
                apiMethod.Description,
                paramComments: methodParamComments,
                returnComment: returnComment);

            var methodAttributeType = apiMethod.Method switch
            {
                "get" => "HttpGet",
                "post" => "HttpPost",
                "put" => "HttpPut",
                "patch" => "HttpPatch",
                "delete" => "HttpDelete",
                _ => throw new Exception("Oh no!")
            };

            var methodAttributeRoute = !string.IsNullOrWhiteSpace(apiMethod.MethodRoute)
                ? new[] { $"\"{apiMethod.MethodRoute}\"" }
                : null;

            method.Attributes.Add(new CodeGenAttribute(methodAttributeType, methodAttributeRoute));

            return method;
        }

        private string GenerateMethodBody(ApiMethod apiMethod)
        {
            string validatorType = $"{apiMethod.Name}Validator";
            string handlerType = $"{apiMethod.Name}Handler";

            var builder = new StringBuilder();
            List<string> methodParams = apiMethod.QueryParams.Select(qp => qp.Name).ToList();

            if (apiMethod.RequestBodyType != null) methodParams.Add("requestBody");

            string methodParamStr = string.Join(", ", methodParams);

            builder.AppendLine($@"{handlerType} handler = _ServiceProvider.GetService<{handlerType}>();
if (handler == null) return new StatusCodeResult(500);");

            builder.AppendLine();

            if (methodParams.Any())
            {
                builder.AppendLine($@"{validatorType} validator = _ServiceProvider.GetService<{validatorType}>();
if (validator == null) return new StatusCodeResult(500); 

try
{{
    ValidationResult validationResult = validator.Validate({methodParamStr})

    if (!validationResult.Success)
    {{
        return handler.HandleValidationFailure(validationResults);
    }}
}}
catch (Exception e)
{{
    return handler.HandleValidationError(e);
}}");

                builder.AppendLine();
            }

            string handleCall = $"handler.HandleRequest({methodParamStr});";

            if (apiMethod.ResponseBodyType != null)
            {
                builder.AppendLine($"{ResolveModelType(apiMethod.ResponseBodyType.Type)} responseBody = {handleCall}");
                builder.AppendLine($"return Ok(responseBody);");
            }
            else
            {
                builder.AppendLine(handleCall);
                builder.AppendLine($"return NoContent();");
            }

            
            return builder.ToString();
        }

        private IEnumerable<CodeGenClass> GenerateModels(ApiDefinition definition)
        {
            foreach (ApiModel model in definition.Models)
            {
                if (string.IsNullOrWhiteSpace(model.Name)) throw new ArgumentException("Invalid model name");

                var modelClass = new CodeGenClass(
                    model.Name,
                    Scope.Public,
                    ClassType.Normal);

                modelClass.Comment = new CodeGenComment(model.Description);

                foreach (ApiType prop in model.Props)
                {
                    if (string.IsNullOrWhiteSpace(prop.Name)) throw new ArgumentException("Invalid property name");
                    if (string.IsNullOrWhiteSpace(prop.Type)) throw new ArgumentException("Invalid property type");

                    var propProp = new CodeGenProperty(
                        LowerToUpperCamel(prop.Name),
                        ResolveModelType(prop.Type), 
                        Scope.Public, 
                        true);

                    propProp.Comment = new CodeGenComment(prop.Description);
                    
                    // TODO required

                    modelClass.Properties.Add(propProp);
                }

                yield return modelClass;
            }
        }

        private string LowerToUpperCamel(string lower) => $"{lower.ToUpper()[0]}{lower.Substring(1)}";

        private string ResolveModelType(string type)
        {
            if (!type.StartsWith("#/")) return type;

            return type.Substring(type.LastIndexOf('/') + 1);
        }
    }
}
