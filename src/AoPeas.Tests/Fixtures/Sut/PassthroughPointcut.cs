namespace AoPeas.Tests.Fixtures.Sut;

public class PassthroughPointcutAttribute : PointcutAttribute { }

public class PassthroughPointcut : IAdvice<PassthroughPointcutAttribute>
{
    public object? Apply(MethodInvocationDetails invocationDetails) => invocationDetails.Next();
}
