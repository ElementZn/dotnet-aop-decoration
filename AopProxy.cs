using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Workplace;

public class AopProxy<T> : DispatchProxy where T : class
{
    public T? Target { get; private set; }
    public IEnumerable<IAopBehavior> Behaviors { get; private set; } = [];

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        if (targetMethod == null || Target == null || !Behaviors.Any())
            throw new ArgumentNullException("Arguments could not be fulfilled"); // TODO: Add more verbose error handling

        var implementedTargetMethod = GetImplementedMethod(targetMethod, Target);
        if (implementedTargetMethod == null) return null;

        object?[]? chainArgs = [..args];
        MethodInfo chainMethodInfo = implementedTargetMethod;
        object chainTarget = Target;
        Func<object?> chainMethodCall = () => chainMethodInfo.Invoke(chainTarget, chainArgs);
        foreach (var behavior in Behaviors)
        {
            var behaviorInterface = behavior.GetType().GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAopBehavior<>));
            var aopAttributeType = behaviorInterface.GetGenericArguments()[0];
            if(!implementedTargetMethod.CustomAttributes.Any(x=> x.AttributeType == aopAttributeType))
                continue;

            var previousArgs = chainArgs;
            chainArgs = [chainTarget, chainMethodInfo, previousArgs];
            chainTarget = behavior;
            chainMethodInfo = behavior.GetType().GetMethod(nameof(IAopBehavior.InvokeWrapped));
            chainMethodCall = () => chainMethodInfo.Invoke(chainTarget, chainArgs);
        }
        var result = chainMethodCall();

        return result;
    }

    private static MethodInfo GetImplementedMethod(MethodInfo interfaceMethod, T target)
    {
        var targetInterface = interfaceMethod.DeclaringType!;
        var interfaceMap = target.GetType().GetInterfaceMap(targetInterface);
        for (int i = 0; i < interfaceMap.InterfaceMethods.Length; i++)
        {
            if (interfaceMap.InterfaceMethods[i] == interfaceMethod)
                return interfaceMap.TargetMethods[i];
        }
        throw new InvalidOperationException($"No implementation for the specific method '{interfaceMethod.Name}'");
    }

    public static T Decorate(T target, IEnumerable<IAopBehavior> behaviors)
    {
        var decorated = Create<T, AopProxy<T>>();

        if (decorated is AopProxy<T> proxy)
        {
            proxy.Target = target;
            proxy.Behaviors = behaviors;
        }

        return decorated;
    }
}

public abstract class AopAttibute : Attribute { }

public interface IAopBehavior<T> : IAopBehavior where T : AopAttibute { }

public interface IAopBehavior
{
    public object? InvokeWrapped(object targetObject, MethodInfo targetMethod, object?[]? args);
}

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
