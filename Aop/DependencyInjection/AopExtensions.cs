using Microsoft.Extensions.DependencyInjection;
using Workplace.Aop.Contracts;

namespace Workplace.Aop.DependencyInjection;

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
                var aspectMap = aspectTypesMap.GetAspectMap(pointcutTypes, sp);
                return CreateAopProxy(registration.ServiceType, target, aspectMap);
            }, registration.Lifetime));
        }
        return services;
    }

    private static object CreateAopProxy(Type serviceType, object target, AspectMap aspectMap)
    {
        var proxyType = typeof(AopProxy<>).MakeGenericType(serviceType);
        var factoryMethod = proxyType.GetMethod(nameof(AopProxy<object>.Create))!;
        return factoryMethod.Invoke(null, [target, aspectMap])
            ?? throw new InvalidOperationException($"Could not instantiate AoP proxy for type {serviceType}");
    }
}
