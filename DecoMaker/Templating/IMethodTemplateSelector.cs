namespace DecoMaker.Templating
{
    /// <summary>
    /// Selects the best method template for a method being decorated.
    /// </summary>
    internal interface IMethodTemplateSelector
    {
        /// <summary>
        /// Select the best method template for a method.
        /// </summary>
        /// <param name="methodName">The name of the method being decorated.</param>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="paramTypes">The parameter types of the method.</param>
        /// <param name="isAsync">If the method is async.</param>
        /// <returns>The selected best method template, or null if there was none.</returns>
        MethodTemplate Select(string methodName, string returnType, string[] paramTypes, bool isAsync);
    }
}