using WorkflowEngine.Core.Models;

namespace WorkflowEngine.Core.Interfaces;

public interface IWorkflowBuilder
{
    IWorkflowBuilder AddStep<T>(string stepName, int order, int delayMinutes = 0, object? configuration = null) where T : IWorkflowStep;
    IWorkflowBuilder AddStep(string stepType, string stepName, int order, int delayMinutes = 0, object? configuration = null);
    IWorkflowBuilder AddCondition(int stepOrder, string conditionExpression);
    Task<Guid> SaveAsync(string workflowName, string? description = null, CancellationToken cancellationToken = default);
    WorkflowDefinitionModel Build();
}

