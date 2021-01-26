using System;

namespace CodeGen
{
    public class CodeGenStyle
    {
        public int IndentSize { get; } = 4;

        public int IndentCount { get; set; } = 0;

        public char IndentChar { get; } = ' ';

        public string Indent => new string(IndentChar, IndentSize * IndentCount);

        public string IndentMultilineString(string original)
        {
            var replaced = original.Trim().Replace(Environment.NewLine, $"{Environment.NewLine}{Indent}");
            return $"{Indent}{replaced}";
        }

        public string FormatComment(string comment)
        {
            string pre = $"{Indent}/// ";

            var replaced = comment.Trim().Replace(Environment.NewLine, $"{Environment.NewLine}{pre}");
            return $"{pre}<summary>{Environment.NewLine}{pre}{replaced}{Environment.NewLine}{pre}</summary>";
        }
    }
}
