using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeGen
{

    public class CodeGenConstructor : ICodeGenElement
    {
        public CodeGenConstructor(string className, Scope scope, string[] genericTypes, string[] arguments, string body)
        {
            ClassName = className;
            Scope = scope;
            GenericTypes = genericTypes ?? new string[0];
            Arguments = arguments ?? new string[0];
            Body = body ?? string.Empty;
        }

        public string ClassName { get; }

        public Scope Scope { get; }

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

            var genericList = GenericTypes.Any() ? $"<{string.Join(", ", GenericTypes)}>" : string.Empty;

            var argList = string.Join(", ", Arguments);

            builder.AppendLine($"{ClassName}{genericList}({argList})");
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
