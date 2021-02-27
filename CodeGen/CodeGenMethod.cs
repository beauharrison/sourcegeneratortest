using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeGen
{
    public class CodeGenMethod : ICodeGenElement
    {
        public CodeGenMethod(
            string name, 
            string returnType, 
            Scope scope, 
            MethodType methodType, 
            IEnumerable<CodeGenGeneric> genericTypes,
            IEnumerable<string> parameters, 
            string body)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name));

            Name = name;
            ReturnType = !string.IsNullOrWhiteSpace(returnType) ? returnType : "void";
            Scope = scope;
            MethodType = methodType;
            GenericTypes = genericTypes ?? new CodeGenGeneric[0];
            Parameters = parameters ?? new string[0];
            Body = body ?? string.Empty;
        }

        public string Name { get; }

        public string ReturnType { get; }

        public Scope Scope { get; }

        public MethodType MethodType { get; }

        public IEnumerable<CodeGenGeneric> GenericTypes { get; }

        public IEnumerable<string> Parameters { get; }

        public string Body { get; }

        public CodeGenComment Comment { get; set; }

        public string ReturnComment { get; }

        public List<CodeGenAttribute> Attributes { get; } = new List<CodeGenAttribute>();

        public string GenerateCode(CodeGenStyle style = null)
        {
            if (style == null) style = new CodeGenStyle();

            var builder = new StringBuilder();

            if (Comment != null)
            {
                builder.AppendLine(Comment.GenerateCode(style));
            }

            foreach (var attribute in Attributes)
            {
                builder.AppendLine(attribute.GenerateCode(style));
            }

            builder.Append(style.Indent);
            builder.Append(Scope.ToString().ToLower());
            builder.Append(" ");

            if (MethodType != MethodType.Normal)
            {
                builder.Append(MethodType.ToString().ToLower());
                builder.Append(" ");
            }

            var genericList = GenericTypes.Any() ? $"<{string.Join(", ", GenericTypes.Select(gt => gt.Name))}>" : string.Empty;

            var argList = string.Join(", ", Parameters);

            builder.AppendLine($"{ReturnType} {Name}{genericList}({argList})");
            
            foreach (var constrainedGeneric in GenericTypes.Where(gt => gt.Constraint != null))
            {
                style.IndentCount++;
                builder.AppendLine($"{style.Indent}where {constrainedGeneric.Name} : {constrainedGeneric.Constraint}");
                style.IndentCount--;
            }

            builder.AppendLine($"{style.Indent}{{");

            style.IndentCount++;
            builder.AppendLine(style.IndentMultilineString(Body));
            style.IndentCount--;

            builder.Append($"{style.Indent}}}");            

            return builder.ToString();
        }

        public override string ToString() => GenerateCode();
    }
}
