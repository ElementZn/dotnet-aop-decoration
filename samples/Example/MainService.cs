using Microsoft.Extensions.Logging;
using AoPeas;

namespace Example;

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
    [NotImplementedPointcut]
    public int GetSum(int a, int b) => a + b;
    [EnableSecondProxyLogging]
    [EnableProxyLogging]
    public int GetSum(int a, int b, int c) => a + b + c;
}

public class NotImplementedPointcutAttribute : PointcutAttribute { }

public class EnableProxyLoggingAttribute : PointcutAttribute { }

public class LoggingAdvice(ILogger<LoggingAdvice> logger) : IAdvice<EnableProxyLoggingAttribute>
{
    private int count = 0;

    public object? Apply(MethodInvocationDetails invocationDetails)
    {
        count++;
        logger.LogInformation("Start method '{MethodInfo}', arguments: {Arguments}, count: {count}", invocationDetails.Name, string.Join(',', invocationDetails.Args), count);

        var result = invocationDetails.Next();

        logger.LogInformation("End method '{MethodInfo}', result: {Result}", invocationDetails.Name, result);

        return result;
    }
}

public class EnableSecondProxyLoggingAttribute : PointcutAttribute { }

public class SecondLoggingAdvice(ILogger<SecondLoggingAdvice> logger) : IAdvice<EnableSecondProxyLoggingAttribute>
{
    public object? Apply(MethodInvocationDetails invocationDetails)
    {
        logger.LogInformation("Start method '{MethodInfo}', arguments: {Arguments}", invocationDetails.Name, string.Join(',', invocationDetails.Args));

        var result = invocationDetails.Next();

        logger.LogInformation("End method '{MethodInfo}', result: {Result}", invocationDetails.Name, result);

        return result;
    }
}