namespace SafeMap
{
    /// <summary>
    ///  SAFE VALUE FOR REFERENCE TYPES
    ///
    /// This wrapper holds a reference type and ensures no null-related crashes.
    /// Every .Map() call checks:
    ///     - If the current value exists
    ///     - If the projector function crashes
    ///     - If the returned value is valid
    ///
    /// If something fails → the SafeValue becomes empty (HasValue = false)
    /// but the code never throws.
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class SafeValue<T> where T : class
    {
        private readonly T? _value;
        private readonly bool _hasValue;
        private bool _hasDefault;
        private object? _defaultValue;
        internal bool _isFaulted;

        internal SafeValue(T? value, bool hasValue)
        {
            _value = value;
            _hasValue = hasValue;
        }

        /// <summary>
        /// Map to reference TResult type
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="projector"></param>
        /// <returns></returns>
        public SafeValue<TResult> Map<TResult>(Func<T, TResult?> projector) where TResult : class
        {
            if (_isFaulted) 
                return new SafeValue<TResult>(null, false) { _isFaulted = true };
            
            if (_hasValue && _value != null)
            {
                try
                {
                    var r = projector(_value);
            
                    return new SafeValue<TResult>(r, r != null);
                }
                catch
                {
                    return new SafeValue<TResult>(null, false) { _isFaulted = true };
                }
            }

            return new SafeValue<TResult>(null, false);
        }

        /// <summary>
        /// Map to nullable struct TResult? where TResult : struct
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="projector"></param>
        /// <returns></returns>
        public SafeValueStruct<TResult> Map<TResult>(Func<T, TResult?> projector) where TResult : struct
        {
            if (_isFaulted) 
                return new SafeValueStruct<TResult>(null, false) { _isFaulted = true };
            
            if (_hasValue && _value != null)
            {
                try
                {
                    var r = projector(_value);
            
                    return new SafeValueStruct<TResult>(r, r.HasValue);
                }
                catch
                {
                    return new SafeValueStruct<TResult>(null, false) { _isFaulted = true };
                }
            }

            return new SafeValueStruct<TResult>(null, false);
        }

        public SafeValue<TResult> Transform<TResult>(Func<T, TResult?> projector) where TResult : class
            => Map(projector);

        public SafeValue<T> Fallback(Func<T?> fallback)
        {
            if (_isFaulted) 
                return new SafeValue<T>(null, false) { _isFaulted = true };

            if (_hasValue && _value != null) 
                return this;
            
            try
            {
                var fb = fallback();
            
                return new SafeValue<T>(fb, fb != null);
            }
            catch
            {
                return new SafeValue<T>(null, false) { _isFaulted = true };
            }
        }

        public SafeValue<T> Fallback(SafeValue<T> other)
        {
            if (_isFaulted) 
                return new SafeValue<T>(null, false) { _isFaulted = true };
            
            return (_hasValue && _value != null) ? this : other;
        }

        public SafeValue<T> Default(T fallback)
        {
            _hasDefault = true;
            _defaultValue = fallback;

            return this;
        }

        public T? Value()
        {
            if (_isFaulted) return default;
            if (_hasValue && _value != null) return _value;
            if (_hasDefault) return (T?)_defaultValue;
            
            return default;
        }

        public T ValueOrThrow(Func<Exception>? exFactory = null)
        {
            if (_isFaulted) throw new InvalidOperationException("SafeValue is faulted");
            if (_hasValue && _value != null) return _value;
            if (_hasDefault && _defaultValue != null) return (T)_defaultValue;
            if (exFactory != null) throw exFactory();
            
            throw new InvalidOperationException("No value present");
        }

        public bool TryGet(out T? found)
        {
            if (_isFaulted) { found = default; return false; }
            if (_hasValue && _value != null) { found = _value; return true; }
            if (_hasDefault) { found = (T?)_defaultValue; return true; }

            found = default;
            return false;
        }
    }
}