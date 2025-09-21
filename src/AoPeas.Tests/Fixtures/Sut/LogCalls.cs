
namespace AoPeas.Tests.Fixtures.Sut;

public class LogCallsAttribute : PointcutAttribute { }

public class LogCalls() : IAdvice<LogCallsAttribute>
{
    private readonly List<string> logs = [];

    public object? Apply(MethodInvocationDetails invocationDetails)
    {
        logs.Add($"Start method '{invocationDetails.Name}', arguments: {string.Join(',', invocationDetails.Args)}");

        var result = invocationDetails.Next();

        logs.Add($"End method '{invocationDetails.Name}', result: {result}");

        return result;
    }

    public IReadOnlyList<string> GetLogs() => logs.AsReadOnly();
}