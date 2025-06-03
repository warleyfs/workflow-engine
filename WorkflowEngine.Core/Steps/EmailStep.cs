using System.Text.Json;
using Microsoft.Extensions.Logging;
using WorkflowEngine.Core.Interfaces;
using WorkflowEngine.Core.Models;

namespace WorkflowEngine.Core.Steps;

public class EmailStep : IWorkflowStep
{
    private readonly ILogger<EmailStep> _logger;

    public EmailStep(ILogger<EmailStep> logger)
    {
        _logger = logger;
    }

    public string StepType => "EmailStep";

    public async Task<StepResult> ExecuteAsync(StepContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var config = ExtractConfiguration(context.Configuration);
            
            _logger.LogInformation("Sending email to {To} with subject '{Subject}'", 
                config.To, config.Subject);

            // Simulate email sending delay
            await Task.Delay(1000, cancellationToken);

            var result = new
            {
                EmailSent = true,
                To = config.To,
                Subject = config.Subject,
                SentAt = DateTime.UtcNow
            };

            _logger.LogInformation("Email sent successfully to {To}", config.To);
            
            return StepResult.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email");
            return StepResult.Failure(ex.Message, shouldRetry: true, retryDelay: TimeSpan.FromMinutes(2));
        }
    }

    public async Task<bool> CanExecuteAsync(StepContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var config = ExtractConfiguration(context.Configuration);
            return !string.IsNullOrEmpty(config.To) && !string.IsNullOrEmpty(config.Subject);
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

    private EmailConfiguration ExtractConfiguration(JsonDocument? configJson)
    {
        if (configJson == null)
            throw new ArgumentException("Email configuration is required");

        var config = JsonSerializer.Deserialize<EmailConfiguration>(configJson.RootElement.GetRawText());
        
        if (config == null)
            throw new ArgumentException("Invalid email configuration");

        return config;
    }

    public class EmailConfiguration
    {
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? Cc { get; set; }
        public string? Bcc { get; set; }
    }
}

