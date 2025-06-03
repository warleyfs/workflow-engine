using System.Text.Json;
using Microsoft.Extensions.Logging;
using WorkflowEngine.Core.Interfaces;
using WorkflowEngine.Core.Models;

namespace WorkflowEngine.Core.Steps;

public class LogStep : IWorkflowStep
{
    private readonly ILogger<LogStep> _logger;

    public LogStep(ILogger<LogStep> logger)
    {
        _logger = logger;
    }

    public string StepType => "LogStep";

    public async Task<StepResult> ExecuteAsync(StepContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var config = ExtractConfiguration(context.Configuration);
            
            switch (config.Level.ToLower())
            {
                case "information":
                case "info":
                    _logger.LogInformation(config.Message);
                    break;
                case "warning":
                case "warn":
                    _logger.LogWarning(config.Message);
                    break;
                case "error":
                    _logger.LogError(config.Message);
                    break;
                case "debug":
                    _logger.LogDebug(config.Message);
                    break;
                default:
                    _logger.LogInformation(config.Message);
                    break;
            }

            var result = new
            {
                Logged = true,
                Level = config.Level,
                Message = config.Message,
                LoggedAt = DateTime.UtcNow
            };

            return StepResult.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute log step");
            return StepResult.Failure(ex.Message);
        }
    }

    public async Task<bool> CanExecuteAsync(StepContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var config = ExtractConfiguration(context.Configuration);
            return !string.IsNullOrEmpty(config.Message);
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
            ExtractConfiguration(context.Configuration);
            return StepResult.Success();
        }
        catch (Exception ex)
        {
            return StepResult.Failure($"Invalid configuration: {ex.Message}");
        }
    }

    private LogConfiguration ExtractConfiguration(JsonDocument? configJson)
    {
        if (configJson == null)
            throw new ArgumentException("Log configuration is required");

        var config = JsonSerializer.Deserialize<LogConfiguration>(configJson.RootElement.GetRawText());
        
        if (config == null)
            throw new ArgumentException("Invalid log configuration");

        return config;
    }

    public class LogConfiguration
    {
        public string Message { get; set; } = string.Empty;
        public string Level { get; set; } = "Information";
    }
}

