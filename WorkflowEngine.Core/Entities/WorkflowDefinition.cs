using System.ComponentModel.DataAnnotations;

namespace WorkflowEngine.Core.Entities;

public class WorkflowDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
    public virtual ICollection<WorkflowExecution> Executions { get; set; } = new List<WorkflowExecution>();
}

