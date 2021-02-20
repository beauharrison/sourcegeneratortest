using System;

namespace CodeGen
{
    public class CodeGenGeneric
    {
        public CodeGenGeneric(string name, string constraint = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Constraint = constraint;
        }

        public string Name { get; }

        public string Constraint { get; }
    }
}
