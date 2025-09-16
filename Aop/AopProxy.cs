using System.Reflection;
using Workplace.Aop.Contracts;

namespace Workplace.Aop;

public class AopProxy<T> : DispatchProxy where T : class
{
    private T target = null!; // initialized in factory method
    private Dictionary<Type, HashSet<IAopBehavior>> behaviorMap = [];

    public static T Create(T target, Dictionary<Type, HashSet<IAopBehavior>> behaviorMap)
    {
        var decorated = Create<T, AopProxy<T>>();
        if (decorated is not AopProxy<T> proxy)
            throw new InvalidOperationException("Can't create proxy type");

        ArgumentNullException.ThrowIfNull(target);
        proxy.target = target;

        if (behaviorMap.Count == 0)
            throw new ArgumentException("No registered behaviors", nameof(behaviorMap));
        proxy.behaviorMap = behaviorMap;

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

        var aopAttributeTypes = implementedTargetMethod.GetAopAttributeTypes().Reverse();
        foreach (var aopAttributeType in aopAttributeTypes)
        {
            var behaviors = behaviorMap.GetValueOrDefault(aopAttributeType, []);
            foreach (var behavior in behaviors)
            {
                var previousInvocationDetails = invocationDetails;
                invocationDetails = invocationDetails with { Next = () => behavior.InvokeWrapped(previousInvocationDetails) };
            }
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
