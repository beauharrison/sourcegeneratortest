using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Generators
{
    internal class ClassWithAttributeSyntaxReciever : ISyntaxReceiver
    {
        private HashSet<string> _AttributeNames;

        public ClassWithAttributeSyntaxReciever(params Type[] attributeTypes)
        {
            _AttributeNames = new HashSet<string>(attributeTypes.Select(GetAttributeName));
        }

        public List<(ClassDeclarationSyntax classDelaration, AttributeSyntax attributeName)> Classes = 
            new List<(ClassDeclarationSyntax classDelaration, AttributeSyntax attributeName)>();
        
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax { AttributeLists: { Count: >= 1 } } classDeclaration)
            {
                AttributeSyntax attributeSyntax = classDeclaration.AttributeLists
                    .SelectMany(attrList => attrList.Attributes)
                    .Select(attr => (attr, GetAttributeIdentifierName(attr.Name)))
                    .Where(attr => _AttributeNames.Contains(attr.Item2.Identifier.Text))
                    .Select(attr => attr.Item1)
                    .FirstOrDefault();

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

        private string GetAttributeName(Type type) => type.Name.Length > 9 && type.Name.EndsWith("Attribute")
            ? type.Name.Substring(0, type.Name.Length - 9)
            : type.Name;
    }
}
