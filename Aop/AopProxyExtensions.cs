using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Workplace.Aop.Contracts;

namespace Workplace.Aop;

public static class AopProxyExtensions
{
    public static IServiceCollection AddAopDecoration(this IServiceCollection services)
    {
        var aopAttributeTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(x => !x.IsAbstract && typeof(AopAttibute).IsAssignableFrom(x))
            .ToList();
        var aopBehaviorTypes = services
            .Select(x => x.ServiceType)
            .Where(x => !x.IsAbstract && typeof(IAopBehavior).IsAssignableFrom(x))
            .ToList();

        var registrations = services.ToList();
        foreach (var registration in registrations)
        {
            if (registration.ImplementationType == null) continue;

            var serviceAopAttributeTypes = registration.ImplementationType.GetMethods()
                .SelectMany(x => x.CustomAttributes)
                .Select(x => x.AttributeType)
                .Intersect(aopAttributeTypes)
                .Distinct()
                .ToList();
            if (!serviceAopAttributeTypes.Any()) continue;

            var serviceAopBehaviorInterfaces = serviceAopAttributeTypes.Select(x => typeof(IAopBehavior<>).MakeGenericType(x)).ToList();

            var serviceBehaviorTypes = aopBehaviorTypes
                .IntersectBy(serviceAopBehaviorInterfaces,
                    x => x.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAopBehavior<>)))
                .ToList();
            if (!serviceBehaviorTypes.Any()) continue;

            services.Remove(registration);
            services.Add(new ServiceDescriptor(registration.ImplementationType, registration.ImplementationType, registration.Lifetime));
            services.Add(new ServiceDescriptor(registration.ServiceType, services =>
            {
                var target = services.GetRequiredService(registration.ImplementationType);

                var proxyType = typeof(AopProxy<>).MakeGenericType(registration.ServiceType);

                var behaviors = serviceBehaviorTypes.Select(x => services.GetService(x) as IAopBehavior).ToList();

                var factoryMethod = proxyType.GetMethod(nameof(AopProxy<object>.Decorate));
                return factoryMethod?.Invoke(null, [target, behaviors])
                    ?? throw new InvalidOperationException($"Could not instantiate object for type {proxyType}");
            }, registration.Lifetime));
        }

        return services;
    }
}
