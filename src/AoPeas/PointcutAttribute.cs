namespace AoPeas;

/// <summary>
/// Attribute for the pointcut, which specifies when an advice should be applied
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
public abstract class PointcutAttribute : Attribute { }
