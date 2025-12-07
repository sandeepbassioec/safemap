namespace SafeMap
{
    public static class SafeTextExtensions
    {
        public static SafeText ToSafeText(this string? s) => 
            SafeText.From(s);

        public static SafeText ToSafeText<T>(this SafeValue<T> sv) where T : class
        {
            if (sv == null) 
                return SafeText.From((string?)null);
            
            return SafeText.From(sv.Value());
        }
    }
}