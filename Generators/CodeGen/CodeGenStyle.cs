using System;

namespace CodeGen
{
    public class CodeGenStyle
    {
        public int IndentSize { get; } = 4;

        public int IndentCount { get; set; } = 0;

        public char IndentChar { get; } = ' ';

        public string Indent => new string(IndentChar, IndentSize * IndentCount);

        public string IndentMultilineString(string original, bool prependIndent = true)
        {
            var replaced = original.Trim().Replace(Environment.NewLine, $"{Environment.NewLine}{Indent}");
            return prependIndent ? $"{Indent}{replaced}" : replaced;
        }

        public string FormatCommentSummary(string comment)
        {
            string pre = $"{Indent}/// ";

            var replaced = comment.Trim().Replace(Environment.NewLine, $"{Environment.NewLine}{pre}");
            return $"{pre}<summary>{Environment.NewLine}{pre}{replaced}{Environment.NewLine}{pre}</summary>";
        }

        public string FormatCommentTypeParamRef(string name, string comment)
        {
            return @$"{Indent}/// <typeparam name=""{name}"">{comment}</typeparam>";
        }

        public string FormatCommentParamRef(string name, string comment)
        {
            return @$"{Indent}/// <param name=""{name}"">{comment}</param>";
        }

        public string FormatCommentReturn(string comment)
        {
            return @$"{Indent}/// <returns>{comment}</returns>";
        }
    }
}
