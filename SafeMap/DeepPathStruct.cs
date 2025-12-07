namespace SafeMap
{
    /// <summary>
    /// For nullable structs (DateTime?, int?, etc.)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class DeepPathStruct<T> where T : struct
    {
        private readonly SafeValueStruct<T> _current;

        internal DeepPathStruct(SafeValueStruct<T> current) => _current = current;

        public DeepPath<TNext> Go<TNext>(Func<T, TNext?> projector) where TNext : class
        {
            var next = _current.Map(projector);
            return new DeepPath<TNext>(next);
        }

        public DeepPathStruct<TNext> Go<TNext>(Func<T, TNext?> projector) where TNext : struct
        {
            var next = _current.Map(projector);
            return new DeepPathStruct<TNext>(next);
        }

        public T Value() => _current.Value();
    }
}