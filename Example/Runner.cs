using Microsoft.Extensions.Logging;

namespace Workplace.Example;

public class Runner(IMainService mainService, ILogger<Runner> logger)
{
    public void Run()
    {
        logger.LogInformation("Start RUN");

        mainService.GetIncrement(13);
        mainService.GetSum(5, 7);
        mainService.GetSum(2, 4, 6);

        logger.LogInformation("End RUN");
    }
}
