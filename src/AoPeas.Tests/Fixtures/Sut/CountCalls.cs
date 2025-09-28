
namespace AoPeas.Tests.Fixtures.Sut;

public class CountCallsAttribute : DecoratorAttribute { }

public class CountCalls : IBehavior<CountCallsAttribute>
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
