using System;

namespace DecoMaker
{
    /// <summary>
    /// Attribute used to indicate a class or interface should have a decorator created for it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class DecorateAttribute : Attribute
    {
        /// <summary>
        /// Constructor for this attribute
        /// </summary>
        /// <param name="name">Identifier that, in conjunction with the name of the type being decorated, is used to name the generated deocrator.</param>
        /// <param name="template">The type of the the template class to use to generate the decorator.</param>
        public DecorateAttribute(string name, Type template)
        {
        }

        /// <summary>
        /// Constructor for this attribute
        /// </summary>
        /// <param name="name">Identifier that, in conjunction with the name of the type being decorated, is used to name the generated deocrator.</param>
        /// <param name="template">The type of the the template class to use to generate the decorator.</param>
        /// <param name="implements">A type that the genrated decorator will implement, such as an interface. Note the generated interface must naturally satify this implementation.</param>
        public DecorateAttribute(string name, Type template, Type implements)
        {
        }
    }
}
