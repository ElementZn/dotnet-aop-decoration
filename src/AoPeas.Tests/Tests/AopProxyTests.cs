using AoPeas.Internal;
using AoPeas.Tests.Fixtures;
using AoPeas.Tests.Fixtures.Sut;

namespace AoPeas.Tests.Tests;

public class AopProxyTests
{
    private readonly LogCalls logCallsPointcut;
    private readonly CountCalls countCallsPointcut;
    private readonly ITestService proxySut;

    public AopProxyTests()
    {
        logCallsPointcut = new LogCalls();
        countCallsPointcut = new CountCalls();
        var aspectMap = new AspectMap(new()
        {
            [typeof(PassthroughPointcutAttribute)] = [new PassthroughPointcut()],
            [typeof(LogCallsAttribute)] = [logCallsPointcut],
            [typeof(CountCallsAttribute)] = [countCallsPointcut]
        });
        var testService = new TestService();
        proxySut = (ITestService)AopProxy.Create(typeof(ITestService), testService, aspectMap);
    }

    [Fact]
    public void WhenCallingMethod_ReturnsCorrectResult()
    {
        var param1 = 4;

        var result = proxySut.GetIncrement(param1);

        Assert.Equal(param1 + 1, result);
    }

    [Fact]
    public void WhenCallingOverloadedMethod_AppliesCorrespondingAdvice()
    {
        var param1 = 4;
        var param2 = 6;

        proxySut.GetSum(param1, param2);

        var result = logCallsPointcut.GetLogs();
        Assert.Empty(result);
    }

    [Fact]
    public void WhenCallingMethod_AppliesAdvice()
    {
        var param1 = 4;
        var param2 = 6;
        var param3 = 8;

        proxySut.GetSum(param1, param2, param3);

        var result = logCallsPointcut.GetLogs();
        Assert.Equal(2, result.Count);
        var startLog = result[0];
        Assert.Contains($"arguments: {param1},{param2},{param3}", startLog);
        var endLog = result[1];
        Assert.Contains($"result: {param1 + param2 + param3}", endLog);
    }
}
