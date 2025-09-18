using AoPeas.Internal;
using AoPeas.Tests.Fixtures;

namespace AoPeas.Tests.Tests;

public class AopProxyTests
{
    [Fact]
    public void GivenAdvice_WhenCallingMethod_ReturnsCorrectResult()
    {
        var testService = new TestService();
        var aspectMap = new AspectMap(new() { [typeof(NoAdviceAttribute)] = [new NoAdvice()] });

        var proxy = (ITestService)AopProxy.Create(typeof(ITestService), testService, aspectMap);

        Assert.Equal(4, proxy.GetIncrement(3));
    }

    [Fact]
    public void GivenAdvice_WhenAdjustingResult_ReturnsModifiedResult()
    {
        var testService = new TestService();
        var aspectMap = new AspectMap(new() { [typeof(AddTenAttribute)] = [new AddTen()] });

        var proxy = (ITestService)AopProxy.Create(typeof(ITestService), testService, aspectMap);

        Assert.Equal(14, proxy.GetIncrement(3));
    }
}