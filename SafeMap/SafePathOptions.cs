namespace SafeMap
{
    /// <summary>
    /// Config options for DeepPath
    /// </summary>
    public static class SafePathOptions
    {
        /// <summary>
        /// If true -> when a string path contains a missing property name,
        /// throw an exception. 
        /// If false -> return null silently.
        /// </summary>
        public static bool ThrowOnMissingProperty { get; set; } = false;
    }
}