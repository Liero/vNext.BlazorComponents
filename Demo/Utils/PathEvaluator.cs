using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace vNext.BlazorComponents.Demo.Utils
{
    public class PathEvaluator
    {
        public static PathResult GetProperty(object obj, string path)
        {
            var parts = path.Split(new[] { '.', '[' });
            var result = new PathResult();
            foreach (var pathPart in parts)
            {
                if (obj == null)
                {
                    return new PathResult();
                }
                if (pathPart.EndsWith(']'))
                {
                    result.Property = obj.GetType().GetProperty("Item");
                    if (result.Property != null)
                    {
                        object index = Convert.ChangeType(pathPart.TrimEnd(']'), result.Property.GetIndexParameters()[0].ParameterType);
                        result.Args = new[] { index };
                    }
                }
                else
                {
                    result.Args = null;
                    result.Property = obj.GetType().GetProperty(pathPart);
                }
                result.Obj = obj;
                obj = result.Property?.GetValue(obj, result.Args);
            }
            return result;
        }
    }

    public class PathResult
    {
        public object Obj { get; set; }
        public PropertyInfo Property { get; set; }
        public object[] Args { get; set; }

        bool Success => Obj != null && Property != null;

        public bool TrySetValue(object value)
        {
            if (!Success)
            {
                return false;
            }
            Property!.SetValue(Obj, value, Args);
            return true;
        }

        public object GetValue()
        {
            return Property?.GetValue(Obj, Args);
        }
    }
}
