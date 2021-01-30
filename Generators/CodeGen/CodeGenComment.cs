using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeGen
{
    public class CodeGenComment : ICodeGenElement
    {
        public CodeGenComment(
            string summary,
            IEnumerable<KeyValuePair<string, string>> typeParamComments = null,
            IEnumerable<KeyValuePair<string, string>> paramComments = null, 
            string returnComment = null)
        {
            Summary = summary ?? string.Empty;
            TypeParams = typeParamComments ?? Enumerable.Empty<KeyValuePair<string, string>>();
            Params = paramComments ?? Enumerable.Empty<KeyValuePair<string, string>>();
            Return = returnComment;
        }

        public string Summary { get; }

        public IEnumerable<KeyValuePair<string, string>> TypeParams { get; }

        public IEnumerable<KeyValuePair<string, string>> Params { get; }

        public string Return { get; }

        public string GenerateCode(CodeGenStyle style = null)
        {
            var builder = new StringBuilder();

            builder.AppendLine(style.FormatCommentSummary(Summary));

            foreach (KeyValuePair<string, string> typePair in TypeParams)
            {
                if (!string.IsNullOrWhiteSpace(typePair.Key) && !string.IsNullOrWhiteSpace(typePair.Value))
                {
                    builder.AppendLine(style.FormatCommentTypeParamRef(typePair.Key, typePair.Value));
                }
            }

            foreach (KeyValuePair<string, string> paramPair in Params)
            {
                if (!string.IsNullOrWhiteSpace(paramPair.Key) && !string.IsNullOrWhiteSpace(paramPair.Value))
                {
                    builder.AppendLine(style.FormatCommentParamRef(paramPair.Key, paramPair.Value));
                }
            }

            if (!string.IsNullOrWhiteSpace(Return))
            {
                builder.AppendLine(style.FormatCommentReturn(Return));
            }

            return builder.ToString().TrimEnd();
        }
    }
}
