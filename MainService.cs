
namespace Workplace;

public interface IMainService
{
    public int GetSum(int a, int b);
    public int GetIncrement(int a);
}

public class MainService : IMainService
{
    public int GetIncrement(int a)
    {
        return a + 1;
    }

    [EnableProxyLogging]
    public int GetSum(int a, int b)
    {
        return a + b;
    }
}
