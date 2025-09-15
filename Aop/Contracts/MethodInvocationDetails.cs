namespace Workplace.Aop.Contracts;

public record MethodInvocationDetails
{
    public required string Name { get; init; }
    public required object Target { get; init; }
    public required object?[] Args { get; init; }
    public required Func<object?> Next { get; init; }
}
