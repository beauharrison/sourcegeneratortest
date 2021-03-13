using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeGen
{
    public static class CodeGenHelpers
    {
        public static string ToTypeParamList(this IEnumerable<string> types)
        {
            return types?.Any() ?? false ? $"<{string.Join(", ", types)}>" : string.Empty;
        }
    }
}
