using Workplace.Aop.Contracts;

namespace Workplace.Aop;

/// <summary>
/// Aspect map for advice retrieval based on pointcut attribute type
/// </summary>
/// <param name="aspectMap"></param>
public class AspectMap(Dictionary<Type, HashSet<IAdvice>> aspectMap)
{
    /// <summary>
    /// Get advices based on pointcut type
    /// </summary>
    /// <param name="pointcutType"></param>
    /// <returns></returns>
    public HashSet<IAdvice> GetAdvices(Type pointcutType) => aspectMap.GetValueOrDefault(pointcutType, []);
}
