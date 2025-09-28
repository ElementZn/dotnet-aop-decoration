using AoPeas.DependencyInjection;
using AoPeas.Internal;
using AoPeas.Tests.Fixtures;
using AoPeas.Tests.Fixtures.Sut;
using Microsoft.Extensions.DependencyInjection;

namespace AoPeas.Tests.Tests;

public class AopRegistrationTests
{
    private readonly ServiceCollection servicesSut;

    public AopRegistrationTests()
    {
        servicesSut = new ServiceCollection();
    }

    [Fact]
    public void GivenNoBehaviors_DoesNotAlterRegistration()
    {
        servicesSut.AddScoped<ITestService, TestService>();

        servicesSut.AddAop();

        var sp = servicesSut.BuildServiceProvider();
        var testService = sp.GetRequiredService<ITestService>();
        Assert.IsType<TestService>(testService, exactMatch: true);
    }

    [Fact]
    public void GivenBehaviors_RegistersAopProxy()
    {
        servicesSut.AddScoped<ITestService, TestService>();
        servicesSut.AddScoped<LogCalls>();
        servicesSut.AddScoped<CountCalls>();
        servicesSut.AddScoped<PassthroughDecorator>();

        servicesSut.AddAop();

        var sp = servicesSut.BuildServiceProvider();
        var testService = sp.GetRequiredService<ITestService>();
        Assert.IsType<AopProxy>(testService, exactMatch: false);
    }
}
