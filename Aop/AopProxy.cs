using System.Reflection;
using Workplace.Aop.Contracts;

namespace Workplace.Aop;

public class AopProxy<T> : DispatchProxy where T : class
{
    private T? Target { get; set; }
    private IEnumerable<IAopBehavior> Behaviors { get; set; } = [];

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        if (targetMethod == null || Target == null || !Behaviors.Any())
            throw new ArgumentNullException("Arguments could not be fulfilled"); // TODO: Add more verbose error handling

        var implementedTargetMethod = GetImplementedMethod(targetMethod, Target);
        if (implementedTargetMethod == null) return null;

        var invocationDetails = new MethodInvocationDetails
        {
            Name = implementedTargetMethod.Name,
            Args = args ?? [],
            Target = Target,
            Next = () => implementedTargetMethod.Invoke(Target, args)
        };

        foreach (var behavior in Behaviors)
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

    public static T Decorate(T target, IEnumerable<IAopBehavior> behaviors)
    {
        var decorated = Create<T, AopProxy<T>>();

        if (decorated is AopProxy<T> proxy)
        {
            proxy.Target = target;
            proxy.Behaviors = behaviors;
        }

        return decorated;
    }
}

