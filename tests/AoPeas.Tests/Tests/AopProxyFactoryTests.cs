using AoPeas.Internal;
using AoPeas.Tests.Fixtures;

namespace AoPeas.Tests.Tests;

public class AopProxyFactoryTests
{
    [Fact]
    public void GivenEmptyAspectMap_WhenCreatingProxy_Throws()
    {
        var testService = new TestService();
        var aspectMap = new AspectMap([]);

        var exception = Record.Exception(() => AopProxy.Create(typeof(ITestService), testService, aspectMap));

        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void GivenValidParams_WhenCreatingProxy_ReturnsSuccess()
    {
        var testService = new TestService();
        var aspectMap = new AspectMap(new() { [typeof(NoAdviceAttribute)] = [new NoAdvice()] });

        var exception = Record.Exception(() => AopProxy.Create(typeof(ITestService), testService, aspectMap));

        Assert.Null(exception);
    }
}
