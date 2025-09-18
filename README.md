# Usage

Assuming you have an interface and a service registered via DI:

```csharp
public interface IMainService
{
    public int GetSum(int a, int b);
}

public interface MainService : IMainService
{
    public int GetSum(int a, int b) => a + b;
}

// In service registration (typically Program.cs or Startup.cs):
services.AddScoped<IMainService, MainService>();
```

You can define custom attributes with attached behavior as follows:

```csharp
// Pointcut (attribute that marks where to inject custom behavior)
public class EnableProxyLoggingAttribute : PointcutAttribute { }

// Advice (functionality to inject)
public class LoggingAdvice(ILogger<LoggingAdvice> logger) : IAdvice<EnableProxyLoggingAttribute>
{
    public object? Apply(MethodInvocationDetails invocationDetails)
    {
        logger.LogInformation("Start method '{MethodInfo}', arguments: {Arguments}", invocationDetails.Name, string.Join(',', invocationDetails.Args));

        var result = invocationDetails.Next();

        logger.LogInformation("End method '{MethodInfo}', result: {Result}", invocationDetails.Name, result);

        return result;
    }
}
```

Add AoP registration to DI:

```csharp
services.AddScoped<IMainService, MainService>();
services.AddScoped<LoggingAdvice>();
services.AddAop();
```

Then you can attach the defined behavior to a specific method:

```csharp
public interface MainService : IMainService
{
    [EnableProxyLogging] //attach behaviour when this method is invoked
    public int GetSum(int a, int b) => a + b;
}
```

Example Output:
```shell
Example.LoggingAdvice: Information: Start method 'GetSum', arguments: 5,7
Example.LoggingAdvice: Information: End method 'GetSum', result: 12
```