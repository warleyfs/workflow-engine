using WorkflowEngine.Core.Enums;

namespace WorkflowEngine.Core.Models;

public class WorkflowExecutionResult
{
    public Guid Id { get; set; }
    public Guid WorkflowDefinitionId { get; set; }
    public string WorkflowName { get; set; } = string.Empty;
    public WorkflowExecutionStatus Status { get; set; }
    public object? InputData { get; set; }
    public object? OutputData { get; set; }
    public DateTime? ScheduledTime { get; set; }
    public DateTime? StartedTime { get; set; }
    public DateTime? CompletedTime { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<StepExecutionResult> Steps { get; set; } = new();
}
