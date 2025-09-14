using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Workplace;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        var services = builder.Services;
        services.AddDecoratedScoped<IMainService, MainService>();
        services.AddScoped<Runner>();

        var host = builder.Build();

        var runner = host.Services.GetRequiredService<Runner>();

        runner.Run();
    }
}
