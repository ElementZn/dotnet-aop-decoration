using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Workplace.Aop.DependencyInjection;
using Workplace.Example;

namespace Workplace;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Logging.ClearProviders();
        builder.Logging.AddDebug();

        var services = builder.Services;
        services.AddScoped<IMainService, MainService>();
        services.AddScoped<Runner>();

        services.AddScoped<LoggingAdvice>();
        services.AddScoped<SecondLoggingAdvice>();

        services.AddAop();

        var host = builder.Build();

        var runner = host.Services.GetRequiredService<Runner>();

        runner.Run();
    }
}
