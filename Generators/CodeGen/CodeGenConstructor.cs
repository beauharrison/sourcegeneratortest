using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeGen
{

    public class CodeGenConstructor : ICodeGenElement
    {
        public CodeGenConstructor(
            string className, 
            Scope scope, 
            IEnumerable<string> genericTypes,
            IEnumerable<string> parameters, 
            string body)
        {
            if (string.IsNullOrWhiteSpace(className)) throw new ArgumentException(nameof(className));

            ClassName = className;
            Scope = scope;
            GenericTypes = genericTypes ?? Enumerable.Empty<string>();
            Parameters = parameters ?? Enumerable.Empty<string>();
            Body = body ?? string.Empty;
        }

        public string ClassName { get; }

        public Scope Scope { get; }

        public IEnumerable<string> GenericTypes { get; }

        public IEnumerable<string> Parameters { get; }

        public string Body { get; }

        public CodeGenComment Comment { get; set; }

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

            var genericList = GenericTypes.Any() ? $"<{string.Join(", ", GenericTypes)}>" : string.Empty;

            var argList = string.Join(", ", Parameters);

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
