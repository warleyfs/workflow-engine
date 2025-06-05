import React, { useState, useEffect } from 'react';
import {
  Box,
  Grid,
  Card,
  CardContent,
  Typography,
  CircularProgress,
  Alert,
  Chip,
  LinearProgress,
} from '@mui/material';
import {
  TrendingUp as TrendingUpIcon,
  PlayArrow as PlayArrowIcon,
  CheckCircle as CheckCircleIcon,
  Error as ErrorIcon,
  Schedule as ScheduleIcon,
  Speed as SpeedIcon,
} from '@mui/icons-material';
import {
  PieChart,
  Pie,
  Cell,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  LineChart,
  Line,
} from 'recharts';
import { DashboardModel } from '../types/monitoring';
import { monitoringApi } from '../services/api';
import signalRService from '../services/signalr';

const COLORS = {
  Pending: '#ff9800',
  Running: '#2196f3',
  Completed: '#4caf50',
  Failed: '#f44336',
  Cancelled: '#9e9e9e',
  Paused: '#ff5722',
};

const Dashboard: React.FC = () => {
  const [dashboard, setDashboard] = useState<DashboardModel | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isConnected, setIsConnected] = useState(false);

  useEffect(() => {
    loadDashboard();
    setupSignalR();

    return () => {
      signalRService.off('dashboardUpdated');
      signalRService.off('connected');
      signalRService.off('disconnected');
    };
  }, []);

  const loadDashboard = async () => {
    try {
      setLoading(true);
      const data = await monitoringApi.getDashboard();
      setDashboard(data);
      setError(null);
    } catch (err) {
      setError('Failed to load dashboard data');
      console.error('Dashboard error:', err);
    } finally {
      setLoading(false);
    }
  };

  const setupSignalR = async () => {
    try {
      if (!signalRService.isConnected) {
        await signalRService.start();
      }
      await signalRService.subscribeToDashboard();
      setIsConnected(true);

      // Listen for real-time updates
      signalRService.on('dashboardUpdated', () => {
        loadDashboard();
      });

      signalRService.on('newExecution', () => {
        loadDashboard();
      });

      signalRService.on('executionCompleted', () => {
        loadDashboard();
      });

      signalRService.on('connected', () => {
        setIsConnected(true);
      });

      signalRService.on('disconnected', () => {
        setIsConnected(false);
      });
    } catch (error) {
      console.error('SignalR setup error:', error);
      setIsConnected(false);
    }
  };

  const formatDuration = (minutes: number): string => {
    if (minutes < 1) return `${Math.round(minutes * 60)}s`;
    if (minutes < 60) return `${Math.round(minutes)}m`;
    return `${Math.round(minutes / 60)}h`;
  };

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'Completed': return <CheckCircleIcon color="success" />;
      case 'Failed': return <ErrorIcon color="error" />;
      case 'Running': return <PlayArrowIcon color="primary" />;
      case 'Pending': return <ScheduleIcon color="warning" />;
      default: return <ScheduleIcon />;
    }
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
        <CircularProgress />
      </Box>
    );
  }

  if (error || !dashboard) {
    return (
      <Alert severity="error" action={
        <button onClick={loadDashboard}>Retry</button>
      }>
        {error || 'No data available'}
      </Alert>
    );
  }

  return (
    <Box sx={{ p: 3 }}>
      {/* Header */}
      <Box sx={{ mb: 3, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Typography variant="h4" component="h1">
          Workflow Dashboard
        </Typography>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Chip
            icon={isConnected ? <CheckCircleIcon /> : <ErrorIcon />}
            label={isConnected ? 'Live Updates' : 'Disconnected'}
            color={isConnected ? 'success' : 'error'}
            variant="outlined"
          />
        </Box>
      </Box>

      <Grid container spacing={3}>
        {/* Key Metrics */}
        <Grid size={{ xs: 12, md: 3 }}>
          <Card>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Total Workflows
              </Typography>
              <Typography variant="h4">
                {dashboard.totalWorkflows}
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 3 }}>
          <Card>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Total Executions
              </Typography>
              <Typography variant="h4">
                {dashboard.totalExecutions}
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
                {dashboard.performance.successRate.toFixed(1)}%
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
                {dashboard.performance.activeExecutions}
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        {/* Performance Metrics */}
        <Grid size={{ xs: 12, md: 6 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Performance Metrics
              </Typography>
              <Box sx={{ mt: 2 }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                  <Typography variant="body2">Avg Execution Time</Typography>
                  <Typography variant="body2" fontWeight="bold">
                    {formatDuration(dashboard.performance.averageExecutionTimeMinutes)}
                  </Typography>
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                  <Typography variant="body2">Last 24h</Typography>
                  <Typography variant="body2" fontWeight="bold">
                    {dashboard.performance.executionsLast24Hours} executions
                  </Typography>
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                  <Typography variant="body2">Last 7 days</Typography>
                  <Typography variant="body2" fontWeight="bold">
                    {dashboard.performance.executionsLast7Days} executions
                  </Typography>
                </Box>
                <Box sx={{ mt: 2 }}>
                  <Typography variant="body2" sx={{ mb: 1 }}>Success Rate</Typography>
                  <LinearProgress
                    variant="determinate"
                    value={dashboard.performance.successRate}
                    sx={{ height: 8, borderRadius: 4 }}
                  />
                </Box>
              </Box>
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
                    data={dashboard.executionsByStatus}
                    cx="50%"
                    cy="50%"
                    outerRadius={80}
                    fill="#8884d8"
                    dataKey="count"
                    label={({ status, percentage }) => `${status} (${percentage}%)`}
                  >
                    {dashboard.executionsByStatus.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={COLORS[entry.status as keyof typeof COLORS] || '#8884d8'} />
                    ))}
                  </Pie>
                  <Tooltip />
                </PieChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>
        </Grid>

        {/* Recent Executions */}
        <Grid size={12}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Recent Executions
              </Typography>
              <Box sx={{ mt: 2 }}>
                {dashboard.recentExecutions.map((execution) => (
                  <Box
                    key={execution.id}
                    sx={{
                      display: 'flex',
                      justifyContent: 'space-between',
                      alignItems: 'center',
                      py: 1,
                      borderBottom: '1px solid #eee',
                      '&:last-child': { borderBottom: 'none' },
                    }}
                  >
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                      {getStatusIcon(execution.status)}
                      <Box>
                        <Typography variant="body2" fontWeight="bold">
                          {execution.workflowName}
                        </Typography>
                        <Typography variant="caption" color="textSecondary">
                          {new Date(execution.createdAt).toLocaleString()}
                        </Typography>
                      </Box>
                    </Box>
                    <Box sx={{ textAlign: 'right' }}>
                      <Chip
                        label={execution.status}
                        size="small"
                        color={
                          execution.status === 'Completed' ? 'success' :
                          execution.status === 'Failed' ? 'error' :
                          execution.status === 'Running' ? 'primary' : 'default'
                        }
                      />
                      {execution.duration && (
                        <Typography variant="caption" color="textSecondary" sx={{ ml: 1 }}>
                          {formatDuration(execution.duration / 60000)}
                        </Typography>
                      )}
                    </Box>
                  </Box>
                ))}
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
};

export default Dashboard;

