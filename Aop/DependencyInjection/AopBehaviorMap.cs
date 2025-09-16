using Microsoft.Extensions.DependencyInjection;
using Workplace.Aop.Contracts;

namespace Workplace.Aop.DependencyInjection;

public class AopBehaviorMap
{
    private readonly Dictionary<Type, HashSet<Type>> behaviorMappings = [];

    public void BuildAttributeMapping(IServiceCollection services)
    {
        var behaviorTypes = services
            .Select(x => x.ServiceType)
            .Where(x => !x.IsAbstract && typeof(IAopBehavior).IsAssignableFrom(x))
            .ToHashSet();
        foreach (var behaviorType in behaviorTypes)
        {
            var attributeTypes = behaviorType.GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IAopBehavior<>))
                .Distinct()
                .Select(x => x.GetGenericArguments()[0]);
            foreach (var attributeType in attributeTypes)
            {
                if (!behaviorMappings.ContainsKey(attributeType))
                    behaviorMappings.Add(attributeType, []);
                behaviorMappings[attributeType].Add(behaviorType);
            }
        }
    }

    public HashSet<Type> GetRegisteredAttributeTypes(Type implementationType)
    {
        var serviceAopAttributeTypes = implementationType.GetMethods()
            .SelectMany(MehtodInvocationExtenstions.GetAopAttributeTypes)
            .ToHashSet();

        return serviceAopAttributeTypes
            .Where(behaviorMappings.ContainsKey)
            .ToHashSet();
    }

    public Dictionary<Type, HashSet<IAopBehavior>> GetBehaviorMap(HashSet<Type> aopAttributeTypes, IServiceProvider serviceProvider)
    {
        Dictionary<Type, IAopBehavior> behaviors = [];
        Dictionary<Type, HashSet<IAopBehavior>> result = [];
        foreach (var aopAttributeType in aopAttributeTypes)
        {
            result[aopAttributeType] = [];
            var behaviorTypes = behaviorMappings.GetValueOrDefault(aopAttributeType, []);
            foreach (var behaviorType in behaviorTypes)
            {
                if (!behaviors.TryGetValue(behaviorType, out var behavior))
                {
                    behavior = (IAopBehavior)serviceProvider.GetRequiredService(behaviorType);
                    behaviors.Add(behaviorType, behavior);
                }
                result[aopAttributeType].Add(behavior);
            }
        }
        return result;
    }
}
