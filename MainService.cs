
namespace Workplace;

public interface IMainService
{
    public int GetIncrement(int a);
    public int GetSum(int a, int b);
    public int GetSum(int a, int b, int c);
}

public class MainService : IMainService
{
    public int GetIncrement(int a)
    {
        return a + 1;
    }

    public int GetSum(int a, int b)
    {
        return a + b;
    }
    
    [EnableProxyLogging]
    public int GetSum(int a, int b, int c)
    {
        return a + b + c;
    }
}
