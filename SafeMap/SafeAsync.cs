namespace SafeMap
{
    /// <summary>
    ///  SAFE ASYNC WRAPPER
    ///
    /// Same logic as SafeValue<T>, but for async values.
    /// Useful when you fetch data from DB or API and still want Safe-Map chaining.
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class SafeAsync<T> where T : class
    {
        private readonly Task<T?> _task;
        private bool _isFaulted;

        internal SafeAsync(Task<T?> task)
        {
            _task = task;
        }

        /// <summary>
        /// Async project which returns Task<TResult?>
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="projector"></param>
        /// <returns></returns>
        public async Task<SafeValue<TResult>> MapAsync<TResult>(Func<T, Task<TResult?>> projector) where TResult : class
        {
            try
            {
                var v = await _task.ConfigureAwait(false);
                
                if (v == null) 
                    return new SafeValue<TResult>(null, false);
                
                var r = await projector(v).ConfigureAwait(false);
                
                return new SafeValue<TResult>(r, r != null);
            }
            catch
            {
                return new SafeValue<TResult>(null, false);
            }
        }

        /// <summary>
        /// Async to sync projector
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="projector"></param>
        /// <returns></returns>
        public async Task<SafeValue<TResult>> MapAsync<TResult>(Func<T, TResult?> projector) where TResult : class
        {
            try
            {
                var v = await _task.ConfigureAwait(false);

                if (v == null) 
                    return new SafeValue<TResult>(null, false);

                var r = projector(v);

                return new SafeValue<TResult>(r, r != null);
            }
            catch
            {
                return new SafeValue<TResult>(null, false);
            }
        }

        public async Task<T?> ValueAsync()
        {
            try 
            { 
                return await _task.ConfigureAwait(false); 
            }
            catch 
            { 
                return default; 
            }
        }
    }
}