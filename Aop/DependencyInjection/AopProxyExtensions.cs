using Microsoft.Extensions.DependencyInjection;
using Workplace.Aop.Contracts;

namespace Workplace.Aop.DependencyInjection;

public static class AopProxyExtensions
{
    public static IServiceCollection AddAopDecoration(this IServiceCollection services)
    {
        var aopBehaviorMap = new AopBehaviorMap();
        aopBehaviorMap.BuildAttributeMapping(services);

        var registrations = services.ToList();
        foreach (var registration in registrations)
        {
            if (registration.ImplementationType == null || registration.ServiceType == registration.ImplementationType) continue;

            var aopTypesWithBehavior = aopBehaviorMap.GetRegisteredAttributeTypes(registration.ImplementationType);
            if (aopTypesWithBehavior.Count == 0) continue;

            services.Remove(registration);
            services.Add(new ServiceDescriptor(registration.ImplementationType, registration.ImplementationType, registration.Lifetime));
            services.Add(new ServiceDescriptor(registration.ServiceType, sp =>
            {
                var target = sp.GetRequiredService(registration.ImplementationType);
                var serviceBehaviors = aopBehaviorMap.GetBehaviorMap(aopTypesWithBehavior, sp);
                return CreateAopProxy(registration.ServiceType, target, serviceBehaviors);
            }, registration.Lifetime));
        }
        return services;
    }

    private static object CreateAopProxy(Type serviceType, object target, Dictionary<Type, HashSet<IAopBehavior>> behaviors)
    {
        var proxyType = typeof(AopProxy<>).MakeGenericType(serviceType);
        var factoryMethod = proxyType.GetMethod(nameof(AopProxy<object>.Create))!;
        return factoryMethod.Invoke(null, [target, behaviors])
            ?? throw new InvalidOperationException($"Could not instantiate AoP proxy for type {serviceType}");
    }
}
