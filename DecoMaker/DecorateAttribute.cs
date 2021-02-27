using System;

namespace DecoMaker
{
    public class DecorateAttribute : Attribute
    {
        public DecorateAttribute(string name, Type template)
        {
        }
        public DecorateAttribute(string name, Type template, Type implements)
        {
        }
    }
}
