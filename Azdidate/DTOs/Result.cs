namespace Azdidate.DTOs;

public class Result<T>
{
    public Result(T resultObject, string? errorMessage = null)
    {
        Object = resultObject;
        ErrorMessage = errorMessage ?? string.Empty;
    }
    
    public Result(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }
    
    public T? Object { get; }
    
    public string ErrorMessage { get; }
}