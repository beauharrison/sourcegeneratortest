namespace DecoMaker.Templating
{
    /// <summary>
    /// Selection rule regarding property type for property templates.
    /// </summary>
    internal enum PropertyTypeRule
    {
        /// <summary>
        /// Can be selected for properties with any type.
        /// </summary>
        Any,

        /// <summary>
        /// Can be selected for properties of a specific type.
        /// </summary>
        Specified
    }
}
