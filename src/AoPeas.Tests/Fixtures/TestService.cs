using AoPeas.Tests.Fixtures.Sut;

namespace AoPeas.Tests.Fixtures;

public interface ITestService
{
    public int GetIncrement(int a);
    public int GetSum(int a, int b);
    public int GetSum(int a, int b, int c);
}

[CountCalls]
public class TestService : ITestService
{
    [PassthroughPointcut]
    public int GetIncrement(int a) => a + 1;
    [NoImplementedPointcut]
    public int GetSum(int a, int b) => a + b;
    [LogCalls]
    public int GetSum(int a, int b, int c) => a + b + c;
}
