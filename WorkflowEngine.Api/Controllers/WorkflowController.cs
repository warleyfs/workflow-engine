using Microsoft.AspNetCore.Mvc;
using WorkflowEngine.Core.Interfaces;
using WorkflowEngine.Core.Models;
using WorkflowEngine.Core.Data;
using Microsoft.EntityFrameworkCore;
using WorkflowEngine.Core.Entities;
using System.Text.Json;

namespace WorkflowEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkflowController : ControllerBase
{
    private readonly IWorkflowEngine _workflowEngine;
    private readonly WorkflowDbContext _context;
    private readonly ILogger<WorkflowController> _logger;

    public WorkflowController(
        IWorkflowEngine workflowEngine,
        WorkflowDbContext context,
        ILogger<WorkflowController> logger)
    {
        _workflowEngine = workflowEngine;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Iniciar execução de um workflow
    /// </summary>
    [HttpPost("{workflowDefinitionId}/execute")]
    public async Task<ActionResult<Guid>> StartWorkflow(
        Guid workflowDefinitionId,
        [FromBody] StartWorkflowRequest request)
    {
        try
        {
            var executionId = await _workflowEngine.StartWorkflowAsync(
                workflowDefinitionId,
                request.InputData,
                request.ScheduledTime);

            return Ok(new { ExecutionId = executionId });
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting workflow {WorkflowDefinitionId}", workflowDefinitionId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Obter status de execução de um workflow
    /// </summary>
    [HttpGet("execution/{executionId}")]
    public async Task<ActionResult<WorkflowExecutionResult>> GetWorkflowStatus(Guid executionId)
    {
        try
        {
            var result = await _workflowEngine.GetWorkflowStatusAsync(executionId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow status {ExecutionId}", executionId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Cancelar execução de um workflow
    /// </summary>
    [HttpPost("execution/{executionId}/cancel")]
    public async Task<ActionResult> CancelWorkflow(Guid executionId)
    {
        try
        {
            var success = await _workflowEngine.CancelWorkflowAsync(executionId);
            if (success)
                return Ok(new { Message = "Workflow cancelled successfully" });
            else
                return BadRequest("Could not cancel workflow. It may be already completed or cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling workflow {ExecutionId}", executionId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Pausar execução de um workflow
    /// </summary>
    [HttpPost("execution/{executionId}/pause")]
    public async Task<ActionResult> PauseWorkflow(Guid executionId)
    {
        try
        {
            var success = await _workflowEngine.PauseWorkflowAsync(executionId);
            if (success)
                return Ok(new { Message = "Workflow paused successfully" });
            else
                return BadRequest("Could not pause workflow. It may not be running.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing workflow {ExecutionId}", executionId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Retomar execução de um workflow
    /// </summary>
    [HttpPost("execution/{executionId}/resume")]
    public async Task<ActionResult> ResumeWorkflow(Guid executionId)
    {
        try
        {
            var success = await _workflowEngine.ResumeWorkflowAsync(executionId);
            if (success)
                return Ok(new { Message = "Workflow resumed successfully" });
            else
                return BadRequest("Could not resume workflow. It may not be paused.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming workflow {ExecutionId}", executionId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Listar todas as definições de workflow
    /// </summary>
    [HttpGet("definitions")]
    public async Task<ActionResult<IEnumerable<object>>> GetWorkflowDefinitions()
    {
        try
        {
            var definitions = await _context.WorkflowDefinitions
                .Include(w => w.Steps)
                .ThenInclude(s => s.StepDefinition)
                .Where(w => w.IsActive)
                .Select(w => new
                {
                    w.Id,
                    w.Name,
                    w.Description,
                    w.CreatedAt,
                    StepCount = w.Steps.Count,
                    Steps = w.Steps.OrderBy(s => s.Order).Select(s => new
                    {
                        s.Order,
                        s.DelayMinutes,
                        StepName = s.StepDefinition.Name,
                        StepType = s.StepDefinition.StepType
                    })
                })
                .ToListAsync();

            return Ok(definitions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow definitions");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Criar uma nova definição de workflow simples para demonstração
    /// </summary>
    [HttpPost("demo")]
    public async Task<ActionResult<Guid>> CreateDemoWorkflow()
    {
        try
        {
            // Criar etapas de definição
            var logStepDef = new StepDefinition
            {
                Name = "Log Start",
                StepType = "LogStep",
                Description = "Log the start of the workflow",
                Configuration = JsonSerializer.Serialize(new { Message = "Workflow started!", Level = "Information" })
            };

            var delayStepDef = new StepDefinition
            {
                Name = "Wait 5 seconds",
                StepType = "DelayStep",
                Description = "Wait for 5 seconds",
                Configuration = JsonSerializer.Serialize(new { DelaySeconds = 5 })
            };

            var emailStepDef = new StepDefinition
            {
                Name = "Send notification",
                StepType = "EmailStep",
                Description = "Send notification email",
                Configuration = JsonSerializer.Serialize(new 
                { 
                    To = "user@example.com", 
                    Subject = "Workflow Completed", 
                    Body = "Your workflow has been completed successfully!" 
                })
            };

            var logEndStepDef = new StepDefinition
            {
                Name = "Log End",
                StepType = "LogStep",
                Description = "Log the end of the workflow",
                Configuration = JsonSerializer.Serialize(new { Message = "Workflow completed!", Level = "Information" })
            };

            _context.StepDefinitions.AddRange(logStepDef, delayStepDef, emailStepDef, logEndStepDef);
            await _context.SaveChangesAsync();

            // Criar workflow
            var workflowDef = new WorkflowDefinition
            {
                Name = "Demo Workflow",
                Description = "A demonstration workflow with log, delay, and email steps"
            };

            _context.WorkflowDefinitions.Add(workflowDef);
            await _context.SaveChangesAsync();

            // Criar etapas do workflow
            var workflowSteps = new[]
            {
                new WorkflowStep
                {
                    WorkflowDefinitionId = workflowDef.Id,
                    StepDefinitionId = logStepDef.Id,
                    Order = 1,
                    DelayMinutes = 0
                },
                new WorkflowStep
                {
                    WorkflowDefinitionId = workflowDef.Id,
                    StepDefinitionId = delayStepDef.Id,
                    Order = 2,
                    DelayMinutes = 0
                },
                new WorkflowStep
                {
                    WorkflowDefinitionId = workflowDef.Id,
                    StepDefinitionId = emailStepDef.Id,
                    Order = 3,
                    DelayMinutes = 1 // 1 minuto de delay
                },
                new WorkflowStep
                {
                    WorkflowDefinitionId = workflowDef.Id,
                    StepDefinitionId = logEndStepDef.Id,
                    Order = 4,
                    DelayMinutes = 0
                }
            };

            _context.WorkflowSteps.AddRange(workflowSteps);
            await _context.SaveChangesAsync();

            return Ok(new { WorkflowDefinitionId = workflowDef.Id, Message = "Demo workflow created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating demo workflow");
            return StatusCode(500, "Internal server error");
        }
    }
}

public class StartWorkflowRequest
{
    public object? InputData { get; set; }
    public DateTime? ScheduledTime { get; set; }
}

