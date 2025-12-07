namespace SafeMap
{
    /// <summary>
    /// Extension methods to convert regular values into SafeMapX types
    /// </summary>
    public static class SafeExtensions
    {
        public static SafeValueStruct<T> ToSafeStruct<T>(this T? v) where T : struct
            => Safe.GuardStruct(v);

        public static SafeValue<T> ToSafe<T>(this T? v) where T : class
            => Safe.Guard(v);

        public static SafeCollection<T> ToSafeCollection<T>(this IEnumerable<T?> items) where T : class
            => Safe.FromCollection(items);
    }
}