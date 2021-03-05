namespace DecoMaker.Templating
{
    /// <summary>
    /// Selection rule regarding return type for method templates.
    /// </summary>
    internal enum ReturnTypeRule
    {
        /// <summary>
        /// Can be selected for methods with any return type.
        /// </summary>
        Any,

        /// <summary>
        /// Can be selected for methods which return a specific type.
        /// </summary>
        Specified
    }
}
