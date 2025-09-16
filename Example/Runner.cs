using Microsoft.Extensions.Logging;

namespace Workplace.Example;

public class Runner(IMainService mainService, IMainService mainService2, ILogger<Runner> logger)
{
    public void Run()
    {
        logger.LogInformation("Start RUN");

        mainService.GetIncrement(13);
        mainService2.GetIncrement(5);

        mainService.GetSum(5, 7);
        mainService.GetSum(2, 4, 6);

        logger.LogInformation("End RUN");
    }
}
