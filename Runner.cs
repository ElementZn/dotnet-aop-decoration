using Microsoft.Extensions.Logging;

namespace Workplace;

public class Runner(IMainService mainService, ILogger<Runner> logger)
{

    public void Run()
    {
        logger.LogInformation("Start RUN");

        mainService.GetSum(2, 4);
        mainService.GetIncrement(13);

        logger.LogInformation("End RUN");
    }
}
