namespace Workplace.Aop.Contracts;

/// <summary>
/// Interface for implementing an advice, which specifies the code to be run
/// </summary>
public interface IAdvice<TPointcut> : IAdvice where TPointcut : PointcutAttribute { }

public interface IAdvice
{
    public object? Apply(MethodInvocationDetails invocationDetails);
}
