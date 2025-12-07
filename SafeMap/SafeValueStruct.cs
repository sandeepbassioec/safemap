namespace SafeMap
{
    /// <summary>
    ///  SAFE VALUE FOR NULLABLE STRUCTS
    ///
    /// Same logic as SafeValue<T>, but for value types like:
    ///     int?, DateTime?, decimal?, bool?, Guid?, etc.
    ///
    /// We treat them differently because nullable structs behave differently
    /// than reference types in .NET.
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class SafeValueStruct<T> where T : struct
    {
        private readonly T? _value;
        private readonly bool _hasValue;
        private bool _hasDefault;
        private T _defaultValue = default!;
        internal bool _isFaulted;

        internal SafeValueStruct(T? value, bool hasValue)
        {
            _value = value;
            _hasValue = hasValue;
        }

        /// <summary>
        /// Map from struct -> struct?
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="projector"></param>
        /// <returns></returns>
        public SafeValueStruct<TResult> Map<TResult>(Func<T, TResult?> projector) where TResult : struct
        {
            if (_isFaulted) 
                return new SafeValueStruct<TResult>(null, false) { _isFaulted = true };
            
            if (_hasValue && _value.HasValue)
            {
                try
                {
                    var r = projector(_value.Value);
            
                    return new SafeValueStruct<TResult>(r, r.HasValue);
                }
                catch
                {
                    return new SafeValueStruct<TResult>(null, false) { _isFaulted = true };
                }
            }

            return new SafeValueStruct<TResult>(null, false);
        }

        /// <summary>
        /// Map from struct -> reference type
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="projector"></param>
        /// <returns></returns>
        public SafeValue<TResult> Map<TResult>(Func<T, TResult?> projector) where TResult : class
        {
            if (_isFaulted) 
                return new SafeValue<TResult>(null, false) { _isFaulted = true };
            
            if (_hasValue && _value.HasValue)
            {
                try
                {
                    var r = projector(_value.Value);
            
                    return new SafeValue<TResult>(r, r != null);
                }
                catch
                {
                    return new SafeValue<TResult>(null, false) { _isFaulted = true };
                }
            }

            return new SafeValue<TResult>(null, false);
        }

        public SafeValueStruct<T> Fallback(Func<T?> fallback)
        {
            if (_isFaulted) 
                return new SafeValueStruct<T>(null, false) { _isFaulted = true };
            
            if (_hasValue && _value.HasValue) 
                return this;
            
            try
            {
                var fb = fallback();
                
                return new SafeValueStruct<T>(fb, fb.HasValue);
            }
            catch
            {
                return new SafeValueStruct<T>(null, false) { _isFaulted = true };
            }
        }

        public SafeValueStruct<T> Default(T fallback)
        {
            _hasDefault = true;
            _defaultValue = fallback;
            
            return this;
        }

        public T Value()
        {
            if (_isFaulted) throw new InvalidOperationException("SafeValueStruct is faulted");
            if (_hasValue && _value.HasValue) return _value.Value;
            if (_hasDefault) return _defaultValue;
            
            return default!;
        }

        public bool TryGet(out T found)
        {
            if (_isFaulted) { found = default!; return false; }
            if (_hasValue && _value.HasValue) { found = _value.Value; return true; }
            if (_hasDefault) { found = _defaultValue; return true; }
            
            found = default!;
            return false;
        }
    }
}