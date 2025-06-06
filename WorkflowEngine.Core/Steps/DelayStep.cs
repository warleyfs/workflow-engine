using System.Text.Json;
using Microsoft.Extensions.Logging;
using WorkflowEngine.Core.Interfaces;
using WorkflowEngine.Core.Models;

namespace WorkflowEngine.Core.Steps;

public class DelayStep(ILogger<DelayStep> logger) : IWorkflowStep
{
    public string StepType => "DelayStep";

    public async Task<StepResult> ExecuteAsync(StepContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var config = ExtractConfiguration(context.Configuration);
            
            logger.LogInformation("Delaying for {DelaySeconds} seconds", config.DelaySeconds);

            await Task.Delay(TimeSpan.FromSeconds(config.DelaySeconds), cancellationToken);

            var result = new
            {
                DelayCompleted = true,
                DelaySeconds = config.DelaySeconds,
                CompletedAt = DateTime.UtcNow
            };

            logger.LogInformation("Delay of {DelaySeconds} seconds completed", config.DelaySeconds);
            
            return StepResult.Success(result);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Delay step was cancelled");
            return StepResult.Failure("Delay was cancelled");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute delay step");
            return StepResult.Failure(ex.Message);
        }
    }

    public async Task<bool> CanExecuteAsync(StepContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var config = ExtractConfiguration(context.Configuration);
            return config.DelaySeconds > 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<StepResult> ValidateInputAsync(StepContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var config = ExtractConfiguration(context.Configuration);
            if (config.DelaySeconds <= 0)
                return StepResult.Failure("DelaySeconds must be greater than 0");
            
            return StepResult.Success();
        }
        catch (Exception ex)
        {
            return StepResult.Failure($"Invalid configuration: {ex.Message}");
        }
    }

    private DelayConfiguration ExtractConfiguration(JsonDocument? configJson)
    {
        if (configJson == null)
            throw new ArgumentException("Delay configuration is required");

        var config = JsonSerializer.Deserialize<DelayConfiguration>(configJson.RootElement.GetRawText());
        
        if (config == null)
            throw new ArgumentException("Invalid delay configuration");

        return config;
    }

    public class DelayConfiguration
    {
        public int DelaySeconds { get; set; } = 5;
    }
}

