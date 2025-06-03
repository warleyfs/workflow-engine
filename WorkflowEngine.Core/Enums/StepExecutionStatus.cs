namespace WorkflowEngine.Core.Enums;

public enum StepExecutionStatus
{
    Pending = 0,
    Running = 1,
    Completed = 2,
    Failed = 3,
    Skipped = 4,
    Retrying = 5
}

