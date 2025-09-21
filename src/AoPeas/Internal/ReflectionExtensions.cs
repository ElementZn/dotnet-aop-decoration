using System.Reflection;

namespace AoPeas.Internal;

public static class ReflectionExtensions
{
    public static IEnumerable<Type> GetPointcutTypes(this Type classType)
    {
        return classType.CustomAttributes
            .Select(x => x.AttributeType)
            .Where(x => typeof(PointcutAttribute).IsAssignableFrom(x));
    }
    public static IEnumerable<Type> GetPointcutTypes(this MethodInfo methodInfo)
    {
        return methodInfo.CustomAttributes
            .Select(x => x.AttributeType)
            .Where(x => typeof(PointcutAttribute).IsAssignableFrom(x));
    }
}