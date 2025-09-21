
using Microsoft.Extensions.Logging;

namespace AoPeas.Tests.Fixtures.Sut;

public class LogCallsAttribute : PointcutAttribute { }

public class LogCalls(ILogger<LogCalls> logger) : IAdvice<LogCallsAttribute>
{
    public object? Apply(MethodInvocationDetails invocationDetails)
    {
        logger.LogInformation("Start method '{MethodInfo}', arguments: {Arguments}", invocationDetails.Name, string.Join(',', invocationDetails.Args));

        var result = invocationDetails.Next();

        logger.LogInformation("End method '{MethodInfo}', result: {Result}", invocationDetails.Name, result);

        return result;
    }
}