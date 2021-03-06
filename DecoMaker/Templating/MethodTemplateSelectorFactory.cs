using System.Collections.Generic;

namespace DecoMaker.Templating
{
    /// <summary>
    /// Factory for creating <see cref="MethodTemplateSelector"/>.
    /// </summary>
    internal class MethodTemplateSelectorFactory : IMethodTemplateSelectorFactory
    {
        /// <inheritdoc />
        public IMethodTemplateSelector Create(IEnumerable<MethodTemplate> methodTemplates)
        {
            return new MethodTemplateSelector(methodTemplates);
        }
    }
}
