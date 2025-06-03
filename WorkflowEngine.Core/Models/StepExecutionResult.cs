using WorkflowEngine.Core.Enums;

namespace WorkflowEngine.Core.Models;

public class StepExecutionResult
{
    public Guid Id { get; set; }
    public string StepName { get; set; } = string.Empty;
    public string StepType { get; set; } = string.Empty;
    public int Order { get; set; }
    public StepExecutionStatus Status { get; set; }
    public object? InputData { get; set; }
    public object? OutputData { get; set; }
    public DateTime? ScheduledTime { get; set; }
    public DateTime? StartedTime { get; set; }
    public DateTime? CompletedTime { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; }
    public DateTime CreatedAt { get; set; }
}
