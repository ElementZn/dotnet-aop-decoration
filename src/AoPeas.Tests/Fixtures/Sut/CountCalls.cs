namespace AoPeas.Tests.Fixtures.Sut;

public class CountCallsDecoratorAttribute : DecoratorAttribute { }

public class CountCallsBehavior : IBehavior<CountCallsDecoratorAttribute>
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
