using WorkflowEngine.Core.Models;

namespace WorkflowEngine.Core.Interfaces;

public interface IWorkflowStep
{
    string StepType { get; }
    Task<StepResult> ExecuteAsync(StepContext context, CancellationToken cancellationToken = default);
    Task<bool> CanExecuteAsync(StepContext context, CancellationToken cancellationToken = default);
    Task<StepResult> ValidateInputAsync(StepContext context, CancellationToken cancellationToken = default);
}

