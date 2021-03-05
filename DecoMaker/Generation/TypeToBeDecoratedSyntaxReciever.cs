using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DecoMaker.Generation
{
    internal class TypeToBeDecoratedSyntaxReciever : ISyntaxReceiver
    {
        public List<TypeToBeDecorated> Classes = new List<TypeToBeDecorated>();

        private static readonly string DecorateAttributeName = "Decorate";

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is TypeDeclarationSyntax { AttributeLists: { Count: >= 1 } } typeDeclaration)
            {
                AttributeSyntax[] attributeSyntax = typeDeclaration.AttributeLists
                    .SelectMany(attrList => attrList.Attributes)
                    .Select(attr => (attr, GetAttributeIdentifierName(attr.Name)))
                    .Where(attr => attr.Item2.Identifier.Text.StartsWith(DecorateAttributeName))
                    .Select(attr => attr.Item1)
                    .ToArray();

                if (attributeSyntax != null)
                {
                    Classes.Add(new TypeToBeDecorated 
                    { 
                        TypeDeclaration = typeDeclaration, 
                        DecorateAttributes = attributeSyntax 
                    });
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
