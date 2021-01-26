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
            string assignment = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Scope = scope;
            VariableType = variableType;
            Assignment = assignment;
        }

        public string Name { get; }

        public string Type { get; }

        public Scope Scope { get; }

        public VariableType VariableType { get; }

        public string Assignment { get; }

        public string GenerateCode(CodeGenStyle style = null)
        {
            if (style == null) style = new CodeGenStyle();

            var builder = new StringBuilder(style.Indent);

            builder.Append(Scope.ToString().ToLower());
            builder.Append(" ");

            if (VariableType != VariableType.Normal)
            {
                builder.Append(VariableType.ToString().ToLower());
                builder.Append(" ");
            }

            builder.Append($"{Type} {Name}");

            if (!string.IsNullOrWhiteSpace(Assignment))
            {
                builder.Append($" = {Assignment};");
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
