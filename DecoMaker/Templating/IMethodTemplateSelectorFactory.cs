using System.Collections.Generic;

namespace DecoMaker.Templating
{
    /// <summary>
    /// Factory for creating <see cref="IMethodTemplateSelector"/>.
    /// </summary>
    internal interface IMethodTemplateSelectorFactory
    {
        /// <summary>
        /// Create the <see cref="IMethodTemplateSelector"/>
        /// </summary>
        /// <param name="methodTemplates">The method templates.</param>
        /// <returns>The selector.</returns>
        IMethodTemplateSelector Create(IEnumerable<MethodTemplate> methodTemplates);
    }
}
