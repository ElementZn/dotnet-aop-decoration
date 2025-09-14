
namespace Workplace;

public interface IMainService
{
    public int GetIncrement(int a);
    public int GetSum(int a, int b);
    public int GetSum(int a, int b, int c);
}

public class MainService : IMainService
{
    [EnableProxyLogging]
    public int GetIncrement(int a) => a + 1;
    [EnableProxyLogging]
    public int GetSum(int a, int b) => a + b;
    [EnableProxyLogging]
    public int GetSum(int a, int b, int c) => a + b + c;
}


[AttributeUsage(AttributeTargets.Method)]
public class EnableProxyLoggingAttribute : AopAttibute { }