export interface DashboardModel {
  totalWorkflows: number;
  totalExecutions: number;
  executionsByStatus: ExecutionStatusCount[];
  recentExecutions: RecentExecutionSummary[];
  performance: WorkflowPerformanceMetrics;
}

export interface ExecutionStatusCount {
  status: string;
  count: number;
  percentage: number;
}

export interface RecentExecutionSummary {
  id: string;
  workflowName: string;
  status: string;
  createdAt: Date;
  completedTime?: Date;
  duration?: number; // in milliseconds
}

export interface WorkflowPerformanceMetrics {
  averageExecutionTimeMinutes: number;
  executionsLast24Hours: number;
  executionsLast7Days: number;
  successRate: number;
  failureRate: number;
  activeExecutions: number;
}

export interface ExecutionListResponse {
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  executions: ExecutionSummary[];
}

export interface ExecutionSummary {
  id: string;
  workflowDefinitionId: string;
  workflowName: string;
  status: string;
  createdAt: Date;
  startedTime?: Date;
  completedTime?: Date;
  duration?: number;
  errorMessage?: string;
  totalSteps: number;
  completedSteps: number;
  failedSteps: number;
}

export interface WorkflowMetrics {
  workflowId: string;
  workflowName: string;
  totalExecutions: number;
  completedExecutions: number;
  failedExecutions: number;
  activeExecutions: number;
  successRate: number;
  failureRate: number;
  averageExecutionTimeMinutes: number;
  lastExecution?: Date;
  executionsByStatus: { status: string; count: number }[];
}

export interface StepStatistics {
  stepType: string;
  totalExecutions: number;
  completedExecutions: number;
  failedExecutions: number;
  averageRetries: number;
}

export interface ExecutionLogs {
  executionId: string;
  workflowName: string;
  status: string;
  createdAt: Date;
  startedTime?: Date;
  completedTime?: Date;
  errorMessage?: string;
  steps: StepLog[];
}

export interface StepLog {
  stepId: string;
  stepName: string;
  stepType: string;
  order: number;
  status: string;
  createdAt: Date;
  startedTime?: Date;
  completedTime?: Date;
  retryCount: number;
  errorMessage?: string;
  outputData?: any;
}

// SignalR Event Types
export interface WorkflowExecutionStatusUpdate {
  executionId: string;
  workflowDefinitionId: string;
  status: string;
  startedTime?: Date;
  completedTime?: Date;
  errorMessage?: string;
  updatedAt: Date;
}

export interface StepExecutionStatusUpdate {
  stepExecutionId: string;
  workflowExecutionId: string;
  status: string;
  startedTime?: Date;
  completedTime?: Date;
  retryCount: number;
  errorMessage?: string;
  updatedAt: Date;
}

export interface NewExecutionNotification {
  executionId: string;
  workflowDefinitionId: string;
  workflowName: string;
  status: string;
  createdAt: Date;
  scheduledTime?: Date;
}

export interface ExecutionCompletedNotification {
  executionId: string;
  workflowDefinitionId: string;
  workflowName: string;
  status: string;
  completedTime?: Date;
  duration?: number;
  errorMessage?: string;
}

export interface DashboardUpdateNotification {
  updatedAt: Date;
  message: string;
}

// Query Filters
export interface ExecutionFilters {
  status?: string;
  workflowDefinitionId?: string;
  startDate?: Date;
  endDate?: Date;
  page?: number;
  pageSize?: number;
}

