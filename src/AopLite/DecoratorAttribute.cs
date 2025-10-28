namespace AopLite;

/// <summary>
/// Attribute for the decorator, which specifies when a behavior should be applied
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
public abstract class DecoratorAttribute : Attribute { }
