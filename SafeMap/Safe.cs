using System.Linq.Expressions;
using System.Reflection;

namespace SafeMap
{
    /// <summary>
    ///  ENTRY POINT STATIC CLASS
    ///
    /// The "Safe" class is how to start using the Safe_Map pattern.
    /// Its like a "factory" for creating safe wrappers.
    ///
    /// Safe.Guard(obj)        → wraps reference types
    /// Safe.GuardStruct(obj)  → wraps nullable value types
    /// Safe.GuardAsync(task)  → wraps async values
    /// Safe.FromCollection()  → wraps collections
    /// </summary>
    public static class Safe
    {
        public static SafeValue<T> Guard<T>(T? value) where T : class
            => new SafeValue<T>(value, value != null);

        public static SafeValueStruct<T> GuardStruct<T>(T? value) where T : struct
            => new SafeValueStruct<T>(value, value.HasValue);

        public static SafeAsync<T> GuardAsync<T>(Task<T?> task) where T : class
            => new SafeAsync<T>(task);

        public static SafeCollection<T> FromCollection<T>(IEnumerable<T?> items) where T : class
            => new SafeCollection<T>(items);

        /// <summary>
        /// Safe.Path - expression-based deep path
        /// Usage:
        ///   var x = Safe.Path(person, p => p.Address.Location.StreetName);
        ///   
        /// This will traverse properties step-by-step and stop safely if any intermediate is null.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static TResult? Path<TSource, TResult>(TSource? source, Expression<Func<TSource, TResult>> path)
            where TSource : class
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (source == null) return default;

            // Evaluate path step by step
            var nodes = ExpressionPathHelper.ExtractPath(path.Body);
            object? current = source;
            foreach (var node in nodes)
            {
                if (current == null) return default;
                try
                {
                    current = node.GetValue(current);
                }
                catch
                {
                    // if any reflection / invocation fails, return null (safe)
                    return default;
                }
            }

            // final value may be boxed; if it's TResult or convertible, return
            return current is TResult t ? t : default;
        }

        /// <summary>
        /// Safe.Path - string-based deep path
        /// Usage:
        ///   var x = Safe.Path(person, "Address.Location.StreetName");
        /// Behavior depends on SafePathOptions.ThrowOnMissingProperty
        /// </summary>
        /// <param name="source"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static object? Path(object? source, string path)
        {
            if (source == null) return null;
            if (string.IsNullOrWhiteSpace(path)) return null;

            var parts = path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            object? current = source;
            var type = source.GetType();

            foreach (var propName in parts)
            {
                if (current == null) return null;

                var prop = type.GetProperty(propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (prop == null)
                {
                    if (SafePathOptions.ThrowOnMissingProperty)
                        throw new InvalidOperationException($"Property '{propName}' not found on type {type.FullName}");
                    
                    return null;
                }

                try
                {
                    current = prop.GetValue(current);

                    if (current == null) 
                        return null;
                    
                    type = current.GetType();
                }
                catch
                {
                    if (SafePathOptions.ThrowOnMissingProperty)
                        throw;
                    return null;
                }
            }

            return current;
        }
    }
}