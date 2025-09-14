using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Workplace;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        var services = builder.Services;
        services.AddScoped<IMainService, MainService>();
        services.AddScoped<Runner>();

        var host = builder.Build();

        var runner = host.Services.GetRequiredService<Runner>();

        runner.Run();
    }
}
