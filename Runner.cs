using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Workplace;

public class Runner(IServiceScopeFactory scopeFactory)
    : BackgroundService
{

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();

        var mainService = scope.ServiceProvider.GetRequiredService<IMainService>();

        Console.WriteLine("Start");

        mainService.GetSum(2, 4);
        mainService.GetIncrement(13);

        Console.WriteLine("End");

        return Task.CompletedTask;
    }
}