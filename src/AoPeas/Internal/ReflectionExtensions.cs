using System.Reflection;

namespace AoPeas.Internal;

public static class ReflectionExtensions
{
    public static IEnumerable<Type> GetDecoratorTypes(this Type classType)
    {
        return classType.CustomAttributes
            .Select(x => x.AttributeType)
            .Where(x => typeof(DecoratorAttribute).IsAssignableFrom(x));
    }
    public static IEnumerable<Type> GetDecoratorTypes(this MethodInfo methodInfo)
    {
        return methodInfo.CustomAttributes
            .Select(x => x.AttributeType)
            .Where(x => typeof(DecoratorAttribute).IsAssignableFrom(x));
    }
}