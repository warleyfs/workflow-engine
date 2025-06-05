import axios from 'axios';
import {
  WorkflowDefinition,
  WorkflowExecution,
  StepExecution,
  CreateWorkflowRequest,
  ExecuteWorkflowRequest
} from '../types/workflow';
import {
  DashboardModel,
  ExecutionListResponse,
  WorkflowMetrics,
  StepStatistics,
  ExecutionLogs,
  ExecutionFilters
} from '../types/monitoring';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

export const workflowApi = {
  // Workflow Definitions
  getWorkflows: async (): Promise<WorkflowDefinition[]> => {
    const response = await api.get('/workflow/definitions');
    return response.data;
  },

  createWorkflow: async (workflow: CreateWorkflowRequest): Promise<WorkflowDefinition> => {
    const response = await api.post('/workflow/definitions', workflow);
    return response.data;
  },

  getWorkflow: async (id: string): Promise<WorkflowDefinition> => {
    const response = await api.get(`/workflow/definitions/${id}`);
    return response.data;
  },

  updateWorkflow: async (id: string, workflow: Partial<CreateWorkflowRequest>): Promise<WorkflowDefinition> => {
    const response = await api.put(`/workflow/definitions/${id}`, workflow);
    return response.data;
  },

  deleteWorkflow: async (id: string): Promise<void> => {
    await api.delete(`/workflow/definitions/${id}`);
  },

  // Workflow Executions
  executeWorkflow: async (id: string, request: ExecuteWorkflowRequest): Promise<WorkflowExecution> => {
    const response = await api.post(`/workflow/${id}/execute`, request);
    return response.data;
  },

  getExecution: async (id: string): Promise<WorkflowExecution> => {
    const response = await api.get(`/workflow/execution/${id}`);
    return response.data;
  },

  getExecutions: async (): Promise<WorkflowExecution[]> => {
    const response = await api.get('/workflow/executions');
    return response.data;
  },

  pauseExecution: async (id: string): Promise<void> => {
    await api.post(`/workflow/execution/${id}/pause`);
  },

  resumeExecution: async (id: string): Promise<void> => {
    await api.post(`/workflow/execution/${id}/resume`);
  },

  cancelExecution: async (id: string): Promise<void> => {
    await api.post(`/workflow/execution/${id}/cancel`);
  },

  // Step Executions
  getStepExecutions: async (executionId: string): Promise<StepExecution[]> => {
    const response = await api.get(`/workflow/execution/${executionId}/steps`);
    return response.data;
  },

  // Demo endpoint
  createDemoWorkflow: async (): Promise<WorkflowDefinition> => {
    const response = await api.post('/workflow/demo');
    return response.data;
  },
};

// Monitoring API
export const monitoringApi = {
  // Dashboard
  getDashboard: async (): Promise<DashboardModel> => {
    const response = await api.get('/monitoring/dashboard');
    return response.data;
  },

  // Executions
  getExecutions: async (filters?: ExecutionFilters): Promise<ExecutionListResponse> => {
    const params = new URLSearchParams();
    
    if (filters?.status) params.append('status', filters.status);
    if (filters?.workflowDefinitionId) params.append('workflowDefinitionId', filters.workflowDefinitionId);
    if (filters?.startDate) params.append('startDate', filters.startDate.toISOString());
    if (filters?.endDate) params.append('endDate', filters.endDate.toISOString());
    if (filters?.page) params.append('page', filters.page.toString());
    if (filters?.pageSize) params.append('pageSize', filters.pageSize.toString());
    
    const response = await api.get(`/monitoring/executions?${params.toString()}`);
    return response.data;
  },

  // Workflow Metrics
  getWorkflowMetrics: async (workflowId: string): Promise<WorkflowMetrics> => {
    const response = await api.get(`/monitoring/workflow/${workflowId}/metrics`);
    return response.data;
  },

  // Step Statistics
  getStepStatistics: async (): Promise<StepStatistics[]> => {
    const response = await api.get('/monitoring/steps/statistics');
    return response.data;
  },

  // Execution Logs
  getExecutionLogs: async (executionId: string): Promise<ExecutionLogs> => {
    const response = await api.get(`/monitoring/execution/${executionId}/logs`);
    return response.data;
  },
};

export default api;

