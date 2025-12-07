using System.Linq.Expressions;
using System.Reflection;

namespace SafeMap
{
    /// <summary>
    /// Helper used by expression-based path traversal
    /// It converts an expression like x => x.Address.Location.StreetName
    /// into a list of nodes each able to "GetValue" from an object.
    /// </summary>
    internal static class ExpressionPathHelper
    {
        internal static List<IPathNode> ExtractPath(Expression expr)
        {
            var nodes = new List<IPathNode>();
            while (expr != null)
            {
                if (expr is MemberExpression m)
                {
                    if (m.Member is PropertyInfo prop)
                        nodes.Insert(0, new PropertyNode(prop));
                    
                    expr = m.Expression!;
                    continue;
                }

                if (expr is MethodCallExpression mc)
                {
                    var mi = mc.Method;
                    // only support simple instance method calls without parameters or with constant args
                    var args = mc.Arguments.Select(a =>
                    {
                        if (a is ConstantExpression ce) 
                            return ce.Value;
                        
                        // we try to compile expression arg to evaluate
                        try 
                        { 
                            return Expression.Lambda(a).Compile().DynamicInvoke(); 
                        } 
                        catch 
                        { 
                            return null; 
                        }
                    }).ToArray();

                    nodes.Insert(0, new MethodCallNode(mi, args));
                    expr = mc.Object ?? mc.Arguments.FirstOrDefault();
                    
                    continue;
                }

                if (expr is UnaryExpression ue)
                {
                    expr = ue.Operand;
                    continue;
                }

                if (expr is ParameterExpression) break;
                break;
            }
            return nodes;
        }
    }
}