namespace Workplace;

public class Runner(IMainService mainService)
{

    public void Run()
    {
        Console.WriteLine("Start RUN");

        mainService.GetSum(2, 4);
        mainService.GetIncrement(13);

        Console.WriteLine("End RUN");
    }
}