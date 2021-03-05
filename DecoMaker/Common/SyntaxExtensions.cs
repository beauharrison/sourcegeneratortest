using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace DecoMaker.Common
{
    public static class SyntaxExtensions
    {
        private const string PublicModifier = "public";
        private const string AsyncModifier = "async";
        private const string GetterAccessor = "get";
        private const string SetterAccessor = "set";

        /// <summary>
        /// Check if a member delaration is public.
        /// </summary>
        /// <param name="syntax">The delaration syntax to check.</param>
        /// <returns>If the delaration is public.</returns>
        public static bool IsPublic(this MemberDeclarationSyntax syntax) => HasModifier(syntax, PublicModifier);

        /// <summary>
        /// Check if a member delaration is async.
        /// </summary>
        /// <param name="syntax">The delaration syntax to check.</param>
        /// <returns>If the delaration is async.</returns>
        public static bool IsAsync(this MemberDeclarationSyntax syntax) => HasModifier(syntax, AsyncModifier);

        /// <summary>
        /// Check if a property declaration has a getter.
        /// </summary>
        /// <param name="syntax">The property delaration syntax to check.</param>
        /// <returns>If the property has a getter.</returns>
        public static bool HasGetter(this PropertyDeclarationSyntax syntax) => HasAccessor(syntax, GetterAccessor);

        /// <summary>
        /// Check if a property declaration has a setter.
        /// </summary>
        /// <param name="syntax">The property delaration syntax to check.</param>
        /// <returns>If the property has a setter.</returns>
        public static bool HasSetter(this PropertyDeclarationSyntax syntax) => HasAccessor(syntax, SetterAccessor);

        private static bool HasModifier(MemberDeclarationSyntax syntax, string modifier)
        {
            return syntax.Modifiers.Any(mod => mod.Text == modifier);
        }

        private static bool HasAccessor(PropertyDeclarationSyntax syntax, string accessor)
        {
            return syntax.AccessorList.Accessors.Any(acc => acc.Keyword.Text == accessor);
        }
    }
}
