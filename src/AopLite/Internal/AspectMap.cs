namespace AopLite.Internal;

/// <summary>
/// Aspect map for behavior retrieval based on decorator attribute type
/// </summary>
/// <param name="aspectMap"></param>
public class AspectMap(Dictionary<Type, HashSet<IBehavior>> aspectMap)
{
    /// <summary>
    /// Get behaviors based on decorator type
    /// </summary>
    /// <param name="decoratorType"></param>
    /// <returns></returns>
    public HashSet<IBehavior> GetBehaviors(Type decoratorType) => aspectMap.GetValueOrDefault(decoratorType, []);
    /// <summary>
    /// Check if the aspect map contains registered behaviors
    /// </summary>
    /// <returns></returns>
    public bool IsEmpty() => aspectMap.Values.Count == 0;
}
