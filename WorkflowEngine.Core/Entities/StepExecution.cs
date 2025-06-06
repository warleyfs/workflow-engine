using WorkflowEngine.Core.Enums;

namespace WorkflowEngine.Core.Entities;

public class StepExecution
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid WorkflowExecutionId { get; set; }
    
    public Guid WorkflowStepId { get; set; }
    
    public StepExecutionStatus Status { get; set; } = StepExecutionStatus.Pending;
    
    // JSON input data for this step
    public string? InputData { get; set; }
    
    // JSON output data from this step
    public string? OutputData { get; set; }
    
    public DateTime? ScheduledTime { get; set; }
    
    public DateTime? StartedTime { get; set; }
    
    public DateTime? CompletedTime { get; set; }
    
    public string? ErrorMessage { get; set; }
    
    public int RetryCount { get; set; } = 0;
    
    public int MaxRetries { get; set; } = 3;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual WorkflowExecution WorkflowExecution { get; set; } = null!;
    
    public virtual WorkflowStep WorkflowStep { get; set; } = null!;
}

