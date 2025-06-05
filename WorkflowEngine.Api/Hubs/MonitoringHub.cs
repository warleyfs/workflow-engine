using Microsoft.AspNetCore.SignalR;
using WorkflowEngine.Core.Models;

namespace WorkflowEngine.Api.Hubs;

public interface IMonitoringHubClient
{
    Task WorkflowExecutionStatusChanged(object executionUpdate);
    Task StepExecutionStatusChanged(object stepUpdate);
    Task DashboardMetricsUpdated(DashboardModel? dashboard);
    Task NewExecutionStarted(object execution);
    Task ExecutionCompleted(object execution);
}

public class MonitoringHub : Hub<IMonitoringHubClient>
{
    private readonly ILogger<MonitoringHub> _logger;

    public MonitoringHub(ILogger<MonitoringHub> logger)
    {
        _logger = logger;
    }

    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} joined group {GroupName}", Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} left group {GroupName}", Context.ConnectionId, groupName);
    }

    public async Task SubscribeToWorkflowExecution(string executionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"execution_{executionId}");
        _logger.LogInformation("Client {ConnectionId} subscribed to execution {ExecutionId}", Context.ConnectionId, executionId);
    }

    public async Task UnsubscribeFromWorkflowExecution(string executionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"execution_{executionId}");
        _logger.LogInformation("Client {ConnectionId} unsubscribed from execution {ExecutionId}", Context.ConnectionId, executionId);
    }

    public async Task SubscribeToDashboard()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "dashboard");
        _logger.LogInformation("Client {ConnectionId} subscribed to dashboard updates", Context.ConnectionId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client {ConnectionId} disconnected", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}

