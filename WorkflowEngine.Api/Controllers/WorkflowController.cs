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
    /// Criar definição de workflow
    /// </summary>
    /// <param name="workflow"></param>
    /// <returns></returns>
    [HttpPost("definitions")]
    public async Task<ActionResult<Guid>> CreateWorkflowDefinition(WorkflowDefinitionModel workflow)
    {
        try
        {
            // Cria as definições das steps
            var stepDefs = workflow.Steps.Select(s => new StepDefinition()
            {
                Name = s.Name,
                StepType = s.StepType,
                Description = s.Description,
                Configuration = JsonSerializer.Serialize(s.Configuration),
            }).ToArray();
            
            await _context.StepDefinitions.AddRangeAsync(stepDefs);
            await _context.SaveChangesAsync();
            
            // Criar workflow
            var workflowDef = new WorkflowDefinition
            {
                Name = workflow.Name,
                Description = workflow.Description,
            };

            _context.WorkflowDefinitions.Add(workflowDef);
            await _context.SaveChangesAsync();
            
            // Criar etapas do workflow
            for (var i = 0; i < stepDefs.Length; i++)
            {
                _context.WorkflowSteps.AddRange(new WorkflowStep
                {
                    WorkflowDefinitionId = workflowDef.Id,
                    StepDefinitionId = stepDefs[i].Id,
                    Order = i,
                    DelayMinutes = 0
                });    
            }
            
            await _context.SaveChangesAsync();

            return Ok(new { WorkflowDefinitionId = workflowDef.Id, Message = "Workflow de demonstração criado com sucesso!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocorreu um erro ao criar o workflow.");
            return StatusCode(500, "Ocorreu um erro inexperado");
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
                Name = "Logar Início",
                StepType = "LogStep",
                Description = "Grava um log com o início do workflow",
                Configuration = JsonSerializer.Serialize(new { Message = "Workflow started!", Level = "Information" })
            };

            var delayStepDef = new StepDefinition
            {
                Name = "Aguardar 5 minutos",
                StepType = "DelayStep",
                Description = "Aguardar 5 minutos",
                Configuration = JsonSerializer.Serialize(new { DelaySeconds = (5 * 60) })
            };

            var emailStepDef = new StepDefinition
            {
                Name = "Enviar Email",
                StepType = "EmailStep",
                Description = "Enviar email de notificação",
                Configuration = JsonSerializer.Serialize(new 
                { 
                    To = "user@example.com", 
                    Subject = "Workflow Finalizado", 
                    Body = "Seu workflow finalizou com sucesso!" 
                })
            };

            var logEndStepDef = new StepDefinition
            {
                Name = "Logar Finalização",
                StepType = "LogStep",
                Description = "Grava log com a finalização do workflow",
                Configuration = JsonSerializer.Serialize(new { Message = "Workflow finalizado!", Level = "Information" })
            };

            _context.StepDefinitions.AddRange(logStepDef, delayStepDef, emailStepDef, logEndStepDef);
            await _context.SaveChangesAsync();

            // Criar workflow
            var workflowDef = new WorkflowDefinition
            {
                Name = "Demo Workflow",
                Description = "Um workflow de demonstração com etapas de log, delay e envio de email."
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

            return Ok(new { WorkflowDefinitionId = workflowDef.Id, Message = "Workflow de demonstração criado com sucesso!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocorreu um erro ao criar o workflow de demonstração.");
            return StatusCode(500, "Ocorreu um erro inexperado");
        }
    }
}

public class StartWorkflowRequest
{
    public object? InputData { get; set; }
    public DateTime? ScheduledTime { get; set; }
}

