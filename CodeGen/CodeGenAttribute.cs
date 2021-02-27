using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeGen
{
    public class CodeGenAttribute : ICodeGenElement
    {
        public CodeGenAttribute(string name, params string[] arguments)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Arguments = arguments?.ToList() ?? new List<string>();
        }

        public string Name { get; }
        
        public List<string> Arguments { get; }

        public string GenerateCode(CodeGenStyle style = null)
        {
            if (style == null) style = new CodeGenStyle();

            var builder = new StringBuilder(style.Indent);

            builder.Append($"[{Name}");

            if (Arguments.Count > 0)
            {
                var argList = string.Join(", ", Arguments);
                builder.Append($"({argList})");
            }

            builder.Append("]");

            return builder.ToString();
        }
    }
}
