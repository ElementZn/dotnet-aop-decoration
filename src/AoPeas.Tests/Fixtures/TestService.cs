using AoPeas.Tests.Fixtures.Sut;

namespace AoPeas.Tests.Fixtures;

public interface ITestService
{
    public int GetIncrement(int a);
    public int GetSum(int a, int b);
    public int GetSum(int a, int b, int c);
}

[CountCallsDecorator]
public class TestService : ITestService
{
    [PassthroughDecorator]
    public int GetIncrement(int a) => a + 1;
    [NoImplementedDecorator]
    public int GetSum(int a, int b) => a + b;
    [LogCallsDecorator]
    public int GetSum(int a, int b, int c) => a + b + c;
}
