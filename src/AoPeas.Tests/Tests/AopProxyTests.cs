using AoPeas.Internal;
using AoPeas.Tests.Fixtures;
using AoPeas.Tests.Fixtures.Sut;

namespace AoPeas.Tests.Tests;

public class AopProxyTests
{
    [Fact]
    public void GivenAdvice_WhenCallingMethod_ReturnsCorrectResult()
    {
        var testService = new TestService();
        var aspectMap = new AspectMap(new() { [typeof(PassthroughPointcutAttribute)] = [new PassthroughPointcut()] });

        var proxy = (ITestService)AopProxy.Create(typeof(ITestService), testService, aspectMap);

        Assert.Equal(4, proxy.GetIncrement(3));
    }
}