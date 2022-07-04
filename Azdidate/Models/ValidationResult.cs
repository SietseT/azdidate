using Azdidate.Enums;

namespace Azdidate.Models;

internal class ValidationResult
{
    internal ValidationResult(ValidationStateEnum state, string? message = null)
    {
        State = state;
        Message = message;
    }

    public ValidationStateEnum State { get; }
    public string? Message { get; }
}