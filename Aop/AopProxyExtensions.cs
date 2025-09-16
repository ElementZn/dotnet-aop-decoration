using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Workplace.Aop.Contracts;

namespace Workplace.Aop;

public static class AopProxyExtensions
{
    public static IServiceCollection AddAopDecoration(this IServiceCollection services)
    {
        var aopOptions = new AopProxyOptions(services);
        aopOptions.BuildAttributeMapping();

        var registrations = services.ToList();
        foreach (var registration in registrations)
        {
            if (registration.ImplementationType == null || registration.ServiceType == registration.ImplementationType) continue;

            var serviceBehaviorTypes = aopOptions.GetBehaviorTypes(registration.ImplementationType);
            if (serviceBehaviorTypes.Count == 0) continue;

            services.Remove(registration);
            services.Add(new ServiceDescriptor(registration.ImplementationType, registration.ImplementationType, registration.Lifetime));
            services.Add(new ServiceDescriptor(registration.ServiceType, services =>
            {
                var proxyType = typeof(AopProxy<>).MakeGenericType(registration.ServiceType);
                var factoryMethod = proxyType.GetMethod(nameof(AopProxy<object>.Decorate));

                var target = services.GetRequiredService(registration.ImplementationType);
                var serviceBehaviors = serviceBehaviorTypes
                    .Select(x => services.GetService(x) as IAopBehavior)
                    .ToList();
                return factoryMethod?.Invoke(null, [target, serviceBehaviors])
                    ?? throw new InvalidOperationException($"Could not instantiate AoP proxy for type {registration.ServiceType}");
            }, registration.Lifetime));
        }
        return services;
    }
}

public class AopProxyOptions(IServiceCollection services)
{
    private readonly Dictionary<Type, HashSet<Type>> behaviorMappings = [];

    internal void BuildAttributeMapping()
    {
        var behaviorTypes = services
            .Select(x => x.ServiceType)
            .Where(x => !x.IsAbstract && typeof(IAopBehavior).IsAssignableFrom(x))
            .ToHashSet();
        foreach (var behaviorType in behaviorTypes)
        {
            var attributeTypes = behaviorType.GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IAopBehavior<>)).Distinct()
                .Select(x => x.GetGenericArguments()[0]);
            foreach (var attributeType in attributeTypes)
            {
                if (!behaviorMappings.ContainsKey(attributeType))
                    behaviorMappings.Add(attributeType, []);
                behaviorMappings[attributeType].Add(behaviorType);
            }
        }
    }

    internal ICollection<Type> GetBehaviorTypes(Type implementationType)
    {
        var serviceAopAttributeTypes = implementationType.GetMethods()
            .SelectMany(x => x.CustomAttributes)
            .Select(x => x.AttributeType)
            .Where(x => typeof(AopAttibute).IsAssignableFrom(x))
            .ToHashSet();

        HashSet<Type> results = [];
        foreach (var attributeType in serviceAopAttributeTypes)
        {
            var values = behaviorMappings.GetValueOrDefault(attributeType, []);
            results.UnionWith(values);
        }
        return results;
    }
}
