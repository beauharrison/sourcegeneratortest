using CodeGen;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DecoMaker.Generation
{
    internal static class FactoryHelpers
    {
        internal static CodeGenGeneric GenerateMethodParameter(string genericType, IDictionary<string, string> constraints)
        {
            constraints.TryGetValue(genericType, out string constraint);
            return new CodeGenGeneric(genericType, constraint);
        }

        internal static string CleanBody(string body)
        {
            // remove wrapping braces
            body = body.TrimStart('{', '\r', '\n').TrimEnd('}');

            // normalize tabs
            body = body.Replace("\t", "    ");

            // get index of first non-space character
            char firstChar = body.First(c => c != ' ');
            int firstCharIndex = body.IndexOf(firstChar);

            // replace excessive whitespace based on first lines whitespace
            return body.Replace($"{Environment.NewLine}{new string(' ', firstCharIndex)}", Environment.NewLine).Trim();
        }
    }
}
