namespace Edt.Domain.Shared;

public sealed record Error(string Code, string Message, ErrorType Type)
{
    public static readonly Error None = new("", "", ErrorType.Failure);
}
