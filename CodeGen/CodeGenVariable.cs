using System;
using System.Linq;
using System.Text;

namespace CodeGen
{
    public class CodeGenVariable : ICodeGenElement
    {
        public CodeGenVariable(
            string name,
            string type,
            Scope scope,
            VariableType variableType = VariableType.Normal,
            bool readOnly = false,
            string assignment = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name));
            if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException(nameof(type));

            Name = name;
            Type = type;
            Scope = scope;
            VariableType = variableType;
            ReadOnly = readOnly;
            Assignment = assignment;
        }

        public string Name { get; }

        public string Type { get; }

        public Scope Scope { get; }

        public VariableType VariableType { get; }

        public bool ReadOnly { get; }

        public string Assignment { get; }

        public CodeGenComment Comment { get; }

        public string GenerateCode(CodeGenStyle style = null)
        {
            if (style == null) style = new CodeGenStyle();

            var builder = new StringBuilder();

            if (Comment != null)
            {
                builder.AppendLine(Comment.GenerateCode(style));
            }

            builder.Append(style.Indent);
            builder.Append(Scope.ToString().ToLower());
            builder.Append(" ");

            if (ReadOnly)
            {
                builder.Append("readonly ");
            }

            if (VariableType != VariableType.Normal)
            {
                builder.Append(VariableType.ToString().ToLower());
                builder.Append(" ");
            }

            builder.Append($"{Type} {Name}");

            if (!string.IsNullOrWhiteSpace(Assignment))
            {
                builder.Append($" = {style.IndentMultilineString(Assignment, false)};");
            } 
            else
            {
                builder.Append(";");
            }

            return builder.ToString();
        }

        public override string ToString() => GenerateCode();
    }
}
