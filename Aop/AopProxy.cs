using System.Reflection;
using Workplace.Aop.Contracts;

namespace Workplace.Aop;

public class AopProxy<T> : DispatchProxy where T : class
{
    private T target = null!; // initialized in factory method
    private IEnumerable<IAopBehavior> behaviors = [];

    public static T Create(T target, ICollection<IAopBehavior> behaviors)
    {
        var decorated = Create<T, AopProxy<T>>();
        if (decorated is not AopProxy<T> proxy)
            throw new InvalidOperationException("Can't create proxy type");

        ArgumentNullException.ThrowIfNull(target);
        proxy.target = target;

        if (behaviors.Count == 0)
            throw new ArgumentException("No registered behaviors", nameof(behaviors));
        proxy.behaviors = behaviors;

        return decorated;
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(targetMethod);

        var implementedTargetMethod = GetImplementedMethod(targetMethod)
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

    private MethodInfo GetImplementedMethod(MethodInfo interfaceMethod)
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
