using System.Collections.Generic;

namespace DecoMaker.Templating
{
    /// <summary>
    /// Condition to be satisfied by a method being decorated when determing the tempmlate to use to generate that methods decorator.
    /// </summary>
    internal class MethodTemplateMatchCondition
    {
        /// <summary>
        /// The rule for the condition regarding the return type.
        /// </summary>
        public ReturnTypeRule ReturnTypeRule { get; set; }

        /// <summary>
        /// The return type for when <see cref="ReturnTypeRule"/> is <see cref="ReturnTypeRule.Specified"/>.
        /// </summary>
        public string ReturnType { get; set; }

        /// <summary>
        /// The rule for the condition regarding the parameter types.
        /// </summary>
        public ParamTypeRule ParamTypeRule { get; set; }

        /// <summary>
        /// The parameter types for when <see cref="ParamTypeRule"/> is <see cref="ParamTypeRule.Specified"/>.
        /// </summary>
        public IDictionary<string, string> Params { get; set; }
    }
}
