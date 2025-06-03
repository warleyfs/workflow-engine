using WorkflowEngine.Core.Enums;

namespace WorkflowEngine.Core.Entities;

public class WorkflowExecution
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid WorkflowDefinitionId { get; set; }
    
    public WorkflowExecutionStatus Status { get; set; } = WorkflowExecutionStatus.Pending;
    
    // JSON input data for the workflow
    public string? InputData { get; set; }
    
    // JSON output data from the workflow
    public string? OutputData { get; set; }
    
    public DateTime? ScheduledTime { get; set; }
    public DateTime? StartedTime { get; set; }
    public DateTime? CompletedTime { get; set; }
    
    public string? ErrorMessage { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual WorkflowDefinition WorkflowDefinition { get; set; } = null!;
    public virtual ICollection<StepExecution> StepExecutions { get; set; } = new List<StepExecution>();
}

