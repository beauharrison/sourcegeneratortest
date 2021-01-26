using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeGen
{ 
    public class CodeGenMethod : ICodeGenElement
    {
        public CodeGenMethod(string name, string returnType, Scope scope, MethodType methodType, string[] genericTypes, string[] arguments, string body)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ReturnType = returnType ?? "void";
            Scope = scope;
            MethodType = methodType;
            GenericTypes = genericTypes ?? new string[0];
            Arguments = arguments ?? new string[0];
            Body = body ?? string.Empty;
        }

        public string Name { get; }

        public string ReturnType { get; }

        public Scope Scope { get; }

        public MethodType MethodType { get; }

        public string[] GenericTypes { get; }

        public string[] Arguments { get; }

        public string Body { get; }

        public List<CodeGenAttribute> Attributes { get; } = new List<CodeGenAttribute>();

        public string GenerateCode(CodeGenStyle style = null)
        {
            if (style == null) style = new CodeGenStyle();

            var builder = new StringBuilder();

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

            var genericList = GenericTypes.Any() ? $"<{string.Join(", ", GenericTypes)}>" : string.Empty;

            var argList = string.Join(", ", Arguments);

            builder.AppendLine($"{ReturnType} {Name}{genericList}({argList})");
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
