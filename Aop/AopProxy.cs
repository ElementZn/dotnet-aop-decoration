using System.Reflection;
using Workplace.Aop.Contracts;

namespace Workplace.Aop;

public class AopProxy<T> : DispatchProxy where T : class
{
    private T? target;
    private IEnumerable<IAopBehavior> behaviors = [];

    public static T Create(T target, ICollection<IAopBehavior> behaviors)
    {
        ArgumentNullException.ThrowIfNull(target);
        if (behaviors.Count == 0)
            throw new ArgumentException("No registered behaviors", nameof(behaviors));

        var decorated = Create<T, AopProxy<T>>();
        if (decorated is not AopProxy<T> proxy)
            throw new InvalidOperationException("Can't create proxy type");

        proxy.target = target;
        proxy.behaviors = behaviors;

        return decorated;
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(targetMethod);
        if (target == null)
            throw new InvalidOperationException("Invalid target");
        if (!behaviors.Any())
            throw new InvalidOperationException("No registered behaviors");

        var implementedTargetMethod = GetImplementedMethod(targetMethod, target)
            ?? throw new ArgumentException("No corresponding implemented method", nameof(targetMethod));

        var invocationDetails = new MethodInvocationDetails
        {
            Name = implementedTargetMethod.Name,
            Args = args ?? [],
            Target = target,
            Next = () => implementedTargetMethod.Invoke(target, args)
        };

        foreach (var behavior in behaviors)
        {
            var behaviorInterface = behavior.GetType().GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAopBehavior<>));
            var aopAttributeType = behaviorInterface.GetGenericArguments()[0];
            if (!implementedTargetMethod.CustomAttributes.Any(x => x.AttributeType == aopAttributeType))
                continue;

            var previousInvocationDetails = invocationDetails;
            invocationDetails = invocationDetails with { Next = () => behavior.InvokeWrapped(previousInvocationDetails) };
        }
        var result = invocationDetails.Next();

        return result;
    }

    private static MethodInfo GetImplementedMethod(MethodInfo interfaceMethod, T target)
    {
        var targetInterface = interfaceMethod.DeclaringType!;
        var interfaceMap = target.GetType().GetInterfaceMap(targetInterface);
        for (int i = 0; i < interfaceMap.InterfaceMethods.Length; i++)
        {
            if (interfaceMap.InterfaceMethods[i] == interfaceMethod)
                return interfaceMap.TargetMethods[i];
        }
        throw new InvalidOperationException($"No implementation for the specific method '{interfaceMethod.Name}'");
    }
}
