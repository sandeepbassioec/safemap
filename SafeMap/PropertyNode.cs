using System.Reflection;

namespace SafeMap
{
    internal class PropertyNode : IPathNode
    {
        private readonly PropertyInfo _prop;
        
        public PropertyNode(PropertyInfo prop) => _prop = prop;

        public object? GetValue(object instance) => _prop.GetValue(instance);
    }
}