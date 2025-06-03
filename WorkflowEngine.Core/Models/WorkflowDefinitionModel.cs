namespace WorkflowEngine.Core.Models;

public class WorkflowDefinitionModel
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<StepDefinitionModel> Steps { get; set; } = new();
}

public class StepDefinitionModel
{
    public string Name { get; set; } = string.Empty;
    public string StepType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; }
    public int DelayMinutes { get; set; }
    public object? Configuration { get; set; }
    public string? ConditionRules { get; set; }
}
