using Workplace.Aop.Contracts;

namespace Workplace.Aop;

public class AspectMap(Dictionary<Type, HashSet<IAdvice>> aspectMap)
{
    /// <summary>
    /// Get advices based on pointcut type
    /// </summary>
    /// <param name="pointcutType"></param>
    /// <returns></returns>
    public HashSet<IAdvice> GetAdvices(Type pointcutType) => aspectMap.GetValueOrDefault(pointcutType, []);
}
