using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkflowEngine.Core.Data;
using WorkflowEngine.Core.Enums;
using WorkflowEngine.Core.Models;
using WorkflowEngine.Core.Entities;

namespace WorkflowEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MonitoringController : ControllerBase
{
    private readonly WorkflowDbContext _context;
    private readonly ILogger<MonitoringController> _logger;

    public MonitoringController(
        WorkflowDbContext context,
        ILogger<MonitoringController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Dashboard com métricas completas do sistema
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardModel>> GetDashboard()
    {
        try
        {
            var dashboard = new DashboardModel();

            // Contadores básicos
            dashboard.TotalWorkflows = await _context.WorkflowDefinitions.CountAsync(w => w.IsActive);
            dashboard.TotalExecutions = await _context.WorkflowExecutions.CountAsync();

            // Estatísticas por status
            var statusStats = await _context.WorkflowExecutions
                .GroupBy(e => e.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var totalExec = statusStats.Sum(s => s.Count);
            dashboard.ExecutionsByStatus = statusStats.Select(s => new ExecutionStatusCount
            {
                Status = s.Status.ToString(),
                Count = s.Count,
                Percentage = totalExec > 0 ? Math.Round((double)s.Count / totalExec * 100, 2) : 0
            }).ToList();

            // Execuções recentes
            var recentExecs = await _context.WorkflowExecutions
                .Include(e => e.WorkflowDefinition)
                .OrderByDescending(e => e.CreatedAt)
                .Take(10)
                .Select(e => new RecentExecutionSummary
                {
                    Id = e.Id,
                    WorkflowName = e.WorkflowDefinition.Name,
                    Status = e.Status,
                    CreatedAt = e.CreatedAt,
                    CompletedTime = e.CompletedTime,
                    Duration = e.StartedTime.HasValue && e.CompletedTime.HasValue 
                        ? e.CompletedTime.Value - e.StartedTime.Value 
                        : null
                })
                .ToListAsync();

            dashboard.RecentExecutions = recentExecs;

            // Métricas de performance
            var now = DateTime.UtcNow;
            var last24Hours = now.AddHours(-24);
            var last7Days = now.AddDays(-7);

            dashboard.Performance.ExecutionsLast24Hours = await _context.WorkflowExecutions
                .CountAsync(e => e.CreatedAt >= last24Hours);

            dashboard.Performance.ExecutionsLast7Days = await _context.WorkflowExecutions
                .CountAsync(e => e.CreatedAt >= last7Days);

            dashboard.Performance.ActiveExecutions = await _context.WorkflowExecutions
                .CountAsync(e => e.Status == WorkflowExecutionStatus.Running || 
                               e.Status == WorkflowExecutionStatus.Pending);

            // Taxa de sucesso
            var completedCount = await _context.WorkflowExecutions
                .CountAsync(e => e.Status == WorkflowExecutionStatus.Completed);
            var failedCount = await _context.WorkflowExecutions
                .CountAsync(e => e.Status == WorkflowExecutionStatus.Failed);

            var totalFinished = completedCount + failedCount;
            if (totalFinished > 0)
            {
                dashboard.Performance.SuccessRate = Math.Round((double)completedCount / totalFinished * 100, 2);
                dashboard.Performance.FailureRate = Math.Round((double)failedCount / totalFinished * 100, 2);
            }

            // Tempo médio de execução (calculado no código)
            var executionTimes = await _context.WorkflowExecutions
                .Where(e => e.StartedTime.HasValue && e.CompletedTime.HasValue)
                .Select(e => new { StartedTime = e.StartedTime!.Value, CompletedTime = e.CompletedTime!.Value })
                .ToListAsync();

            if (executionTimes.Any())
            {
                var avgExecutionTime = executionTimes
                    .Select(e => (e.CompletedTime - e.StartedTime).TotalMinutes)
                    .Average();
                dashboard.Performance.AverageExecutionTimeMinutes = Math.Round(avgExecutionTime, 2);
            }

            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard data");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Lista execuções com filtros e paginação
    /// </summary>
    [HttpGet("executions")]
    public async Task<ActionResult<ExecutionListResponse>> GetExecutions(
        [FromQuery] WorkflowExecutionStatus? status = null,
        [FromQuery] Guid? workflowDefinitionId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            if (pageSize > 100) pageSize = 100; // Limit max page size
            if (page < 1) page = 1;

            var query = _context.WorkflowExecutions
                .Include(e => e.WorkflowDefinition)
                .Include(e => e.StepExecutions)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(e => e.Status == status.Value);

            if (workflowDefinitionId.HasValue)
                query = query.Where(e => e.WorkflowDefinitionId == workflowDefinitionId.Value);

            if (startDate.HasValue)
                query = query.Where(e => e.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(e => e.CreatedAt <= endDate.Value);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var executions = await query
                .OrderByDescending(e => e.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new ExecutionSummary
                {
                    Id = e.Id,
                    WorkflowDefinitionId = e.WorkflowDefinitionId,
                    WorkflowName = e.WorkflowDefinition.Name,
                    Status = e.Status,
                    CreatedAt = e.CreatedAt,
                    StartedTime = e.StartedTime,
                    CompletedTime = e.CompletedTime,
                    Duration = e.StartedTime.HasValue && e.CompletedTime.HasValue 
                        ? e.CompletedTime.Value - e.StartedTime.Value 
                        : null,
                    ErrorMessage = e.ErrorMessage,
                    TotalSteps = e.StepExecutions.Count,
                    CompletedSteps = e.StepExecutions.Count(s => s.Status == StepExecutionStatus.Completed),
                    FailedSteps = e.StepExecutions.Count(s => s.Status == StepExecutionStatus.Failed)
                })
                .ToListAsync();

            var result = new ExecutionListResponse
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                Executions = executions
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting executions");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Métricas de um workflow específico
    /// </summary>
    [HttpGet("workflow/{workflowId}/metrics")]
    public async Task<ActionResult<object>> GetWorkflowMetrics(Guid workflowId)
    {
        try
        {
            var workflow = await _context.WorkflowDefinitions
                .FirstOrDefaultAsync(w => w.Id == workflowId);

            if (workflow == null)
                return NotFound("Workflow not found");

            var executions = await _context.WorkflowExecutions
                .Where(e => e.WorkflowDefinitionId == workflowId)
                .ToListAsync();

            var totalExecutions = executions.Count;
            var completedExecutions = executions.Count(e => e.Status == WorkflowExecutionStatus.Completed);
            var failedExecutions = executions.Count(e => e.Status == WorkflowExecutionStatus.Failed);
            var activeExecutions = executions.Count(e => e.Status == WorkflowExecutionStatus.Running || 
                                                        e.Status == WorkflowExecutionStatus.Pending);

            var avgExecutionTime = executions
                .Where(e => e.StartedTime.HasValue && e.CompletedTime.HasValue)
                .Select(e => (e.CompletedTime!.Value - e.StartedTime!.Value).TotalMinutes)
                .DefaultIfEmpty(0)
                .Average();

            var metrics = new
            {
                WorkflowId = workflowId,
                WorkflowName = workflow.Name,
                TotalExecutions = totalExecutions,
                CompletedExecutions = completedExecutions,
                FailedExecutions = failedExecutions,
                ActiveExecutions = activeExecutions,
                SuccessRate = totalExecutions > 0 ? Math.Round((double)completedExecutions / totalExecutions * 100, 2) : 0,
                FailureRate = totalExecutions > 0 ? Math.Round((double)failedExecutions / totalExecutions * 100, 2) : 0,
                AverageExecutionTimeMinutes = Math.Round(avgExecutionTime, 2),
                LastExecution = executions.OrderByDescending(e => e.CreatedAt).FirstOrDefault()?.CreatedAt,
                ExecutionsByStatus = executions.GroupBy(e => e.Status)
                    .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                    .ToList()
            };

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow metrics for {WorkflowId}", workflowId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Estatísticas de steps mais utilizados
    /// </summary>
    [HttpGet("steps/statistics")]
    public async Task<ActionResult<object>> GetStepStatistics()
    {
        try
        {
            var stepStats = await _context.StepExecutions
                .Include(se => se.WorkflowStep)
                .ThenInclude(ws => ws.StepDefinition)
                .GroupBy(se => se.WorkflowStep.StepDefinition.StepType)
                .Select(g => new
                {
                    StepType = g.Key,
                    TotalExecutions = g.Count(),
                    CompletedExecutions = g.Count(se => se.Status == StepExecutionStatus.Completed),
                    FailedExecutions = g.Count(se => se.Status == StepExecutionStatus.Failed),
                    AverageRetries = g.Average(se => se.RetryCount)
                })
                .OrderByDescending(s => s.TotalExecutions)
                .ToListAsync();

            return Ok(stepStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting step statistics");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Logs de uma execução específica
    /// </summary>
    [HttpGet("execution/{executionId}/logs")]
    public async Task<ActionResult<object>> GetExecutionLogs(Guid executionId)
    {
        try
        {
            var execution = await _context.WorkflowExecutions
                .Include(e => e.StepExecutions)
                .ThenInclude(se => se.WorkflowStep)
                .ThenInclude(ws => ws.StepDefinition)
                .FirstOrDefaultAsync(e => e.Id == executionId);

            if (execution == null)
                return NotFound("Execution not found");

            var logs = new
            {
                ExecutionId = executionId,
                WorkflowName = execution.WorkflowDefinition?.Name ?? "Unknown",
                Status = execution.Status.ToString(),
                CreatedAt = execution.CreatedAt,
                StartedTime = execution.StartedTime,
                CompletedTime = execution.CompletedTime,
                ErrorMessage = execution.ErrorMessage,
                Steps = execution.StepExecutions
                    .OrderBy(se => se.WorkflowStep.Order)
                    .Select(se => new
                    {
                        StepId = se.Id,
                        StepName = se.WorkflowStep.StepDefinition.Name,
                        StepType = se.WorkflowStep.StepDefinition.StepType,
                        Order = se.WorkflowStep.Order,
                        Status = se.Status.ToString(),
                        CreatedAt = se.CreatedAt,
                        StartedTime = se.StartedTime,
                        CompletedTime = se.CompletedTime,
                        RetryCount = se.RetryCount,
                        ErrorMessage = se.ErrorMessage,
                        OutputData = se.OutputData
                    })
                    .ToList()
            };

            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting execution logs for {ExecutionId}", executionId);
            return StatusCode(500, "Internal server error");
        }
    }
}

