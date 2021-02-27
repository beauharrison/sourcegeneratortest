using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeGen
{
    public class CodeGenDelegate : ICodeGenElement
    {
        public CodeGenDelegate(
            string name,
            string returnType,
            Scope scope,
            IEnumerable<CodeGenGeneric> genericTypes,
            IEnumerable<string> parameters)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name));

            Name = name;
            ReturnType = !string.IsNullOrWhiteSpace(returnType) ? returnType : "void";
            Scope = scope;
            GenericTypes = genericTypes ?? new CodeGenGeneric[0];
            Parameters = parameters ?? new string[0];
        }

        public string Name { get; }

        public string ReturnType { get; }

        public Scope Scope { get; }

        public IEnumerable<CodeGenGeneric> GenericTypes { get; }

        public IEnumerable<string> Parameters { get; }

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

            var genericList = GenericTypes.Any() ? $"<{string.Join(", ", GenericTypes.Select(gt => gt.Name))}>" : string.Empty;
            var argList = string.Join(", ", Parameters);

            builder.Append($" delegate {ReturnType} {Name}{genericList}({argList});");

            return builder.ToString();
        }
    }
}
