using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Workplace;

public class LoggingProxy<T> : DispatchProxy where T : class
{
    public T? Target { get; private set; }
    public ILogger<LoggingProxy<T>>? Logger { get; private set; }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        if (targetMethod == null || Target == null) return null;

        var implementedTargetMethod = GetImplementedMethod(targetMethod, Target);
        if (implementedTargetMethod == null) return null;

        if (HasProxyLoggingEnabled(implementedTargetMethod))
            Logger?.LogInformation("Start method {MethodInfo}, arguments: {Arguments}", targetMethod.Name, string.Join(',', args ?? []));

        var result = targetMethod.Invoke(Target, args);

        if (HasProxyLoggingEnabled(implementedTargetMethod))
            Logger?.LogInformation("End method {MethodInfo}, result: {Result}", targetMethod.Name, result);

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

    private static bool HasProxyLoggingEnabled(MethodInfo targetMethod)
    {
        return targetMethod.CustomAttributes
            .Any(attribute => attribute.AttributeType == typeof(EnableProxyLoggingAttribute));
    }

    public static T Decorate(T target, ILogger<LoggingProxy<T>> logger)
    {
        var decorated = Create<T, LoggingProxy<T>>();

        if (decorated is LoggingProxy<T> proxy)
        {
            proxy.Target = target;
            proxy.Logger = logger;
        }

        return decorated;
    }
}

public abstract class AopAttibute : Attribute { }

public static class LoggingProxyExtensions
{
    public static IServiceCollection AddDecoratedScoped<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
    {
        services.AddScoped<TImplementation>();
        services.AddScoped(services =>
        {
            var target = services.GetRequiredService<TImplementation>();
            var logger = services.GetRequiredService<ILogger<LoggingProxy<TService>>>();
            return LoggingProxy<TService>.Decorate(target, logger);
        });
        return services;
    }

    public static IServiceCollection AddLoggingDecoration(this IServiceCollection services)
    {
        var aopAttributeTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(x => !x.IsAbstract && typeof(AopAttibute).IsAssignableFrom(x))
            .ToList();

        var loggableRegistrations = services
            .Where(registration =>
                registration.ImplementationType != null &&
                registration.ImplementationType.GetMethods()
                    .Any(methodInfo => methodInfo.CustomAttributes
                        .IntersectBy(aopAttributeTypes, attribute => attribute.AttributeType)
                        .Any()))
            .ToList();

        foreach (var registration in loggableRegistrations)
        {
            if (registration.ImplementationType == null) continue;
            services.Remove(registration);
            services.Add(new ServiceDescriptor(registration.ImplementationType, registration.ImplementationType, registration.Lifetime));
            services.Add(new ServiceDescriptor(registration.ServiceType, (services) =>
            {
                var target = services.GetRequiredService(registration.ImplementationType);

                var proxyType = typeof(LoggingProxy<>).MakeGenericType(registration.ServiceType);

                var loggerType = typeof(ILogger<>).MakeGenericType(proxyType);
                var logger = services.GetRequiredService(loggerType);

                var factoryMethod = proxyType.GetMethod("Decorate");
                return factoryMethod?.Invoke(null, [target, logger])
                    ?? throw new InvalidOperationException($"Could not instantiate object for type {proxyType}");
            }, registration.Lifetime));
        }

        return services;
    }
}
