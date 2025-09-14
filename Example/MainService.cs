

using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Workplace.Example;

public interface IMainService
{
    public int GetIncrement(int a);
    public int GetSum(int a, int b);
    public int GetSum(int a, int b, int c);
}

public class MainService : IMainService
{
    [EnableProxyLogging]
    [EnableSecondProxyLogging]
    public int GetIncrement(int a) => a + 1;
    // [EnableProxyLogging]
    public int GetSum(int a, int b) => a + b;
    [EnableProxyLogging]
    public int GetSum(int a, int b, int c) => a + b + c;
}

public class EnableProxyLoggingAttribute : AopAttibute { }

public class LoggingBehavior(ILogger<LoggingBehavior> logger) : IAopBehavior<EnableProxyLoggingAttribute>
{
    public object? InvokeWrapped(object targetObject, MethodInfo targetMethod, object?[]? args)
    {
        logger?.LogInformation("Start first method {MethodInfo}, arguments: {Arguments}", targetMethod.Name, string.Join(',', args ?? []));

        var result = targetMethod.Invoke(targetObject, args);

        logger?.LogInformation("End first method {MethodInfo}, result: {Result}", targetMethod.Name, result);

        return result;
    }
}


public class EnableSecondProxyLoggingAttribute : AopAttibute { }

public class SecondLoggingBehavior(ILogger<LoggingBehavior> logger) : IAopBehavior<EnableSecondProxyLoggingAttribute>
{
    public object? InvokeWrapped(object targetObject, MethodInfo targetMethod, object?[]? args)
    {
        logger?.LogInformation("Start second method {MethodInfo}, arguments: {Arguments}", targetMethod.Name, string.Join(',', args ?? []));

        var result = targetMethod.Invoke(targetObject, args);

        logger?.LogInformation("End second method {MethodInfo}, result: {Result}", targetMethod.Name, result);

        return result;
    }
}