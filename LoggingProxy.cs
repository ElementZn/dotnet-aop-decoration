using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Workplace;

public class LoggingProxy<T> : DispatchProxy where T : class
{
    public T? Target { get; private set; }
    public ILogger<T>? Logger { get; private set; }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        if (targetMethod == null || Target == null) return null;

        var implementedTargetMethod = Target.GetType().GetMethod(targetMethod.Name);
        if (implementedTargetMethod == null) return null;

        if (HasProxyLoggingEnabled(implementedTargetMethod))
            Logger?.LogInformation("Start method {MethodInfo}, arguments: {Arguments}", targetMethod.Name, string.Join(',', args ?? []));

        var result = targetMethod.Invoke(Target, args);

        if (HasProxyLoggingEnabled(implementedTargetMethod))
            Logger?.LogInformation("End method {MethodInfo}, result: {Result}", targetMethod.Name, result);

        return result;
    }

    private static bool HasProxyLoggingEnabled(MethodInfo targetMethod)
    {
        return targetMethod.CustomAttributes
            .Any(attribute => attribute.AttributeType == typeof(EnableProxyLoggingAttribute));
    }

    public static T Decorate(T target, ILogger<T> logger)
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

[AttributeUsage(AttributeTargets.Method)]
public class EnableProxyLoggingAttribute : Attribute { }

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
            var logger = services.GetRequiredService<ILogger<TImplementation>>();
            return LoggingProxy<TService>.Decorate(target, logger);
        });
        return services;
    }
}
