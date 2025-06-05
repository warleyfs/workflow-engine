import React, { useState, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Grid,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  CircularProgress,
  Alert,
  Chip,
} from '@mui/material';
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
  LineChart,
  Line,
} from 'recharts';
import { WorkflowMetrics as WorkflowMetricsType } from '../types/monitoring';
import { monitoringApi, workflowApi } from '../services/api';

const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884D8'];

const WorkflowMetrics: React.FC = () => {
  const [workflows, setWorkflows] = useState<any[]>([]);
  const [selectedWorkflowId, setSelectedWorkflowId] = useState<string>('');
  const [metrics, setMetrics] = useState<WorkflowMetricsType | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadWorkflows();
  }, []);

  useEffect(() => {
    if (selectedWorkflowId) {
      loadMetrics(selectedWorkflowId);
    }
  }, [selectedWorkflowId]);

  const loadWorkflows = async () => {
    try {
      const data = await workflowApi.getWorkflows();
      setWorkflows(data);
      if (data.length > 0) {
        setSelectedWorkflowId(data[0].id);
      }
    } catch (err) {
      setError('Failed to load workflows');
      console.error('Failed to load workflows:', err);
    }
  };

  const loadMetrics = async (workflowId: string) => {
    try {
      setLoading(true);
      const data = await monitoringApi.getWorkflowMetrics(workflowId);
      setMetrics(data);
      setError(null);
    } catch (err) {
      setError('Failed to load workflow metrics');
      console.error('Failed to load workflow metrics:', err);
    } finally {
      setLoading(false);
    }
  };

  const formatDuration = (minutes: number): string => {
    if (minutes < 1) return `${Math.round(minutes * 60)}s`;
    if (minutes < 60) return `${Math.round(minutes)}m`;
    return `${Math.round(minutes / 60)}h`;
  };

  if (error) {
    return (
      <Alert severity="error">
        {error}
      </Alert>
    );
  }

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" component="h1" sx={{ mb: 3 }}>
        Workflow Metrics
      </Typography>

      <FormControl sx={{ mb: 3, minWidth: 300 }}>
        <InputLabel>Select Workflow</InputLabel>
        <Select
          value={selectedWorkflowId}
          onChange={(e) => setSelectedWorkflowId(e.target.value)}
          label="Select Workflow"
        >
          {workflows.map((workflow) => (
            <MenuItem key={workflow.id} value={workflow.id}>
              {workflow.name}
            </MenuItem>
          ))}
        </Select>
      </FormControl>

      {loading ? (
        <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
          <CircularProgress />
        </Box>
      ) : metrics ? (
        <Grid container spacing={3}>
          {/* Key Metrics Cards */}
          <Grid size={{ xs: 12, md: 3 }}>
            <Card>
              <CardContent>
                <Typography color="textSecondary" gutterBottom>
                  Total Executions
                </Typography>
                <Typography variant="h4">
                  {metrics.totalExecutions}
                </Typography>
              </CardContent>
            </Card>
          </Grid>

          <Grid size={{ xs: 12, md: 3 }}>
            <Card>
              <CardContent>
                <Typography color="textSecondary" gutterBottom>
                  Success Rate
                </Typography>
                <Typography variant="h4" color="success.main">
                  {metrics.successRate.toFixed(1)}%
                </Typography>
              </CardContent>
            </Card>
          </Grid>

          <Grid size={{ xs: 12, md: 3 }}>
            <Card>
              <CardContent>
                <Typography color="textSecondary" gutterBottom>
                  Active Executions
                </Typography>
                <Typography variant="h4" color="primary">
                  {metrics.activeExecutions}
                </Typography>
              </CardContent>
            </Card>
          </Grid>

          <Grid size={{ xs: 12, md: 3 }}>
            <Card>
              <CardContent>
                <Typography color="textSecondary" gutterBottom>
                  Avg Duration
                </Typography>
                <Typography variant="h4">
                  {formatDuration(metrics.averageExecutionTimeMinutes)}
                </Typography>
              </CardContent>
            </Card>
          </Grid>

          {/* Status Distribution */}
          <Grid size={{ xs: 12, md: 6 }}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Execution Status Distribution
                </Typography>
                <ResponsiveContainer width="100%" height={300}>
                  <PieChart>
                    <Pie
                      data={metrics.executionsByStatus}
                      cx="50%"
                      cy="50%"
                      outerRadius={80}
                      fill="#8884d8"
                      dataKey="count"
                      label={({ status, count }) => `${status}: ${count}`}
                    >
                      {metrics.executionsByStatus.map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                      ))}
                    </Pie>
                    <Tooltip />
                  </PieChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>
          </Grid>

          {/* Detailed Statistics */}
          <Grid size={{ xs: 12, md: 6 }}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Detailed Statistics
                </Typography>
                <Box sx={{ mt: 2 }}>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
                    <Typography variant="body2">Completed Executions</Typography>
                    <Chip 
                      label={metrics.completedExecutions} 
                      color="success" 
                      size="small" 
                    />
                  </Box>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
                    <Typography variant="body2">Failed Executions</Typography>
                    <Chip 
                      label={metrics.failedExecutions} 
                      color="error" 
                      size="small" 
                    />
                  </Box>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
                    <Typography variant="body2">Active Executions</Typography>
                    <Chip 
                      label={metrics.activeExecutions} 
                      color="primary" 
                      size="small" 
                    />
                  </Box>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
                    <Typography variant="body2">Failure Rate</Typography>
                    <Typography variant="body2" color="error.main">
                      {metrics.failureRate.toFixed(1)}%
                    </Typography>
                  </Box>
                  {metrics.lastExecution && (
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
                      <Typography variant="body2">Last Execution</Typography>
                      <Typography variant="body2">
                        {new Date(metrics.lastExecution).toLocaleDateString()}
                      </Typography>
                    </Box>
                  )}
                </Box>
              </CardContent>
            </Card>
          </Grid>

          {/* Bar Chart for Status Counts */}
          <Grid size={12}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Executions by Status
                </Typography>
                <ResponsiveContainer width="100%" height={300}>
                  <BarChart data={metrics.executionsByStatus}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="status" />
                    <YAxis />
                    <Tooltip />
                    <Bar dataKey="count" fill="#8884d8" />
                  </BarChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      ) : (
        <Typography variant="body1" color="textSecondary">
          Select a workflow to view its metrics.
        </Typography>
      )}
    </Box>
  );
};

export default WorkflowMetrics;

