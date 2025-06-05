using WorkflowEngine.Core.Enums;

namespace WorkflowEngine.Core.Models;

public class DashboardModel
{
    public int TotalWorkflows { get; set; }
    public int TotalExecutions { get; set; }
    public List<ExecutionStatusCount> ExecutionsByStatus { get; set; } = new();
    public List<RecentExecutionSummary> RecentExecutions { get; set; } = new();
    public WorkflowPerformanceMetrics Performance { get; set; } = new();
}

public class ExecutionStatusCount
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class RecentExecutionSummary
{
    public Guid Id { get; set; }
    public string WorkflowName { get; set; } = string.Empty;
    public WorkflowExecutionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedTime { get; set; }
    public TimeSpan? Duration { get; set; }
}

public class WorkflowPerformanceMetrics
{
    public double AverageExecutionTimeMinutes { get; set; }
    public int ExecutionsLast24Hours { get; set; }
    public int ExecutionsLast7Days { get; set; }
    public double SuccessRate { get; set; }
    public double FailureRate { get; set; }
    public int ActiveExecutions { get; set; }
}

public class ExecutionListResponse
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public List<ExecutionSummary> Executions { get; set; } = new();
}

public class ExecutionSummary
{
    public Guid Id { get; set; }
    public Guid WorkflowDefinitionId { get; set; }
    public string WorkflowName { get; set; } = string.Empty;
    public WorkflowExecutionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedTime { get; set; }
    public DateTime? CompletedTime { get; set; }
    public TimeSpan? Duration { get; set; }
    public string? ErrorMessage { get; set; }
    public int TotalSteps { get; set; }
    public int CompletedSteps { get; set; }
    public int FailedSteps { get; set; }
}

