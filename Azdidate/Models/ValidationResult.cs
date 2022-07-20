namespace Azdidate.Models;

internal class ValidationResult
{
    internal ValidationResult(string logMessage, bool success)
    {
        LogMessage = logMessage;
        Success = success;
    }
    
    public string LogMessage { get; }
    public bool Success { get; }
}