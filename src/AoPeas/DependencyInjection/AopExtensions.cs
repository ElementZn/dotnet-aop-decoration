using Microsoft.Extensions.DependencyInjection;
using AoPeas.Internal;

namespace AoPeas.DependencyInjection;

public static class AopExtensions
{
    /// <summary>
    /// Registers the AoP behavior in the app. 
    /// Must be called after all advices and advised objects have been registered to DI
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

            var pointcutTypes = aspectTypesMap.GetAdvisedPointcutTypes(registration.ImplementationType);
            if (pointcutTypes.Count == 0) continue;

            services.Remove(registration);
            services.Add(new ServiceDescriptor(registration.ImplementationType, registration.ImplementationType, registration.Lifetime));
            services.Add(new ServiceDescriptor(registration.ServiceType, sp =>
            {
                var target = sp.GetRequiredService(registration.ImplementationType);
                var aspectMap = sp.GetAspectMap(aspectTypesMap, pointcutTypes);
                return CreateAopProxy(registration.ServiceType, target, aspectMap);
            }, registration.Lifetime));
        }
        return services;
    }

    private static AspectMap GetAspectMap(this IServiceProvider serviceProvider, AspectTypesMap aspectTypesMap, HashSet<Type> pointcutTypes)
    {
        Dictionary<Type, IAdvice> advicesCache = [];
        Dictionary<Type, HashSet<IAdvice>> result = [];
        foreach (var pointcutType in pointcutTypes)
        {
            result[pointcutType] = [];
            var adviceTypes = aspectTypesMap.GetAdviceTypes(pointcutType);
            foreach (var adviceType in adviceTypes)
            {
                if (!advicesCache.TryGetValue(adviceType, out var advice))
                {
                    advice = (IAdvice)serviceProvider.GetRequiredService(adviceType);
                    advicesCache.Add(adviceType, advice);
                }
                result[pointcutType].Add(advice);
            }
        }
        return new AspectMap(result);
    }

    private static object CreateAopProxy(Type serviceType, object target, AspectMap aspectMap)
    {
        var proxyType = typeof(AopProxy<>).MakeGenericType(serviceType);
        var factoryMethod = proxyType.GetMethod(nameof(AopProxy<object>.Create))!;
        return factoryMethod.Invoke(null, [target, aspectMap])
            ?? throw new InvalidOperationException($"Could not instantiate AoP proxy for type {serviceType}");
    }
}
