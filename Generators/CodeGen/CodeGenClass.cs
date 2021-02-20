using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeGen
{
    public class CodeGenClass : ICodeGenElement
    {
        public CodeGenClass(
            string name, 
            Scope scope, 
            ClassType classType,
            IEnumerable<CodeGenGeneric> genericTypes = null,
            string[] derivedFrom = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Scope = scope;
            ClassType = classType;
            GenericTypes = genericTypes ?? new CodeGenGeneric[0];
            DerivedFrom = derivedFrom ?? new string[0];
        }

        public string Name { get; }

        public Scope Scope { get; }

        public ClassType ClassType { get; }

        public IEnumerable<CodeGenGeneric> GenericTypes { get; }

        public string[] DerivedFrom { get; }

        public CodeGenComment Comment { get; set; }

        public List<CodeGenVariable> Variables { get; } = new List<CodeGenVariable>();

        public List<CodeGenProperty> Properties { get; } = new List<CodeGenProperty>();

        public List<CodeGenMethod> Methods { get; } = new List<CodeGenMethod>();

        public List<CodeGenConstructor> Constructors { get; } = new List<CodeGenConstructor>();

        public List<CodeGenAttribute> Attributes { get; } = new List<CodeGenAttribute>();

        public string GenerateCode(CodeGenStyle style = null)
        {
            if (style == null) style = new CodeGenStyle();

            var builder = new StringBuilder();

            if (Comment != null)
            {
                builder.AppendLine(Comment.GenerateCode(style));
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

            var genericList = GenericTypes.Any() ? $"<{string.Join(", ", GenericTypes.Select(gt => gt.Name))}>" : string.Empty;

            builder.Append($"class {Name}{genericList}");

            if (DerivedFrom.Any())
            {
                var derivedFromList = string.Join(", ", DerivedFrom);
                builder.Append($" : {derivedFromList}");
            }

            builder.AppendLine();

            foreach (var constrainedGeneric in GenericTypes.Where(gt => gt.Constraint != null))
            {
                style.IndentCount++;
                builder.AppendLine($"{style.Indent}where {constrainedGeneric.Name} : {constrainedGeneric.Constraint}");
                style.IndentCount--;
            }

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
