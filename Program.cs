using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Workplace;

public static class Program
{
    public static void Main(string[] args)
    {
        var host = Host.CreateApplicationBuilder(args);

        var services = host.Services;
        services.AddHostedService<Runner>();
        services.AddScoped<IMainService, MainService>();

        host.Build().Run();
    }
}
