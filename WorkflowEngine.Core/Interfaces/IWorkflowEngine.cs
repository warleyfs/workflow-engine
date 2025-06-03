using WorkflowEngine.Core.Models;

namespace WorkflowEngine.Core.Interfaces;

public interface IWorkflowEngine
{
    Task<Guid> StartWorkflowAsync(Guid workflowDefinitionId, object? inputData = null, DateTime? scheduledTime = null, CancellationToken cancellationToken = default);
    Task<WorkflowExecutionResult> GetWorkflowStatusAsync(Guid workflowExecutionId, CancellationToken cancellationToken = default);
    Task<bool> CancelWorkflowAsync(Guid workflowExecutionId, CancellationToken cancellationToken = default);
    Task<bool> PauseWorkflowAsync(Guid workflowExecutionId, CancellationToken cancellationToken = default);
    Task<bool> ResumeWorkflowAsync(Guid workflowExecutionId, CancellationToken cancellationToken = default);
    Task ProcessWorkflowAsync(Guid workflowExecutionId, CancellationToken cancellationToken = default);
    Task ProcessStepAsync(Guid stepExecutionId, CancellationToken cancellationToken = default);
}

