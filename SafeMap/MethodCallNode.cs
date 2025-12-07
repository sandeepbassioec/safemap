using System.Reflection;

namespace SafeMap
{
    internal class MethodCallNode : IPathNode
    {
        private readonly MethodInfo _method;
        private readonly object?[] _args;
        
        public MethodCallNode(MethodInfo method, object?[] args)
        {
            _method = method;
            _args = args;
        }
 
        public object? GetValue(object instance) => _method.Invoke(instance, _args);
    }
}