using Generators.STEvent;
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

    internal class StaticMethodCallSyntaxReceiver : ISyntaxReceiver
    {
        private readonly string _Namespace;
        private readonly string _Class;
        private readonly HashSet<string> _Methods;
        private readonly int? _MinArgCount;
        private readonly int? _MaxArgCount;
        private readonly int? _MinGenericsCount;
        private readonly int? _MaxGenericsCount;

        public StaticMethodCallSyntaxReceiver(string @namespace, string @class, string method, int? minArgCount = null, int? maxArgCount = null, int? minGenericsCount = null, int? maxGenericsCount = null)
        {
            _Namespace = @namespace;
            _Class = @class;
            _Methods = new HashSet<string>(new[] { method });
            _MinArgCount = minArgCount;
            _MaxArgCount = maxArgCount;
            _MinGenericsCount = minGenericsCount;
            _MaxGenericsCount = maxGenericsCount;
        }
        public StaticMethodCallSyntaxReceiver(string @namespace, string @class, string[] methods, int? minArgCount = null, int? maxArgCount = null, int? minGenericsCount = null, int? maxGenericsCount = null)
        {
            _Namespace = @namespace;
            _Class = @class;
            _Methods = new HashSet<string>(methods);
            _MinArgCount = minArgCount;
            _MaxArgCount = maxArgCount;
            _MinGenericsCount = minGenericsCount;
            _MaxGenericsCount = maxGenericsCount;
        }

        public List<StaticMethodCall> Calls = new List<StaticMethodCall>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is InvocationExpressionSyntax invocation &&
                invocation.Expression is MemberAccessExpressionSyntax methodMemberAccess)
            {
                string className;
                ArgumentSyntax[] arguments = invocation.ArgumentList.Arguments.ToArray();

                TypeSyntax[] genericTypes = (methodMemberAccess.Name is GenericNameSyntax genericName)
                    ? genericName.TypeArgumentList.Arguments.ToArray()
                    : new TypeSyntax[0];

                if (_MinArgCount.HasValue && arguments.Length < _MinArgCount.Value ||
                    _MaxArgCount.HasValue && arguments.Length > _MaxArgCount)
                {
                    return;
                }

                if (_MinGenericsCount.HasValue && genericTypes.Length < _MinGenericsCount.Value ||
                    _MaxGenericsCount.HasValue && genericTypes.Length > _MaxGenericsCount)
                {
                    return;
                }

                if (methodMemberAccess.Expression is MemberAccessExpressionSyntax innerMemberAccess)
                {
                    className = innerMemberAccess.Name.Identifier.ValueText;
                }
                else if (methodMemberAccess.Expression is IdentifierNameSyntax methodIdentifier)
                {
                    className = methodIdentifier.Identifier.ValueText;
                }
                else
                {
                    return;
                }
                                
                string methodName = methodMemberAccess.Name.Identifier.ValueText;

                if (string.Equals(className, _Class) && _Methods.Contains(methodName))
                {
                    Calls.Add(new StaticMethodCall
                    {
                        Invocation = invocation,
                        Method = methodName,
                        Arguments = arguments.Select(argEx => argEx.Expression).ToArray(),
                        GenericTypes = genericTypes
                    });
                }

                //if (children.Length == 2 &&
                //    children[0] is MemberAccessExpressionSyntax classMemberAccess &&
                //    children[1] is IdentifierNameSyntax methodIdentifier &&
                //    classMemberAccess.Expression is IdentifierNameSyntax @namespace)
                //{
                //    string methodName = methodIdentifier.Identifier.ValueText;
                //    string className = classMemberAccess.Name.Identifier.ValueText;
                //    string namespaceName = @namespace.Identifier.ValueText;

                //    if (string.Equals(namespaceName, _Namespace) && string.Equals(className, _Class) && string.Equals(methodName, _Method))
                //    {
                //        Calls.Add((
                //            invocation,
                //            arguments.Select(argEx => argEx.Expression).ToArray()
                //        ));
                //    }
                //}
            }
        }
    }
}
