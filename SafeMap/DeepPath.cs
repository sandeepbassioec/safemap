namespace SafeMap
{
    /// <summary>
    ///  DeepPath
    /// This class helps safely reach deeply nested properties.
    /// Developers often write:
    ///
    ///    var s = person?.Address?.Location?.StreetName;
    ///
    /// But real code is worse because each step has transformations or checks.
    ///
    /// DeepPath makes it readable, safe and reusable:
    /// 
    ///    DeepPath.To(person)
    ///        .Go(p => p.Address)
    ///        .Go(a => a.Location)
    ///        .Go(l => l.StreetName)
    ///        .Value();
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class DeepPath<T> where T : class
    {
        private readonly SafeValue<T> _current;

        internal DeepPath(SafeValue<T> current) => _current = current;

        /// <summary>
        /// Start with a root object always.
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public static DeepPath<T> To(T? root) => new DeepPath<T>(Safe.Guard(root));

        /// <summary>
        /// Navigate to the next reference type safely.
        /// </summary>
        /// <typeparam name="TNext"></typeparam>
        /// <param name="projector"></param>
        /// <returns></returns>
        public DeepPath<TNext> Go<TNext>(Func<T, TNext?> projector) where TNext : class
        {
            var next = _current.Map(projector);

            return new DeepPath<TNext>(next);
        }

        /// <summary>
        /// Navigate to the next nullable struct
        /// </summary>
        /// <typeparam name="TNext"></typeparam>
        /// <param name="projector"></param>
        /// <returns></returns>
        public DeepPathStruct<TNext> Go<TNext>(Func<T, TNext?> projector) where TNext : struct
        {
            var next = _current.Map(projector);

            return new DeepPathStruct<TNext>(next);
        }

        /// <summary>
        /// The final value.
        /// </summary>
        /// <returns></returns>
        public T? Value() => _current.Value();
    }
}