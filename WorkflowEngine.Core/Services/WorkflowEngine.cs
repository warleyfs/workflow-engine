using System.Text.Json;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorkflowEngine.Core.Entities;
using WorkflowEngine.Core.Enums;
using WorkflowEngine.Core.Interfaces;
using WorkflowEngine.Core.Models;
using WorkflowEngine.Core.Data;

namespace WorkflowEngine.Core.Services;

public class WorkflowEngine(
    WorkflowDbContext context,
    IServiceProvider serviceProvider,
    ILogger<WorkflowEngine> logger,
    IBackgroundJobClient backgroundJobClient,
    IMonitoringNotificationService? notificationService = null)
    : IWorkflowEngine
{
    public async Task<Guid> StartWorkflowAsync(
        Guid workflowDefinitionId,
        object? inputData = null,
        DateTime? scheduledTime = null,
        CancellationToken cancellationToken = default)
    {
        var workflowDefinition = await context.WorkflowDefinitions
            .Include(w => w.Steps)
            .ThenInclude(s => s.StepDefinition)
            .FirstOrDefaultAsync(w => w.Id == workflowDefinitionId && w.IsActive, cancellationToken);

        if (workflowDefinition == null)
            throw new ArgumentException($"Workflow definition {workflowDefinitionId} not found or inactive");

        var workflowExecution = new WorkflowExecution
        {
            WorkflowDefinitionId = workflowDefinitionId,
            InputData = inputData != null ? JsonSerializer.Serialize(inputData) : null,
            ScheduledTime = scheduledTime,
            Status = scheduledTime.HasValue ? WorkflowExecutionStatus.Pending : WorkflowExecutionStatus.Running
        };

        context.WorkflowExecutions.Add(workflowExecution);
        await context.SaveChangesAsync(cancellationToken);

        // Create step executions
        var stepExecutions = workflowDefinition.Steps
            .OrderBy(s => s.Order)
            .Select(step => new StepExecution
            {
                WorkflowExecutionId = workflowExecution.Id,
                WorkflowStepId = step.Id,
                ScheduledTime = scheduledTime?.AddMinutes(step.DelayMinutes),
                Status = StepExecutionStatus.Pending
            })
            .ToList();

        context.StepExecutions.AddRange(stepExecutions);
        await context.SaveChangesAsync(cancellationToken);

        // Schedule workflow execution
        if (scheduledTime.HasValue)
        {
            backgroundJobClient.Schedule(
                () => ProcessWorkflowAsync(workflowExecution.Id, CancellationToken.None),
                scheduledTime.Value);
        }
        else
        {
            backgroundJobClient.Enqueue(
                () => ProcessWorkflowAsync(workflowExecution.Id, CancellationToken.None));
        }

        logger.LogInformation("Started workflow {WorkflowId} execution {ExecutionId}",
            workflowDefinitionId, workflowExecution.Id);

        // Notify about new execution
        if (notificationService != null)
        {
            await notificationService.NotifyNewExecutionStarted(workflowExecution);
        }

        return workflowExecution.Id;
    }

    public async Task<WorkflowExecutionResult> GetWorkflowStatusAsync(
        Guid workflowExecutionId,
        CancellationToken cancellationToken = default)
    {
        var execution = await context.WorkflowExecutions
            .Include(e => e.WorkflowDefinition)
            .Include(e => e.StepExecutions)
            .ThenInclude(se => se.WorkflowStep)
            .ThenInclude(ws => ws.StepDefinition)
            .FirstOrDefaultAsync(e => e.Id == workflowExecutionId, cancellationToken);

        if (execution == null)
            throw new ArgumentException($"Workflow execution {workflowExecutionId} not found");

        var result = new WorkflowExecutionResult
        {
            Id = execution.Id,
            WorkflowDefinitionId = execution.WorkflowDefinitionId,
            WorkflowName = execution.WorkflowDefinition.Name,
            Status = execution.Status,
            InputData = execution.InputData != null ? JsonSerializer.Deserialize<object>(execution.InputData) : null,
            OutputData = execution.OutputData != null ? JsonSerializer.Deserialize<object>(execution.OutputData) : null,
            ScheduledTime = execution.ScheduledTime,
            StartedTime = execution.StartedTime,
            CompletedTime = execution.CompletedTime,
            ErrorMessage = execution.ErrorMessage,
            CreatedAt = execution.CreatedAt,
            Steps = execution.StepExecutions
                .OrderBy(se => se.WorkflowStep.Order)
                .Select(se => new StepExecutionResult
                {
                    Id = se.Id,
                    StepName = se.WorkflowStep.StepDefinition.Name,
                    StepType = se.WorkflowStep.StepDefinition.StepType,
                    Order = se.WorkflowStep.Order,
                    Status = se.Status,
                    InputData = se.InputData != null ? JsonSerializer.Deserialize<object>(se.InputData) : null,
                    OutputData = se.OutputData != null ? JsonSerializer.Deserialize<object>(se.OutputData) : null,
                    ScheduledTime = se.ScheduledTime,
                    StartedTime = se.StartedTime,
                    CompletedTime = se.CompletedTime,
                    ErrorMessage = se.ErrorMessage,
                    RetryCount = se.RetryCount,
                    MaxRetries = se.MaxRetries,
                    CreatedAt = se.CreatedAt
                })
                .ToList()
        };

        return result;
    }

    public async Task<bool> CancelWorkflowAsync(
        Guid workflowExecutionId,
        CancellationToken cancellationToken = default)
    {
        var execution = await context.WorkflowExecutions
            .FirstOrDefaultAsync(e => e.Id == workflowExecutionId, cancellationToken);

        if (execution == null || 
            execution.Status == WorkflowExecutionStatus.Completed ||
            execution.Status == WorkflowExecutionStatus.Failed ||
            execution.Status == WorkflowExecutionStatus.Cancelled)
            return false;

        execution.Status = WorkflowExecutionStatus.Cancelled;
        execution.CompletedTime = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        // Notify status change
        if (notificationService != null)
        {
            await notificationService.NotifyWorkflowExecutionStatusChanged(execution);
        }

        logger.LogInformation("Cancelled workflow execution {ExecutionId}", workflowExecutionId);
        return true;
    }

    public async Task<bool> PauseWorkflowAsync(
        Guid workflowExecutionId,
        CancellationToken cancellationToken = default)
    {
        var execution = await context.WorkflowExecutions
            .FirstOrDefaultAsync(e => e.Id == workflowExecutionId, cancellationToken);

        if (execution == null || execution.Status != WorkflowExecutionStatus.Running)
            return false;

        execution.Status = WorkflowExecutionStatus.Paused;
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Paused workflow execution {ExecutionId}", workflowExecutionId);
        return true;
    }

    public async Task<bool> ResumeWorkflowAsync(
        Guid workflowExecutionId,
        CancellationToken cancellationToken = default)
    {
        var execution = await context.WorkflowExecutions
            .FirstOrDefaultAsync(e => e.Id == workflowExecutionId, cancellationToken);

        if (execution == null || execution.Status != WorkflowExecutionStatus.Paused)
            return false;

        execution.Status = WorkflowExecutionStatus.Running;
        await context.SaveChangesAsync(cancellationToken);

        backgroundJobClient.Enqueue(
            () => ProcessWorkflowAsync(workflowExecutionId, CancellationToken.None));

        logger.LogInformation("Resumed workflow execution {ExecutionId}", workflowExecutionId);
        return true;
    }

    public async Task ProcessWorkflowAsync(
        Guid workflowExecutionId,
        CancellationToken cancellationToken = default)
    {
        var execution = await context.WorkflowExecutions
            .Include(e => e.WorkflowDefinition)
            .Include(e => e.StepExecutions)
            .ThenInclude(se => se.WorkflowStep)
            .ThenInclude(ws => ws.StepDefinition)
            .FirstOrDefaultAsync(e => e.Id == workflowExecutionId, cancellationToken);

        if (execution == null)
        {
            logger.LogError("Workflow execution {ExecutionId} not found", workflowExecutionId);
            return;
        }

        if (execution.Status == WorkflowExecutionStatus.Cancelled ||
            execution.Status == WorkflowExecutionStatus.Completed ||
            execution.Status == WorkflowExecutionStatus.Failed)
        {
            logger.LogInformation("Workflow execution {ExecutionId} is already in final state: {Status}",
                workflowExecutionId, execution.Status);
            return;
        }

        if (execution.Status == WorkflowExecutionStatus.Paused)
        {
            logger.LogInformation("Workflow execution {ExecutionId} is paused", workflowExecutionId);
            return;
        }

        try
        {
            // Update workflow status to running if it's pending
            if (execution.Status == WorkflowExecutionStatus.Pending)
            {
                execution.Status = WorkflowExecutionStatus.Running;
                execution.StartedTime = DateTime.UtcNow;
                await context.SaveChangesAsync(cancellationToken);
            }

            logger.LogInformation("Processing workflow execution {ExecutionId}", workflowExecutionId);

            // Get next pending step
            var pendingStep = execution.StepExecutions
                .Where(se => se.Status == StepExecutionStatus.Pending)
                .OrderBy(se => se.WorkflowStep.Order)
                .FirstOrDefault();

            if (pendingStep == null)
            {
                // All steps are completed, mark workflow as completed
                var allStepsCompleted = execution.StepExecutions.All(se => 
                    se.Status == StepExecutionStatus.Completed || 
                    se.Status == StepExecutionStatus.Skipped);

                if (allStepsCompleted)
                {
                    execution.Status = WorkflowExecutionStatus.Completed;
                    execution.CompletedTime = DateTime.UtcNow;
                    
                    // Aggregate output data from all completed steps
                    var outputData = new Dictionary<string, object>();
                    foreach (var stepExec in execution.StepExecutions.Where(se => se.Status == StepExecutionStatus.Completed))
                    {
                        if (!string.IsNullOrEmpty(stepExec.OutputData))
                        {
                            var stepOutput = JsonSerializer.Deserialize<object>(stepExec.OutputData);
                            outputData[stepExec.WorkflowStep.StepDefinition.Name] = stepOutput;
                        }
                    }
                    
                    execution.OutputData = JsonSerializer.Serialize(outputData);
                    
                    await context.SaveChangesAsync(cancellationToken);
                    
                    // Notify completion
                    if (notificationService != null)
                    {
                        await notificationService.NotifyExecutionCompleted(execution);
                        await notificationService.NotifyWorkflowExecutionStatusChanged(execution);
                    }
                    
                    logger.LogInformation("Workflow execution {ExecutionId} completed successfully", workflowExecutionId);
                }
                else
                {
                    // Check if any step failed and workflow should fail
                    var failedSteps = execution.StepExecutions.Where(se => se.Status == StepExecutionStatus.Failed).ToList();
                    if (failedSteps.Any())
                    {
                        execution.Status = WorkflowExecutionStatus.Failed;
                        execution.CompletedTime = DateTime.UtcNow;
                        execution.ErrorMessage = $"Step(s) failed: {string.Join(", ", failedSteps.Select(s => s.WorkflowStep.StepDefinition.Name))}";
                        
                        await context.SaveChangesAsync(cancellationToken);
                        
                        logger.LogError("Workflow execution {ExecutionId} failed: {ErrorMessage}", 
                            workflowExecutionId, execution.ErrorMessage);
                    }
                }
                return;
            }

            // Check if step should be delayed
            if (pendingStep.ScheduledTime.HasValue && pendingStep.ScheduledTime.Value > DateTime.UtcNow)
            {
                // Reschedule the step for later
                backgroundJobClient.Schedule(
                    () => ProcessStepAsync(pendingStep.Id, CancellationToken.None),
                    pendingStep.ScheduledTime.Value);
                
                logger.LogInformation("Step {StepId} scheduled for {ScheduledTime}", 
                    pendingStep.Id, pendingStep.ScheduledTime.Value);
                return;
            }

            // Process the current step
            await ProcessStepAsync(pendingStep.Id, cancellationToken);

            // Continue processing next steps if available
            backgroundJobClient.Enqueue(
                () => ProcessWorkflowAsync(workflowExecutionId, CancellationToken.None));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing workflow execution {ExecutionId}", workflowExecutionId);
            
            execution.Status = WorkflowExecutionStatus.Failed;
            execution.CompletedTime = DateTime.UtcNow;
            execution.ErrorMessage = ex.Message;
            
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task ProcessStepAsync(
        Guid stepExecutionId,
        CancellationToken cancellationToken = default)
    {
        var stepExecution = await context.StepExecutions
            .Include(se => se.WorkflowExecution)
            .Include(se => se.WorkflowStep)
            .ThenInclude(ws => ws.StepDefinition)
            .FirstOrDefaultAsync(se => se.Id == stepExecutionId, cancellationToken);

        if (stepExecution == null)
        {
            logger.LogError("Step execution {StepExecutionId} not found", stepExecutionId);
            return;
        }

        if (stepExecution.Status != StepExecutionStatus.Pending && stepExecution.Status != StepExecutionStatus.Retrying)
        {
            logger.LogInformation("Step execution {StepExecutionId} is not in pending/retrying state: {Status}",
                stepExecutionId, stepExecution.Status);
            return;
        }

        if (stepExecution.WorkflowExecution.Status == WorkflowExecutionStatus.Cancelled ||
            stepExecution.WorkflowExecution.Status == WorkflowExecutionStatus.Paused)
        {
            logger.LogInformation("Workflow is {Status}, skipping step execution {StepExecutionId}",
                stepExecution.WorkflowExecution.Status, stepExecutionId);
            return;
        }

        try
        {
            logger.LogInformation("Processing step execution {StepExecutionId} - {StepName}",
                stepExecutionId, stepExecution.WorkflowStep.StepDefinition.Name);

            // Update step status to running
            stepExecution.Status = StepExecutionStatus.Running;
            stepExecution.StartedTime = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);

            // Get the step implementation
            var stepType = GetStepTypeFromAssemblies(stepExecution.WorkflowStep.StepDefinition.StepType);
            if (stepType == null)
            {
                throw new InvalidOperationException($"Step type '{stepExecution.WorkflowStep.StepDefinition.StepType}' not found");
            }

            var stepInstance = serviceProvider.GetService(stepType) as IWorkflowStep;
            if (stepInstance == null)
            {
                throw new InvalidOperationException($"Could not create instance of step type '{stepType.Name}'");
            }

            // Build step context
            var stepContext = await BuildStepContextAsync(stepExecution, cancellationToken);
            stepContext.CancellationToken = cancellationToken;

            // Validate input first
            var validationResult = await stepInstance.ValidateInputAsync(stepContext, cancellationToken);
            if (!validationResult.IsSuccess)
            {
                stepExecution.Status = StepExecutionStatus.Failed;
                stepExecution.CompletedTime = DateTime.UtcNow;
                stepExecution.ErrorMessage = $"Input validation failed: {validationResult.ErrorMessage}";
                await context.SaveChangesAsync(cancellationToken);
                
                logger.LogError("Step execution {StepExecutionId} failed validation: {ErrorMessage}",
                    stepExecutionId, validationResult.ErrorMessage);
                return;
            }

            // Check if step can execute
            var canExecute = await stepInstance.CanExecuteAsync(stepContext, cancellationToken);
            if (!canExecute)
            {
                stepExecution.Status = StepExecutionStatus.Skipped;
                stepExecution.CompletedTime = DateTime.UtcNow;
                stepExecution.ErrorMessage = "Step conditions not met";
                await context.SaveChangesAsync(cancellationToken);
                
                logger.LogInformation("Step execution {StepExecutionId} skipped - conditions not met", stepExecutionId);
                return;
            }

            // Execute the step
            var result = await stepInstance.ExecuteAsync(stepContext, cancellationToken);

            if (result.IsSuccess)
            {
                stepExecution.Status = StepExecutionStatus.Completed;
                stepExecution.CompletedTime = DateTime.UtcNow;
                stepExecution.OutputData = result.OutputData?.RootElement.GetRawText();
                
                logger.LogInformation("Step execution {StepExecutionId} completed successfully", stepExecutionId);
            }
            else
            {
                // Handle failure and retry logic
                if (result.ShouldRetry && stepExecution.RetryCount < stepExecution.MaxRetries)
                {
                    stepExecution.Status = StepExecutionStatus.Retrying;
                    stepExecution.RetryCount++;
                    stepExecution.ErrorMessage = result.ErrorMessage;
                    
                    var retryDelay = result.RetryDelay ?? TimeSpan.FromMinutes(Math.Pow(2, stepExecution.RetryCount)); // Exponential backoff
                    stepExecution.ScheduledTime = DateTime.UtcNow.Add(retryDelay);
                    
                    await context.SaveChangesAsync(cancellationToken);
                    
                    // Schedule retry
                    backgroundJobClient.Schedule(
                        () => ProcessStepAsync(stepExecutionId, CancellationToken.None),
                        stepExecution.ScheduledTime.Value);
                    
                    logger.LogWarning("Step execution {StepExecutionId} failed, scheduled for retry {RetryCount}/{MaxRetries} at {ScheduledTime}: {ErrorMessage}",
                        stepExecutionId, stepExecution.RetryCount, stepExecution.MaxRetries, stepExecution.ScheduledTime, result.ErrorMessage);
                }
                else
                {
                    stepExecution.Status = StepExecutionStatus.Failed;
                    stepExecution.CompletedTime = DateTime.UtcNow;
                    stepExecution.ErrorMessage = result.ErrorMessage;
                    
                    logger.LogError("Step execution {StepExecutionId} failed permanently after {RetryCount} retries: {ErrorMessage}",
                        stepExecutionId, stepExecution.RetryCount, result.ErrorMessage);
                }
            }

            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing step execution {StepExecutionId}", stepExecutionId);
            
            stepExecution.Status = StepExecutionStatus.Failed;
            stepExecution.CompletedTime = DateTime.UtcNow;
            stepExecution.ErrorMessage = ex.Message;
            
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private Type? GetStepTypeFromAssemblies(string stepTypeName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetTypes().FirstOrDefault(t =>
                typeof(IWorkflowStep).IsAssignableFrom(t) &&
                (t.Name == stepTypeName || t.FullName?.EndsWith($".{stepTypeName}") == true));

            if (type != null)
                return type;
        }

        return null;
    }

    private async Task<StepContext> BuildStepContextAsync(StepExecution stepExecution, CancellationToken cancellationToken)
    {
        var workflowExecution = stepExecution.WorkflowExecution;
        var workflowStep = stepExecution.WorkflowStep;
        var stepDefinition = workflowStep.StepDefinition;
        
        // Get workflow data (input data + previous step outputs)
        var workflowData = new Dictionary<string, object>();
        
        // Add workflow input data
        if (!string.IsNullOrEmpty(workflowExecution.InputData))
        {
            var workflowInputData = JsonSerializer.Deserialize<Dictionary<string, object>>(workflowExecution.InputData);
            if (workflowInputData != null)
            {
                foreach (var kvp in workflowInputData)
                {
                    workflowData[kvp.Key] = kvp.Value;
                }
            }
        }
        
        // Add outputs from previous completed steps
        var previousSteps = await context.StepExecutions
            .Include(se => se.WorkflowStep)
            .ThenInclude(ws => ws.StepDefinition)
            .Where(se => se.WorkflowExecutionId == stepExecution.WorkflowExecutionId &&
                        se.WorkflowStep.Order < stepExecution.WorkflowStep.Order &&
                        se.Status == StepExecutionStatus.Completed &&
                        !string.IsNullOrEmpty(se.OutputData))
            .OrderBy(se => se.WorkflowStep.Order)
            .ToListAsync(cancellationToken);
        
        foreach (var prevStep in previousSteps)
        {
            var stepOutput = JsonSerializer.Deserialize<object>(prevStep.OutputData!);
            workflowData[$"step_{prevStep.WorkflowStep.StepDefinition.Name}"] = stepOutput;
        }
        
        // Prepare step configuration (merge step definition config with workflow step config)
        JsonDocument? configuration = null;
        
        if (!string.IsNullOrEmpty(stepDefinition.Configuration) || !string.IsNullOrEmpty(workflowStep.StepConfiguration))
        {
            var configDict = new Dictionary<string, object>();
            
            // Start with step definition configuration
            if (!string.IsNullOrEmpty(stepDefinition.Configuration))
            {
                var defConfig = JsonSerializer.Deserialize<Dictionary<string, object>>(stepDefinition.Configuration);
                if (defConfig != null)
                {
                    foreach (var kvp in defConfig)
                    {
                        configDict[kvp.Key] = kvp.Value;
                    }
                }
            }
            
            // Override with workflow step configuration
            if (!string.IsNullOrEmpty(workflowStep.StepConfiguration))
            {
                var stepConfig = JsonSerializer.Deserialize<Dictionary<string, object>>(workflowStep.StepConfiguration);
                if (stepConfig != null)
                {
                    foreach (var kvp in stepConfig)
                    {
                        configDict[kvp.Key] = kvp.Value;
                    }
                }
            }
            
            configuration = JsonSerializer.SerializeToDocument(configDict);
        }
        
        // Prepare input data for this step
        JsonDocument? inputData = null;
        if (!string.IsNullOrEmpty(stepExecution.InputData))
        {
            inputData = JsonDocument.Parse(stepExecution.InputData);
        }
        
        return new StepContext
        {
            WorkflowExecutionId = stepExecution.WorkflowExecutionId,
            StepExecutionId = stepExecution.Id,
            WorkflowDefinitionId = workflowExecution.WorkflowDefinitionId,
            StepDefinitionId = stepDefinition.Id,
            StepType = stepDefinition.StepType,
            StepName = stepDefinition.Name,
            Configuration = configuration,
            InputData = inputData,
            WorkflowData = workflowData,
            StepData = new Dictionary<string, object>(),
            RetryCount = stepExecution.RetryCount,
            MaxRetries = stepExecution.MaxRetries
        };
    }
}
