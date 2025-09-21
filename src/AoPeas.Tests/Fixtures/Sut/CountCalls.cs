
namespace AoPeas.Tests.Fixtures.Sut;

public class CountCallsAttribute : PointcutAttribute { }

public class CountCalls : IAdvice<CountCallsAttribute>
{
    private int counter = 0;

    public object? Apply(MethodInvocationDetails invocationDetails)
    {
        var result = invocationDetails.Next();

        counter++;

        return result;
    }

    public int GetCounter() => counter;
}