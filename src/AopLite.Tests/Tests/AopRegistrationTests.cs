using AopLite.DependencyInjection;
using AopLite.Internal;
using AopLite.Tests.Fixtures;
using AopLite.Tests.Fixtures.Sut;
using Microsoft.Extensions.DependencyInjection;

namespace AopLite.Tests.Tests;

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
        servicesSut.AddScoped<LogCallsBehavior>();
        servicesSut.AddScoped<CountCallsBehavior>();
        servicesSut.AddScoped<PassthroughBehavior>();

        servicesSut.AddAop();

        var sp = servicesSut.BuildServiceProvider();
        var testService = sp.GetRequiredService<ITestService>();
        Assert.IsType<AopProxy>(testService, exactMatch: false);
    }
}
