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

                try
                {
                    string definitionString = File.ReadAllText(defFile.Path);
                    definition = JsonConvert.DeserializeObject<ApiDefinition>(definitionString);
                }
                catch
                {
                    // TODO what?
                    continue;                    
                }

                GenerateController(context, definition);
                GenerateModels(context, definition);
            }
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

        private void GenerateController(GeneratorExecutionContext context, ApiDefinition definition)
        {
            var controllerClass = new CodeGenClass(
                    $"{definition.Name}Controller",
                    Scope.Public,
                    ClassType.Normal,
                    new[] { "Microsoft.AspNetCore.Mvc.ControllerBase" });

            controllerClass.Attributes.Add(new CodeGenAttribute("Microsoft.AspNetCore.Mvc.ApiController", null));

            if (!string.IsNullOrWhiteSpace(definition.ControllerRoute))
                controllerClass.Attributes.Add(new CodeGenAttribute("Microsoft.AspNetCore.Mvc.Route", new[] { $"\"{definition.ControllerRoute}\"" }));

            if (definition.Dependencies.Any())
            {
                var constructorStatements = new List<string>();
                var constructorArguments = new List<string>();

                foreach (var dependency in definition.Dependencies)
                {
                    constructorStatements.Add($"this.{dependency.Name} = {dependency.Name};");
                    constructorArguments.Add($"{dependency.Type} {dependency.Name}");

                    controllerClass.Variables.Add(new CodeGenVariable(
                        dependency.Name,
                        dependency.Type,
                        Scope.Private));
                }

                controllerClass.Constructors.Add(new CodeGenConstructor(
                    controllerClass.Name,
                    Scope.Public,
                    null,
                    constructorArguments.ToArray(),
                    string.Join(Environment.NewLine, constructorStatements)));
            }

            foreach (ApiMethod apiMethod in definition.Methods)
            {
                // TODO requried attribute
                IEnumerable<string> queryParams = apiMethod.QueryParams.Select(qp => $"[Microsoft.AspNetCore.Mvc.FromQuery] {ResolveModelType(qp.Type, true)} {qp.Name}");
                string[] bodyParams = apiMethod.RequestBodyType != null
                    ? new[] { $"[Microsoft.AspNetCore.Mvc.FromBody] {ResolveModelType(apiMethod.RequestBodyType.Type, true)} requestBody" }
                    : new string[0];

                var method = new CodeGenMethod(
                    apiMethod.Name,
                    apiMethod.Async
                        ? "System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.IActionResult>"
                        : "Microsoft.AspNetCore.Mvc.IActionResult",
                    Scope.Public,
                    MethodType.Normal,
                    null,
                    queryParams.Concat(bodyParams).ToArray(),
                    @"
_TestService.Add(new GeneratedModels.UserModel { Name = ""bonus soda"" });
return NoContent();");

                var methodAttributeType = apiMethod.Method switch
                {
                    "get" => "Microsoft.AspNetCore.Mvc.HttpGet",
                    "post" => "Microsoft.AspNetCore.Mvc.HttpPost",
                    "put" => "Microsoft.AspNetCore.Mvc.HttpPut",
                    "patch" => "Microsoft.AspNetCore.Mvc.HttpPatch",
                    "delete" => "Microsoft.AspNetCore.Mvc.HttpDelete",
                    _ => throw new Exception("Oh no!")
                };

                var methodAttributeRoute = !string.IsNullOrWhiteSpace(apiMethod.MethodRoute)
                    ? new[] { $"\"{apiMethod.MethodRoute}\"" }
                    : null;

                method.Attributes.Add(new CodeGenAttribute(methodAttributeType, methodAttributeRoute));

                controllerClass.Methods.Add(method);
            }

            AddController(context, controllerClass);
        }

        private void GenerateModels(GeneratorExecutionContext context, ApiDefinition definition)
        {
            foreach (KeyValuePair<string, ApiModel> model in definition.Models)
            {
                var modelClass = new CodeGenClass(
                    model.Key,
                    Scope.Public,
                    ClassType.Normal);

                foreach (ApiType prop in model.Value.Props)
                {
                    var propProp = new CodeGenProperty(
                        LowerToUpperCamel(prop.Name),
                        ResolveModelType(prop.Type, false), 
                        Scope.Public, 
                        true);
                    
                    // TODO required

                    modelClass.Properties.Add(propProp);
                }

                AddModel(context, modelClass);
            }
        }

        private void AddController(GeneratorExecutionContext context, CodeGenClass controllerClass)
        {
            var generatedNamespace = new CodeGenNamespace("GeneratedControllers");
            generatedNamespace.Content.Add(controllerClass);

            string generatedCodeString = generatedNamespace.GenerateCode();

            var sourceText = SourceText.From(generatedCodeString, Encoding.UTF8);
            context.AddSource($"{controllerClass.Name}.cs", sourceText);
        }

        private void AddModel(GeneratorExecutionContext context, CodeGenClass modelClass)
        {
            var generatedNamespace = new CodeGenNamespace("GeneratedModels");
            generatedNamespace.Content.Add(modelClass);

            string generatedCodeString = generatedNamespace.GenerateCode();

            var sourceText = SourceText.From(generatedCodeString, Encoding.UTF8);
            context.AddSource($"{modelClass.Name}.cs", sourceText);
        }

        private string LowerToUpperCamel(string lower) => $"{lower.ToUpper()[0]}{lower.Substring(1)}";

        private string ResolveModelType(string type, bool includeNamespace)
        {
            if (!type.StartsWith("#/")) return type;

            string baseType = type.Substring(type.LastIndexOf('/') + 1);
            return includeNamespace ?  $"GeneratedModels.{baseType}" : baseType;
        }
    }
}
