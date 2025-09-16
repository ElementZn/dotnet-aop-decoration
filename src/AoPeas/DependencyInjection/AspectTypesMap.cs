using Microsoft.Extensions.DependencyInjection;
using AoPeas.Internal;

namespace AoPeas.DependencyInjection;

/// <summary>
/// Stores the association between advice types and pointcut types
/// </summary>
public class AspectTypesMap(Dictionary<Type, HashSet<Type>> aspectTypesMap)
{
    /// <summary>
    /// Build aspect types map based on the registered services
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static AspectTypesMap Build(IServiceCollection services)
    {
        var adviceTypes = services
            .Select(x => x.ServiceType)
            .Where(x => !x.IsAbstract && typeof(IAdvice).IsAssignableFrom(x))
            .Distinct();

        Dictionary<Type, HashSet<Type>> aspectMap = [];
        foreach (var adviceType in adviceTypes)
        {
            var attributeTypes = adviceType.GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IAdvice<>))
                .Distinct()
                .Select(x => x.GetGenericArguments()[0]);
            foreach (var attributeType in attributeTypes)
            {
                if (!aspectMap.ContainsKey(attributeType))
                    aspectMap.Add(attributeType, []);
                aspectMap[attributeType].Add(adviceType);
            }
        }
        return new AspectTypesMap(aspectMap);
    }

    /// <summary>
    /// Gets the pointcut types that have at least an advice attached
    /// </summary>
    /// <param name="implementationType">Type of the service implementation</param>
    /// <returns></returns>
    public HashSet<Type> GetAdvisedPointcutTypes(Type implementationType)
    {
        var pointcutTypes = implementationType.GetMethods()
            .SelectMany(MethodInfoExtensions.GetPointcutTypes)
            .ToHashSet();

        return pointcutTypes
            .Where(aspectTypesMap.ContainsKey)
            .ToHashSet();
    }

    /// <summary>
    /// Get advices types based on the pointcut type
    /// </summary>
    /// <param name="pointcutType">Type of the pointcut attribute</param>
    /// <returns></returns>
    public HashSet<Type> GetAdviceTypes(Type pointcutType) => aspectTypesMap.GetValueOrDefault(pointcutType, []);
}
