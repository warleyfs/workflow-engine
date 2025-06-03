using System.ComponentModel.DataAnnotations;

namespace WorkflowEngine.Core.Entities;

public class StepDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string StepType { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    // JSON configuration for the step
    public string? Configuration { get; set; }
    
    // JSON schema for input validation
    public string? InputSchema { get; set; }
    
    // JSON schema for output validation
    public string? OutputSchema { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<WorkflowStep> WorkflowSteps { get; set; } = new List<WorkflowStep>();
}

