using AoPeas.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace AoPeas.DependencyInjection;

public static class AopExtensions
{
    /// <summary>
    /// Registers the AoP behavior in the app. 
    /// Must be called after all behaviors and decorated objects have been registered to DI
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddAop(this IServiceCollection services)
    {
        var aspectTypesMap = AspectTypesMap.Build(services);

        var registrations = services.ToList();
        foreach (var registration in registrations)
        {
            if (registration.ImplementationType == null || registration.ServiceType == registration.ImplementationType) continue;

            var decoratorTypes = aspectTypesMap.GetDecoratorTypesWithBehavior(registration.ImplementationType);
            if (decoratorTypes.Count == 0) continue;

            services.Remove(registration);
            services.Add(new ServiceDescriptor(registration.ImplementationType, registration.ImplementationType, registration.Lifetime));
            services.Add(new ServiceDescriptor(registration.ServiceType, sp =>
            {
                var target = sp.GetRequiredService(registration.ImplementationType);
                var aspectMap = sp.GetAspectMap(aspectTypesMap, decoratorTypes);
                return AopProxy.Create(registration.ServiceType, target, aspectMap);
            }, registration.Lifetime));
        }
        return services;
    }

    private static AspectMap GetAspectMap(this IServiceProvider serviceProvider, AspectTypesMap aspectTypesMap, HashSet<Type> decoratorTypes)
    {
        Dictionary<Type, IBehavior> behaviorsCache = [];
        Dictionary<Type, HashSet<IBehavior>> result = [];
        foreach (var decoratorType in decoratorTypes)
        {
            result[decoratorType] = [];
            var behaviorTypes = aspectTypesMap.GetBehaviorTypes(decoratorType);
            foreach (var behaviorType in behaviorTypes)
            {
                if (!behaviorsCache.TryGetValue(behaviorType, out var behavior))
                {
                    behavior = (IBehavior)serviceProvider.GetRequiredService(behaviorType);
                    behaviorsCache.Add(behaviorType, behavior);
                }
                result[decoratorType].Add(behavior);
            }
        }
        return new AspectMap(result);
    }
}
