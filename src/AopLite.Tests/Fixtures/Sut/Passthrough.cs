namespace AopLite.Tests.Fixtures.Sut;

public class PassthroughDecoratorAttribute : DecoratorAttribute { }

public class PassthroughBehavior : IBehavior<PassthroughDecoratorAttribute>
{
    public object? Apply(MethodInvocationDetails invocationDetails) => invocationDetails.Next();
}
