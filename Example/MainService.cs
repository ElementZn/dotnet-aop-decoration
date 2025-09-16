using Microsoft.Extensions.Logging;
using Workplace.Aop.Contracts;

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
    public int GetIncrement(int a) => a + 1;
    public int GetSum(int a, int b) => a + b;
    [EnableSecondProxyLogging]
    [EnableProxyLogging]
    public int GetSum(int a, int b, int c) => a + b + c;
}

public class EnableProxyLoggingAttribute : AopAttibute { }

public class LoggingBehavior(ILogger<LoggingBehavior> logger) : IAopBehavior<EnableProxyLoggingAttribute>
{
    public object? InvokeWrapped(MethodInvocationDetails invocationDetails)
    {
        logger.LogInformation("Start method '{MethodInfo}', arguments: {Arguments}", invocationDetails.Name, string.Join(',', invocationDetails.Args));

        var result = invocationDetails.Next();

        logger.LogInformation("End method '{MethodInfo}', result: {Result}", invocationDetails.Name, result);

        return result;
    }
}


public class EnableSecondProxyLoggingAttribute : AopAttibute { }

public class SecondLoggingBehavior(ILogger<SecondLoggingBehavior> logger) : IAopBehavior<EnableSecondProxyLoggingAttribute>
{
    public object? InvokeWrapped(MethodInvocationDetails invocationDetails)
    {
        logger.LogInformation("Start method '{MethodInfo}', arguments: {Arguments}", invocationDetails.Name, string.Join(',', invocationDetails.Args));

        var result = invocationDetails.Next();

        logger.LogInformation("End method '{MethodInfo}', result: {Result}", invocationDetails.Name, result);

        return result;
    }
}