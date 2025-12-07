namespace SafeMap
{
    /// <summary>
    /// DeepNavigator
    /// This is a safe workflow builder.
    /// It creates a pipeline of steps that execute one after another,
    /// but WITHOUT if/else or manual null checks.
    ///
    /// Example:
    ///
    /// var dto = DeepNavigator
    ///     .Start(orderId)
    ///     .Step(id => repo.GetOrder(id))
    ///     .Step(order => order.Customer)
    ///     .Step(customer => repo.GetProfile(customer.Id))
    ///     .Finish();
    ///
    /// If ANY step returns null or throws → navigation stops safely.
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    public sealed class DeepNavigator<TIn> where TIn : class
    {
        private readonly SafeValue<TIn> _current;

        private DeepNavigator(SafeValue<TIn> current) { _current = current; }

        /// <summary>
        /// Start with an object instance
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static DeepNavigator<TIn> Start(TIn? input) => new DeepNavigator<TIn>(Safe.Guard(input));

        /// <summary>
        /// Synchronous step to next reference type
        /// </summary>
        /// <typeparam name="TNext"></typeparam>
        /// <param name="projector"></param>
        /// <returns></returns>
        public DeepNavigator<TNext> Step<TNext>(Func<TIn, TNext?> projector) where TNext : class
        {
            var next = _current.Map(projector);

            return new DeepNavigator<TNext>(next);
        }

        /// <summary>
        /// Async step that uses current.Value()
        /// </summary>
        /// <typeparam name="TNext"></typeparam>
        /// <param name="projector"></param>
        /// <returns></returns>
        public async Task<DeepNavigator<TNext>> StepAsync<TNext>(Func<TIn, Task<TNext?>> projector) where TNext : class
        {
            var currentValue = _current.Value();
            if (currentValue == null)
                return new DeepNavigator<TNext>(new SafeValue<TNext>(null, false));

            try
            {
                var output = await projector(currentValue);
                return new DeepNavigator<TNext>(Safe.Guard(output));
            }
            catch
            {
                return new DeepNavigator<TNext>(new SafeValue<TNext>(null, false));
            }
        }

        /// <summary>
        /// Finish and return the final value
        /// </summary>
        /// <returns></returns>
        public TIn? Finish() => _current.Value();
    }
}