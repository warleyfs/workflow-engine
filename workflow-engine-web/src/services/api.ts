import axios from 'axios';
import {
  WorkflowDefinition,
  WorkflowExecution,
  StepExecution,
  CreateWorkflowRequest,
  ExecuteWorkflowRequest
} from '../types/workflow';

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

export default api;

