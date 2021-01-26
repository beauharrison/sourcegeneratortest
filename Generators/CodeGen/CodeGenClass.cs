using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeGen
{
    public class CodeGenClass : ICodeGenElement
    {
        public CodeGenClass(string name, Scope scope, ClassType classType, string[] derivedFrom = null, string comment = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Scope = scope;
            ClassType = classType;
            DerivedFrom = derivedFrom ?? new string[0];
            Comment = comment;
        }

        public string Name { get; }

        public Scope Scope { get; }

        public ClassType ClassType { get; }

        public string[] DerivedFrom { get; }

        public string Comment { get; }

        public List<CodeGenVariable> Variables { get; } = new List<CodeGenVariable>();

        public List<CodeGenProperty> Properties { get; } = new List<CodeGenProperty>();

        public List<CodeGenMethod> Methods { get; } = new List<CodeGenMethod>();

        public List<CodeGenConstructor> Constructors { get; } = new List<CodeGenConstructor>();

        public List<CodeGenAttribute> Attributes { get; } = new List<CodeGenAttribute>();

        public string GenerateCode(CodeGenStyle style = null)
        {
            if (style == null) style = new CodeGenStyle();

            var builder = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(Comment))
            {
                builder.AppendLine(style.FormatComment(Comment));
            }

            foreach (var attribute in Attributes)
            {
                builder.AppendLine(attribute.GenerateCode(style));
            }

            builder.Append(style.Indent);
            builder.Append(Scope.ToString().ToLower());
            builder.Append(" ");

            if (ClassType != ClassType.Normal)
            {
                builder.Append(ClassType.ToString().ToLower());
                builder.Append(" ");
            }

            builder.Append($"class {Name}");

            if (DerivedFrom.Any())
            {
                var derivedFromList = string.Join(", ", DerivedFrom);
                builder.Append($" : {derivedFromList}");
            }

            builder.AppendLine();
            builder.AppendLine($"{style.Indent}{{");

            style.IndentCount++;

            foreach (var constructor in Constructors)
            {
                builder.AppendLine(constructor.GenerateCode(style));
                builder.AppendLine();
            }

            foreach (var variable in Variables)
            {
                builder.AppendLine(variable.GenerateCode(style));
                builder.AppendLine();
            }

            foreach (var property in Properties)
            {
                builder.AppendLine(property.GenerateCode(style));
                builder.AppendLine();
            }

            foreach (var method in Methods)
            {
                builder.AppendLine(method.GenerateCode(style));
                builder.AppendLine();
            }

            style.IndentCount--;

            builder.Append($"{style.Indent}}}");
            
            return builder.ToString();
        }
    }
}
