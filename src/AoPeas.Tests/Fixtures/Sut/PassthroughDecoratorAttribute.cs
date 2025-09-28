namespace AoPeas.Tests.Fixtures.Sut;

public class PassthroughDecoratorAttribute : DecoratorAttribute { }

public class PassthroughDecorator : IBehavior<PassthroughDecoratorAttribute>
{
    public object? Apply(MethodInvocationDetails invocationDetails) => invocationDetails.Next();
}
