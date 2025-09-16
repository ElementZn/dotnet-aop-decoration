using System.Data;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Workplace.Aop.Contracts;

namespace Workplace.Aop;

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

            var serviceBehaviorTypes = aopBehaviorMap.GetBehaviorTypes(registration.ImplementationType);
            if (serviceBehaviorTypes.Count == 0) continue;

            services.Remove(registration);
            services.Add(new ServiceDescriptor(registration.ImplementationType, registration.ImplementationType, registration.Lifetime));
            services.Add(new ServiceDescriptor(registration.ServiceType, services =>
            {
                var target = services.GetRequiredService(registration.ImplementationType);
                var serviceBehaviors = serviceBehaviorTypes
                    .Select(x => services.GetService(x))
                    .OfType<IAopBehavior>()
                    .ToList();
                return CreateAopProxy(registration.ServiceType, target, serviceBehaviors);
            }, registration.Lifetime));
        }
        return services;
    }

    private static object CreateAopProxy(Type serviceType, object target, List<IAopBehavior> behaviors)
    {
        var proxyType = typeof(AopProxy<>).MakeGenericType(serviceType);
        var factoryMethod = proxyType.GetMethod(nameof(AopProxy<object>.Create))!;
        return factoryMethod.Invoke(null, [target, behaviors])
            ?? throw new InvalidOperationException($"Could not instantiate AoP proxy for type {serviceType}");

    }
}

public class AopBehaviorMap()
{
    private readonly Dictionary<Type, HashSet<Type>> behaviorMappings = [];

    internal void BuildAttributeMapping(IServiceCollection services)
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

    internal ICollection<Type> GetBehaviorTypes(Type implementationType)
    {
        var serviceAopAttributeTypes = implementationType.GetMethods()
            .SelectMany(MehtodInvocationExtenstions.GetAopAttributeTypes)
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
