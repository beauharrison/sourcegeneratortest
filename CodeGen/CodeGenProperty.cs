using System;
using System.Text;

namespace CodeGen
{
    public class CodeGenProperty : ICodeGenElement
    {
        public CodeGenProperty(string name, string type, Scope scope, string getMethodBody, string setMethodBody = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name));
            if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException(nameof(type));

            Name = name;
            Type = type;
            Scope = scope;
            HasSet = !string.IsNullOrWhiteSpace(setMethodBody);
            GetMethodBody = getMethodBody ?? throw new ArgumentNullException(nameof(getMethodBody));
            SetMethodBody = setMethodBody;
        }

        public CodeGenProperty(string name, string type, Scope scope, bool hasSet, string initializer = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name));
            if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException(nameof(type));

            Name = name;
            Type = type;
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

        public CodeGenComment Comment { get; set; }

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

            builder.AppendLine($"{Type} {Name}");
            builder.AppendLine($"{style.Indent}{{");
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
