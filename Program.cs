using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Workplace;

public static class Program
{
    public static void Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        var mediator = host.Services.GetRequiredService<IMediator>();
        mediator.Send(new GetCommand<int>());
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) =>
            {
                services.AddTransient<IMediator, Mediator>();
                services.AddTransient<IRequestHandler<GetCommand<int>, Unit>, GetCommandHandler<int>>();
            });
}
