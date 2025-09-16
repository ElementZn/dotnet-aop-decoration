using System.Reflection;
using Workplace.Aop.Contracts;

namespace Workplace.Aop;

public static class MehtodInvocationExtenstions
{
    public static IEnumerable<Type> GetAopAttributeTypes(this MethodInfo methodInfo)
    {
        return methodInfo.CustomAttributes
            .Select(x => x.AttributeType)
            .Where(x => typeof(AopAttibute).IsAssignableFrom(x));
    }
}