using Microsoft.Extensions.DependencyInjection;
using Workplace.Aop.Contracts;

namespace Workplace.Aop.DependencyInjection;

/// <summary>
/// Class that stores aspects (the combination of advices and pointcuts)
/// </summary>
public class AspectTypesMap(Dictionary<Type, HashSet<Type>> aspectMap)
{
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
            .Where(aspectMap.ContainsKey)
            .ToHashSet();
    }

    /// <summary>
    /// Gets the aspect map for the specified pointcut types
    /// </summary>
    /// <param name="pointcutTypes">type of the pointcuts</param>
    /// <param name="serviceProvider">Store for advice registration</param>
    /// <returns></returns>
    public AspectMap GetAspectMap(HashSet<Type> pointcutTypes, IServiceProvider serviceProvider)
    {
        Dictionary<Type, IAdvice> advices = [];
        Dictionary<Type, HashSet<IAdvice>> result = [];
        foreach (var pointcutType in pointcutTypes)
        {
            result[pointcutType] = [];
            var adviceTypes = aspectMap.GetValueOrDefault(pointcutType, []);
            foreach (var adviceType in adviceTypes)
            {
                if (!advices.TryGetValue(adviceType, out var advice))
                {
                    advice = (IAdvice)serviceProvider.GetRequiredService(adviceType);
                    advices.Add(adviceType, advice);
                }
                result[pointcutType].Add(advice);
            }
        }
        return new AspectMap(result);
    }
}
