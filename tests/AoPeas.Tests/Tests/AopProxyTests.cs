using AoPeas.Internal;
using AoPeas.Tests.Fixtures;

namespace AoPeas.Tests.Tests;

public class AopProxyTests
{
    [Fact]
    public void GivenEmptyAspectMap_WhenCreatingProxy_Throws()
    {
        var testService = new TestService();
        var aspectMap = new AspectMap([]);

        var exception = Record.Exception(() => AopProxy<ITestService>.Create(testService, aspectMap));

        Assert.IsType<ArgumentException>(exception);
    }

}