using System.Reflection;

public class LoggingProxy<T> : DispatchProxy where T: class
{
    public T Target {get; private set;}
    
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        Console.WriteLine("Start method {MethodInfo}, arguments: {Arguments}", targetMethod.Name, string.Join(',', args));
        var result = targetMethod?.Invoke(Target, args);
        Console.WriteLine("End method {MethodInfo}, result: {Result}", targetMethod.Name, result);
        return result;
    }

    public static T Decorate(T target) 
    {
        var decorated = Create<T, LoggingProxy<T>>();

        if (decorated is LoggingProxy<T> proxy)
        {
            proxy.Target = target;
        }

        return decorated as T;
    }
}