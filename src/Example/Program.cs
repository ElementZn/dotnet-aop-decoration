using AoPeas.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Example;

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

        services.AddScoped<LoggingBehavior>();
        services.AddScoped<SecondLoggingBehavior>();

        services.AddAop();

        var host = builder.Build();

        var runner = host.Services.GetRequiredService<Runner>();

        runner.Run();
    }
}
