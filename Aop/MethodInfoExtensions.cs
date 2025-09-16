using System.Reflection;
using Workplace.Aop.Contracts;

namespace Workplace.Aop;

public static class MethodInfoExtensions
{
    public static IEnumerable<Type> GetPointcutTypes(this MethodInfo methodInfo)
    {
        return methodInfo.CustomAttributes
            .Select(x => x.AttributeType)
            .Where(x => typeof(PointcutAttribute).IsAssignableFrom(x));
    }
}