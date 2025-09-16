using System.Reflection;

namespace AoPeas.Internal;

public static class MethodInfoExtensions
{
    public static IEnumerable<Type> GetPointcutTypes(this MethodInfo methodInfo)
    {
        return methodInfo.CustomAttributes
            .Select(x => x.AttributeType)
            .Where(x => typeof(PointcutAttribute).IsAssignableFrom(x));
    }
}