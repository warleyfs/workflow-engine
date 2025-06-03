using System.Text.Json;

namespace WorkflowEngine.Core.Models;

public class StepContext
{
    public Guid WorkflowExecutionId { get; set; }
    public Guid StepExecutionId { get; set; }
    public Guid WorkflowDefinitionId { get; set; }
    public Guid StepDefinitionId { get; set; }
    public string StepType { get; set; } = string.Empty;
    public string StepName { get; set; } = string.Empty;
    public JsonDocument? Configuration { get; set; }
    public JsonDocument? InputData { get; set; }
    public Dictionary<string, object> WorkflowData { get; set; } = new();
    public Dictionary<string, object> StepData { get; set; } = new();
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; }
    public CancellationToken CancellationToken { get; set; }
}

