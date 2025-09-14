namespace Workplace.Aop.Contracts;

public interface IAopBehavior<T> : IAopBehavior where T : AopAttibute { }

public interface IAopBehavior
{
    public object? InvokeWrapped(MethodInvocationDetails invocationDetails);
}
