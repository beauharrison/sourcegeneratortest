using System;
using System.Linq;
using System.Text;

namespace CodeGen
{
    public class CodeGenProperty : ICodeGenElement
    {
        public CodeGenProperty(string name, string type, Scope scope, string getMethodBody, string setMethodBody = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Scope = scope;
            HasSet = !string.IsNullOrWhiteSpace(setMethodBody);
            GetMethodBody = getMethodBody ?? throw new ArgumentNullException(nameof(getMethodBody));
            SetMethodBody = setMethodBody;
        }

        public CodeGenProperty(string name, string type, Scope scope, bool hasSet, string initializer = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Scope = scope;
            HasSet = hasSet;
            Initializer = initializer;
        }

        public string Name { get; }

        public string Type { get; }

        public Scope Scope { get; }

        public bool HasSet { get; }

        public string GetMethodBody { get; }

        public string SetMethodBody { get; }

        public string Initializer { get; }

        public string GenerateCode(CodeGenStyle style = null)
        {
            if (style == null) style = new CodeGenStyle();

            var builder = new StringBuilder(style.Indent);

            builder.Append(Scope.ToString().ToLower());
            builder.Append(" ");

            builder.AppendLine($"{Type} {Name} {{");
            style.IndentCount++;

            if (!string.IsNullOrWhiteSpace(GetMethodBody))
            {
                builder.AppendLine($"{style.Indent}get");
                builder.AppendLine($"{style.Indent}{{");
                style.IndentCount++;
                builder.AppendLine(style.IndentMultilineString(GetMethodBody));
                style.IndentCount--;
                builder.AppendLine($"{style.Indent}}}");
            }
            else
            {
                builder.AppendLine($"{style.Indent}get;");
            }

            if (HasSet)
            {
                if (!string.IsNullOrWhiteSpace(SetMethodBody))
                {
                    builder.AppendLine($"{style.Indent}set");
                    builder.AppendLine($"{style.Indent}{{");
                    style.IndentCount++;
                    builder.AppendLine(style.IndentMultilineString(SetMethodBody));
                    style.IndentCount--;
                    builder.AppendLine($"{style.Indent}}}");
                }
                else
                {
                    builder.AppendLine($"{style.Indent}set;");
                }
            }

            style.IndentCount--;
            builder.Append($"{style.Indent}}}");

            if (!string.IsNullOrWhiteSpace(Initializer))
            {
                builder.Append($" = {Initializer};");
            }

            return builder.ToString();
        }

        public override string ToString() => GenerateCode();
    }
}
