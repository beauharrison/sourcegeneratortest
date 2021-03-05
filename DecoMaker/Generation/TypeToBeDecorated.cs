using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DecoMaker.Generation
{
    /// <summary>
    /// A type to be decorated.
    /// </summary>
    internal class TypeToBeDecorated
    {
        /// <summary>
        /// The delaration of a type. Should be either <see cref="InterfaceDeclarationSyntax"/> or <see cref="ClassDeclarationSyntax"/>
        /// </summary>
        public TypeDeclarationSyntax TypeDeclaration { get; set; }

        /// <summary>
        /// Syntax of attribute which are possibly <see cref="DecorateAttribute"/>.
        /// </summary>
        public AttributeSyntax[] DecorateAttributes { get; set; }
    }
}
