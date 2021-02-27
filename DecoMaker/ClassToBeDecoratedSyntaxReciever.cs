using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DecoMaker
{
    internal class ClassToBeDecoratedSyntaxReciever : ISyntaxReceiver
    {
        public List<(ClassDeclarationSyntax classDelaration, AttributeSyntax[] decorateAttributes)> Classes =
            new List<(ClassDeclarationSyntax classDelaration, AttributeSyntax[] decorateAttributes)>();

        private static readonly string DecorateAttributeName = "Decorate";

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax { AttributeLists: { Count: >= 1 } } classDeclaration)
            {
                AttributeSyntax[] attributeSyntax = classDeclaration.AttributeLists
                    .SelectMany(attrList => attrList.Attributes)
                    .Select(attr => (attr, GetAttributeIdentifierName(attr.Name)))
                    .Where(attr => attr.Item2.Identifier.Text.StartsWith(DecorateAttributeName))
                    .Select(attr => attr.Item1)
                    .ToArray();

                if (attributeSyntax != null)
                {
                    Classes.Add((classDeclaration, attributeSyntax));
                }
            }
        }

        private IdentifierNameSyntax GetAttributeIdentifierName(NameSyntax attributeNameSyntax)
        {
            return attributeNameSyntax switch
            {
                QualifiedNameSyntax qns => GetAttributeIdentifierName(qns.Right),
                IdentifierNameSyntax ins => ins,
                _ => throw new NotSupportedException(attributeNameSyntax.GetType().Name)
            };
        }
    }
}
