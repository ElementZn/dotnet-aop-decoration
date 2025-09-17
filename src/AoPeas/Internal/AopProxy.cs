using System.Reflection;

namespace AoPeas.Internal;

internal class AopProxy : DispatchProxy
{
    private object target = null!; // initialized in factory method
    private AspectMap aspectMap = null!; // initialized in factory method

    public static object Create(Type serviceType, object target, AspectMap aspectMap)
    {
        var decorated = Create(serviceType, typeof(AopProxy));
        if (decorated is not AopProxy proxy)
            throw new InvalidOperationException("Can't create proxy type");

        ArgumentNullException.ThrowIfNull(target);
        proxy.target = target;

        ArgumentNullException.ThrowIfNull(aspectMap);
        if (aspectMap.IsEmpty()) throw new ArgumentException(null, nameof(aspectMap));
        proxy.aspectMap = aspectMap;

        return decorated;
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(targetMethod);

        var implementedTargetMethod = GetImplementedMethod(targetMethod);

        var invocationDetails = new MethodInvocationDetails
        {
            Name = implementedTargetMethod.Name,
            Args = args ?? [],
            Target = target,
            Next = () => implementedTargetMethod.Invoke(target, args)
        };

        var pointcutTypes = implementedTargetMethod.GetPointcutTypes().Reverse();
        foreach (var pointcutType in pointcutTypes)
        {
            var advices = aspectMap.GetAdvices(pointcutType);
            foreach (var advice in advices)
            {
                var previousInvocationDetails = invocationDetails;
                invocationDetails = invocationDetails with { Next = () => advice.Apply(previousInvocationDetails) };
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
