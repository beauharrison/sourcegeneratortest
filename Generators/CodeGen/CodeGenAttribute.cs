using System;
using System.Text;

namespace CodeGen
{
    public class CodeGenAttribute : ICodeGenElement
    {
        public CodeGenAttribute(string name, string[] arguments)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Arguments = arguments ?? new string[0];
        }

        public string Name { get; }
        
        public string[] Arguments { get; }

        public string GenerateCode(CodeGenStyle style = null)
        {
            if (style == null) style = new CodeGenStyle();

            var builder = new StringBuilder(style.Indent);

            builder.Append($"[{Name}");

            if (Arguments.Length > 0)
            {
                var argList = string.Join(", ", Arguments);
                builder.Append($"({argList})");
            }

            builder.Append("]");

            return builder.ToString();
        }
    }
}
