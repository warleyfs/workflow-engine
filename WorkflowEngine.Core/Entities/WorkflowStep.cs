namespace WorkflowEngine.Core.Entities;

public class WorkflowStep
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid WorkflowDefinitionId { get; set; }
    public Guid StepDefinitionId { get; set; }
    
    public int Order { get; set; }
    
    // JSON conditions for when this step should execute
    public string? ConditionRules { get; set; }
    
    // Delay before executing this step (in minutes)
    public int DelayMinutes { get; set; } = 0;
    
    // Step-specific configuration (overrides StepDefinition configuration)
    public string? StepConfiguration { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual WorkflowDefinition WorkflowDefinition { get; set; } = null!;
    public virtual StepDefinition StepDefinition { get; set; } = null!;
    public virtual ICollection<StepExecution> Executions { get; set; } = new List<StepExecution>();
}

