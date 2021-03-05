namespace DecoMaker.Templating
{
    /// <summary>
    /// Selection rule regarding parameter types for method templates.
    /// </summary>
    internal enum ParamTypeRule
    {
        /// <summary>
        /// Can be selected for methods with any set of parameters.
        /// </summary>
        Any,

        /// <summary>
        /// Can be selected for methods which have a specific set of parametrs.
        /// </summary>
        Specified
    }
}
