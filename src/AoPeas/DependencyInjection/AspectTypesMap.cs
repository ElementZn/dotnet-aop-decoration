using AoPeas.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace AoPeas.DependencyInjection;

/// <summary>
/// Stores the association between behavior types and decorator types
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
        var behaviorTypes = services
            .Select(x => x.ServiceType)
            .Where(x => !x.IsAbstract && typeof(IBehavior).IsAssignableFrom(x))
            .Distinct();

        Dictionary<Type, HashSet<Type>> aspectMap = [];
        foreach (var behaviorType in behaviorTypes)
        {
            var attributeTypes = behaviorType.GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IBehavior<>))
                .Distinct()
                .Select(x => x.GetGenericArguments()[0]);
            foreach (var attributeType in attributeTypes)
            {
                if (!aspectMap.ContainsKey(attributeType))
                    aspectMap.Add(attributeType, []);
                aspectMap[attributeType].Add(behaviorType);
            }
        }
        return new AspectTypesMap(aspectMap);
    }

    /// <summary>
    /// Gets the decorator types that have at least a behavior attached
    /// </summary>
    /// <param name="implementationType">Type of the service implementation</param>
    /// <returns></returns>
    public HashSet<Type> GetDecoratorTypesWithBehavior(Type implementationType)
    {
        var decoratorTypes = implementationType.GetMethods()
            .SelectMany(ReflectionExtensions.GetDecoratorTypes)
            .Union(implementationType.GetDecoratorTypes())
            .ToHashSet();

        return decoratorTypes
            .Where(aspectTypesMap.ContainsKey)
            .ToHashSet();
    }

    /// <summary>
    /// Get behaviors types based on the decorator type
    /// </summary>
    /// <param name="decoratorType">Type of the decorator attribute</param>
    /// <returns></returns>
    public HashSet<Type> GetBehaviorTypes(Type decoratorType) => aspectTypesMap.GetValueOrDefault(decoratorType, []);
}
