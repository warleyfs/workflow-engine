using System.Text.Json;

namespace WorkflowEngine.Core.Models;

public class StepResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public JsonDocument? OutputData { get; set; }
    public bool ShouldRetry { get; set; }
    public TimeSpan? RetryDelay { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    public static StepResult Success(object? outputData = null)
    {
        return new StepResult
        {
            IsSuccess = true,
            OutputData = outputData != null ? JsonSerializer.SerializeToDocument(outputData) : null
        };
    }
    
    public static StepResult Failure(string errorMessage, bool shouldRetry = false, TimeSpan? retryDelay = null)
    {
        return new StepResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            ShouldRetry = shouldRetry,
            RetryDelay = retryDelay
        };
    }
}

