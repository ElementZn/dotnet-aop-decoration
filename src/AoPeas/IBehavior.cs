namespace AoPeas;

/// <summary>
/// Interface for implementing a behavior, which specifies the code to be run
/// </summary>
public interface IBehavior<TDecorator> : IBehavior where TDecorator : DecoratorAttribute { }

public interface IBehavior
{
    public object? Apply(MethodInvocationDetails invocationDetails);
}
