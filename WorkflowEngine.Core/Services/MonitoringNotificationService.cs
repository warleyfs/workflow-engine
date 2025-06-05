using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using WorkflowEngine.Core.Entities;
using WorkflowEngine.Core.Enums;
using WorkflowEngine.Core.Models;

namespace WorkflowEngine.Core.Services;

public interface IMonitoringNotificationService
{
    Task NotifyWorkflowExecutionStatusChanged(WorkflowExecution execution);
    Task NotifyStepExecutionStatusChanged(StepExecution stepExecution);
    Task NotifyNewExecutionStarted(WorkflowExecution execution);
    Task NotifyExecutionCompleted(WorkflowExecution execution);
    Task NotifyDashboardUpdate();
}

public class MonitoringNotificationService : IMonitoringNotificationService
{
    private readonly object? _hubContext;
    private readonly ILogger<MonitoringNotificationService> _logger;

    public MonitoringNotificationService(
        ILogger<MonitoringNotificationService> logger,
        object? hubContext = null)
    {
        _logger = logger;
        _hubContext = hubContext;
    }

    public async Task NotifyWorkflowExecutionStatusChanged(WorkflowExecution execution)
    {
        if (_hubContext == null) return;
        
        try
        {
            var update = new
            {
                ExecutionId = execution.Id,
                WorkflowDefinitionId = execution.WorkflowDefinitionId,
                Status = execution.Status.ToString(),
                StartedTime = execution.StartedTime,
                CompletedTime = execution.CompletedTime,
                ErrorMessage = execution.ErrorMessage,
                UpdatedAt = DateTime.UtcNow
            };

            // Notificar todos os clientes conectados
            var hubContext = (dynamic)_hubContext;
            await hubContext.Clients.All.SendAsync("WorkflowExecutionStatusChanged", update);

            // Notificar clientes específicos que subscreveram a esta execução
            await hubContext.Clients.Group($"execution_{execution.Id}")
                .SendAsync("WorkflowExecutionStatusChanged", update);

            _logger.LogDebug("Notified workflow execution status change: {ExecutionId} -> {Status}",
                execution.Id, execution.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying workflow execution status change for {ExecutionId}",
                execution.Id);
        }
    }

    public async Task NotifyStepExecutionStatusChanged(StepExecution stepExecution)
    {
        if (_hubContext == null) return;
        
        try
        {
            var update = new
            {
                StepExecutionId = stepExecution.Id,
                WorkflowExecutionId = stepExecution.WorkflowExecutionId,
                Status = stepExecution.Status.ToString(),
                StartedTime = stepExecution.StartedTime,
                CompletedTime = stepExecution.CompletedTime,
                RetryCount = stepExecution.RetryCount,
                ErrorMessage = stepExecution.ErrorMessage,
                UpdatedAt = DateTime.UtcNow
            };

            // Notificar clientes específicos que subscreveram a esta execução
            var hubContext = (dynamic)_hubContext;
            await hubContext.Clients.Group($"execution_{stepExecution.WorkflowExecutionId}")
                .SendAsync("StepExecutionStatusChanged", update);

            _logger.LogDebug("Notified step execution status change: {StepExecutionId} -> {Status}",
                stepExecution.Id, stepExecution.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying step execution status change for {StepExecutionId}",
                stepExecution.Id);
        }
    }

    public async Task NotifyNewExecutionStarted(WorkflowExecution execution)
    {
        if (_hubContext == null) return;
        
        try
        {
            var notification = new
            {
                ExecutionId = execution.Id,
                WorkflowDefinitionId = execution.WorkflowDefinitionId,
                WorkflowName = execution.WorkflowDefinition?.Name ?? "Unknown",
                Status = execution.Status.ToString(),
                CreatedAt = execution.CreatedAt,
                ScheduledTime = execution.ScheduledTime
            };

            var hubContext = (dynamic)_hubContext;
            await hubContext.Clients.All.SendAsync("NewExecutionStarted", notification);

            _logger.LogDebug("Notified new execution started: {ExecutionId}", execution.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying new execution started for {ExecutionId}",
                execution.Id);
        }
    }

    public async Task NotifyExecutionCompleted(WorkflowExecution execution)
    {
        if (_hubContext == null) return;
        
        try
        {
            var notification = new
            {
                ExecutionId = execution.Id,
                WorkflowDefinitionId = execution.WorkflowDefinitionId,
                WorkflowName = execution.WorkflowDefinition?.Name ?? "Unknown",
                Status = execution.Status.ToString(),
                CompletedTime = execution.CompletedTime,
                Duration = execution.StartedTime.HasValue && execution.CompletedTime.HasValue
                    ? (TimeSpan?)(execution.CompletedTime.Value - execution.StartedTime.Value)
                    : (TimeSpan?)null,
                ErrorMessage = execution.ErrorMessage
            };

            var hubContext = (dynamic)_hubContext;
            await hubContext.Clients.All.SendAsync("ExecutionCompleted", notification);

            _logger.LogDebug("Notified execution completed: {ExecutionId} -> {Status}",
                execution.Id, execution.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying execution completed for {ExecutionId}",
                execution.Id);
        }
    }

    public async Task NotifyDashboardUpdate()
    {
        if (_hubContext == null) return;
        
        try
        {
            // Esta notificação pode ser mais leve, apenas informando que o dashboard deve ser atualizado
            var update = new
            {
                UpdatedAt = DateTime.UtcNow,
                Message = "Dashboard metrics updated"
            };

            var hubContext = (dynamic)_hubContext;
            await hubContext.Clients.Group("dashboard").SendAsync("DashboardMetricsUpdated", update);

            _logger.LogDebug("Notified dashboard update");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying dashboard update");
        }
    }
}

// Interface moved to WorkflowEngine.Api.Hubs namespace
public interface IMonitoringHubClient
{
    Task WorkflowExecutionStatusChanged(object executionUpdate);
    Task StepExecutionStatusChanged(object stepUpdate);
    Task DashboardMetricsUpdated(DashboardModel? dashboard);
    Task NewExecutionStarted(object execution);
    Task ExecutionCompleted(object execution);
}

