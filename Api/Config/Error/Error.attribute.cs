namespace Api.Config.Error;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public sealed class ProducesErrorCodesAttribute(params int[] statusCodes) : Attribute
{
    public IReadOnlyCollection<int> StatusCodes { get; } = statusCodes;
}
