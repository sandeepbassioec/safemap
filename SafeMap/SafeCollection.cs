namespace SafeMap
{
    /// <summary>
    ///  SAFE COLLECTION WRAPPER
    ///
    /// Lets you apply Safe-Map logic to collections.
    /// Any failure in mapping of a single item is safely ignored.
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class SafeCollection<T> where T : class
    {
        private readonly IEnumerable<T?> _items;

        internal SafeCollection(IEnumerable<T?> items)
        {
            _items = items ?? Enumerable.Empty<T?>();
        }

        public IEnumerable<TResult> SafeSelect<TResult>(Func<T, TResult?> projector) where TResult : class
        {
            foreach (var item in _items)
            {
                if (item == null) 
                    continue;
                
                TResult? result = null;
                
                try 
                { 
                    result = projector(item); 
                } 
                catch 
                { 
                    result = null; 
                }
                
                if (result != null) 
                    yield return result;
            }
        }

        public TResult? SafeFirstOrDefault<TResult>(Func<T, TResult?> projector) where TResult : class
        {
            foreach (var item in _items)
            {
                if (item == null) 
                    continue;
                
                TResult? result;

                try 
                { 
                    result = projector(item); 
                } 
                catch 
                { 
                    result = null; 
                }
                
                if (result != null) 
                    return result;
            }

            return default;
        }

        public IEnumerable<SafeValue<TResult>> MapToSafe<TResult>(Func<T, TResult?> projector) where TResult : class
        {
            foreach (var item in _items)
            {
                if (item == null) 
                { 
                    yield return new SafeValue<TResult>(null, false); 
                    continue; 
                }
                
                TResult? result = null;
                try 
                { 
                    result = projector(item); 
                } 
                catch 
                { 
                    result = null; 
                }
                
                yield return new SafeValue<TResult>(result, result != null);
            }
        }
    }
}