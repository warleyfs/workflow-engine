export interface WorkflowDefinition {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface StepDefinition {
  id: string;
  stepType: string;
  name: string;
  description?: string;
  configuration: Record<string, any>;
  order: number;
  workflowDefinitionId: string;
}

export interface WorkflowExecution {
  executionId: string;
  workflowDefinitionId: string;
  status: WorkflowExecutionStatus;
  startedAt?: Date;
  completedAt?: Date;
  inputData: Record<string, any>;
  outputData?: Record<string, any>;
  currentStep?: number;
  error?: string;
}

export interface StepExecution {
  id: string;
  workflowExecutionId: string;
  stepDefinitionId: string;
  status: StepExecutionStatus;
  startedAt?: Date;
  completedAt?: Date;
  inputData: Record<string, any>;
  outputData?: Record<string, any>;
  error?: string;
  retryCount: number;
}

export enum WorkflowExecutionStatus {
  Pending = 'Pending',
  Running = 'Running',
  Completed = 'Completed',
  Failed = 'Failed',
  Cancelled = 'Cancelled',
  Paused = 'Paused'
}

export enum StepExecutionStatus {
  Pending = 'Pending',
  Running = 'Running',
  Completed = 'Completed',
  Failed = 'Failed',
  Skipped = 'Skipped',
  Retrying = 'Retrying'
}

export interface WorkflowNode {
  id: string;
  type: string;
  position: { x: number; y: number };
  data: {
    label: string;
    stepType: string;
    configuration: Record<string, any>;
    status?: StepExecutionStatus;
  };
}

export interface WorkflowEdge {
  id: string;
  source: string;
  target: string;
  type?: string;
}

export interface StepType {
  type: string;
  name: string;
  description: string;
  icon: string;
  configurationSchema: Record<string, any>;
}

export interface CreateWorkflowRequest {
  name: string;
  description?: string;
  steps: Omit<StepDefinition, 'id' | 'workflowDefinitionId'>[];
}

export interface ExecuteWorkflowRequest {
  inputData: Record<string, any>;
  scheduledTime?: Date;
}

