import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TablePagination,
  Chip,
  IconButton,
  TextField,
  MenuItem,
  Grid,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  CircularProgress,
  Alert,
  Tooltip,
} from '@mui/material';
import {
  Visibility as VisibilityIcon,
  Refresh as RefreshIcon,
  PlayArrow as PlayArrowIcon,
  Pause as PauseIcon,
  Stop as StopIcon,
  FilterList as FilterListIcon,
} from '@mui/icons-material';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import {
  ExecutionListResponse,
  ExecutionSummary,
  ExecutionFilters,
  ExecutionLogs,
} from '../types/monitoring';
import { monitoringApi } from '../services/api';
import { workflowApi } from '../services/api';
import signalRService from '../services/signalr';

const statusColors: Record<string, 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning'> = {
  Pending: 'warning',
  Running: 'primary',
  Completed: 'success',
  Failed: 'error',
  Cancelled: 'default',
  Paused: 'secondary',
};

const ExecutionsList: React.FC = () => {
  const [executions, setExecutions] = useState<ExecutionListResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filters, setFilters] = useState<ExecutionFilters>({
    page: 1,
    pageSize: 20,
  });
  const [selectedExecution, setSelectedExecution] = useState<ExecutionLogs | null>(null);
  const [detailsDialogOpen, setDetailsDialogOpen] = useState(false);
  const [workflows, setWorkflows] = useState<any[]>([]);

  const loadExecutions = useCallback(async () => {
    try {
      setLoading(true);
      const data = await monitoringApi.getExecutions(filters);
      setExecutions(data);
      setError(null);
    } catch (err) {
      setError('Failed to load executions');
      console.error('Executions error:', err);
    } finally {
      setLoading(false);
    }
  }, [filters]);

  const loadWorkflows = async () => {
    try {
      const data = await workflowApi.getWorkflows();
      setWorkflows(data);
    } catch (err) {
      console.error('Failed to load workflows:', err);
    }
  };

  useEffect(() => {
    loadExecutions();
    loadWorkflows();

    // Setup SignalR for real-time updates
    const handleExecutionUpdate = () => {
      loadExecutions();
    };

    signalRService.on('workflowStatusChanged', handleExecutionUpdate);
    signalRService.on('newExecution', handleExecutionUpdate);
    signalRService.on('executionCompleted', handleExecutionUpdate);

    return () => {
      signalRService.off('workflowStatusChanged', handleExecutionUpdate);
      signalRService.off('newExecution', handleExecutionUpdate);
      signalRService.off('executionCompleted', handleExecutionUpdate);
    };
  }, [loadExecutions]);

  const handlePageChange = (event: unknown, newPage: number) => {
    setFilters({ ...filters, page: newPage + 1 });
  };

  const handleRowsPerPageChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setFilters({ ...filters, pageSize: parseInt(event.target.value, 10), page: 1 });
  };

  const handleFilterChange = (field: keyof ExecutionFilters, value: any) => {
    setFilters({ ...filters, [field]: value, page: 1 });
  };

  const clearFilters = () => {
    setFilters({ page: 1, pageSize: 20 });
  };

  const viewDetails = async (executionId: string) => {
    try {
      const logs = await monitoringApi.getExecutionLogs(executionId);
      setSelectedExecution(logs);
      setDetailsDialogOpen(true);
    } catch (err) {
      console.error('Failed to load execution details:', err);
    }
  };

  const controlExecution = async (executionId: string, action: 'pause' | 'resume' | 'cancel') => {
    try {
      switch (action) {
        case 'pause':
          await workflowApi.pauseExecution(executionId);
          break;
        case 'resume':
          await workflowApi.resumeExecution(executionId);
          break;
        case 'cancel':
          await workflowApi.cancelExecution(executionId);
          break;
      }
      loadExecutions();
    } catch (err) {
      console.error(`Failed to ${action} execution:`, err);
    }
  };

  const formatDuration = (duration?: number): string => {
    if (!duration) return '-';
    const minutes = Math.floor(duration / 60000);
    const seconds = Math.floor((duration % 60000) / 1000);
    return `${minutes}m ${seconds}s`;
  };

  const renderActionButtons = (execution: ExecutionSummary) => {
    return (
      <Box sx={{ display: 'flex', gap: 1 }}>
        <Tooltip title="View Details">
          <IconButton size="small" onClick={() => viewDetails(execution.id)}>
            <VisibilityIcon />
          </IconButton>
        </Tooltip>
        
        {execution.status === 'Running' && (
          <>
            <Tooltip title="Pause">
              <IconButton 
                size="small" 
                onClick={() => controlExecution(execution.id, 'pause')}
              >
                <PauseIcon />
              </IconButton>
            </Tooltip>
            <Tooltip title="Cancel">
              <IconButton 
                size="small" 
                onClick={() => controlExecution(execution.id, 'cancel')}
              >
                <StopIcon />
              </IconButton>
            </Tooltip>
          </>
        )}
        
        {execution.status === 'Paused' && (
          <Tooltip title="Resume">
            <IconButton 
              size="small" 
              onClick={() => controlExecution(execution.id, 'resume')}
            >
              <PlayArrowIcon />
            </IconButton>
          </Tooltip>
        )}
      </Box>
    );
  };

  if (error) {
    return (
      <Alert severity="error" action={
        <Button onClick={loadExecutions}>Retry</Button>
      }>
        {error}
      </Alert>
    );
  }

  return (
    <LocalizationProvider dateAdapter={AdapterDateFns}>
      <Box sx={{ p: 3 }}>
        <Typography variant="h4" component="h1" sx={{ mb: 3 }}>
          Workflow Executions
        </Typography>

        {/* Filters */}
        <Card sx={{ mb: 3 }}>
          <CardContent>
            <Grid container spacing={2} alignItems="center">
              <Grid size={{ xs: 12, sm: 3 }}>
                <TextField
                  select
                  label="Status"
                  value={filters.status || ''}
                  onChange={(e) => handleFilterChange('status', e.target.value || undefined)}
                  fullWidth
                  size="small"
                >
                  <MenuItem value="">All</MenuItem>
                  <MenuItem value="Pending">Pending</MenuItem>
                  <MenuItem value="Running">Running</MenuItem>
                  <MenuItem value="Completed">Completed</MenuItem>
                  <MenuItem value="Failed">Failed</MenuItem>
                  <MenuItem value="Cancelled">Cancelled</MenuItem>
                  <MenuItem value="Paused">Paused</MenuItem>
                </TextField>
              </Grid>
              
              <Grid size={{ xs: 12, sm: 3 }}>
                <TextField
                  select
                  label="Workflow"
                  value={filters.workflowDefinitionId || ''}
                  onChange={(e) => handleFilterChange('workflowDefinitionId', e.target.value || undefined)}
                  fullWidth
                  size="small"
                >
                  <MenuItem value="">All Workflows</MenuItem>
                  {workflows.map((workflow) => (
                    <MenuItem key={workflow.id} value={workflow.id}>
                      {workflow.name}
                    </MenuItem>
                  ))}
                </TextField>
              </Grid>
              
              <Grid size={{ xs: 12, sm: 2 }}>
                <DatePicker
                  label="Start Date"
                  value={filters.startDate || null}
                  onChange={(date) => handleFilterChange('startDate', date)}
                  slotProps={{ textField: { size: 'small', fullWidth: true } }}
                />
              </Grid>
              
              <Grid size={{ xs: 12, sm: 2 }}>
                <DatePicker
                  label="End Date"
                  value={filters.endDate || null}
                  onChange={(date) => handleFilterChange('endDate', date)}
                  slotProps={{ textField: { size: 'small', fullWidth: true } }}
                />
              </Grid>
              
              <Grid size={{ xs: 12, sm: 2 }}>
                <Box sx={{ display: 'flex', gap: 1 }}>
                  <Button
                    variant="outlined"
                    onClick={clearFilters}
                    startIcon={<FilterListIcon />}
                    size="small"
                  >
                    Clear
                  </Button>
                  <IconButton onClick={loadExecutions}>
                    <RefreshIcon />
                  </IconButton>
                </Box>
              </Grid>
            </Grid>
          </CardContent>
        </Card>

        {/* Executions Table */}
        <Card>
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Workflow</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell>Created</TableCell>
                  <TableCell>Duration</TableCell>
                  <TableCell>Steps</TableCell>
                  <TableCell>Progress</TableCell>
                  <TableCell>Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {loading ? (
                  <TableRow>
                    <TableCell colSpan={7} align="center">
                      <CircularProgress />
                    </TableCell>
                  </TableRow>
                ) : executions?.executions.map((execution) => (
                  <TableRow key={execution.id}>
                    <TableCell>
                      <Typography variant="body2" fontWeight="bold">
                        {execution.workflowName}
                      </Typography>
                      <Typography variant="caption" color="textSecondary">
                        {execution.id.substring(0, 8)}...
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={execution.status}
                        color={statusColors[execution.status] || 'default'}
                        size="small"
                      />
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2">
                        {new Date(execution.createdAt).toLocaleString()}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      {formatDuration(execution.duration)}
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2">
                        {execution.totalSteps} total
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <Typography variant="body2">
                          {execution.completedSteps}/{execution.totalSteps}
                        </Typography>
                        {execution.failedSteps > 0 && (
                          <Chip
                            label={`${execution.failedSteps} failed`}
                            color="error"
                            size="small"
                            variant="outlined"
                          />
                        )}
                      </Box>
                    </TableCell>
                    <TableCell>
                      {renderActionButtons(execution)}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
          
          {executions && (
            <TablePagination
              component="div"
              count={executions.totalCount}
              page={(executions.page || 1) - 1}
              onPageChange={handlePageChange}
              rowsPerPage={executions.pageSize}
              onRowsPerPageChange={handleRowsPerPageChange}
              rowsPerPageOptions={[10, 20, 50, 100]}
            />
          )}
        </Card>

        {/* Execution Details Dialog */}
        <Dialog
          open={detailsDialogOpen}
          onClose={() => setDetailsDialogOpen(false)}
          maxWidth="md"
          fullWidth
        >
          <DialogTitle>
            Execution Details
            {selectedExecution && (
              <Typography variant="subtitle1" color="textSecondary">
                {selectedExecution.workflowName} - {selectedExecution.status}
              </Typography>
            )}
          </DialogTitle>
          <DialogContent>
            {selectedExecution && (
              <Box>
                <Grid container spacing={2} sx={{ mb: 2 }}>
                  <Grid size={6}>
                    <Typography variant="body2" color="textSecondary">Created</Typography>
                    <Typography variant="body1">
                      {new Date(selectedExecution.createdAt).toLocaleString()}
                    </Typography>
                  </Grid>
                  <Grid size={6}>
                    <Typography variant="body2" color="textSecondary">Status</Typography>
                    <Chip
                      label={selectedExecution.status}
                      color={statusColors[selectedExecution.status] || 'default'}
                      size="small"
                    />
                  </Grid>
                </Grid>

                {selectedExecution.errorMessage && (
                  <Alert severity="error" sx={{ mb: 2 }}>
                    {selectedExecution.errorMessage}
                  </Alert>
                )}

                <Typography variant="h6" sx={{ mb: 2 }}>Steps</Typography>
                {selectedExecution.steps.map((step) => (
                  <Card key={step.stepId} sx={{ mb: 1 }}>
                    <CardContent sx={{ py: 1 }}>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                        <Box>
                          <Typography variant="body2" fontWeight="bold">
                            {step.order}. {step.stepName} ({step.stepType})
                          </Typography>
                          {step.startedTime && (
                            <Typography variant="caption" color="textSecondary">
                              Started: {new Date(step.startedTime).toLocaleString()}
                            </Typography>
                          )}
                        </Box>
                        <Box sx={{ textAlign: 'right' }}>
                          <Chip
                            label={step.status}
                            color={statusColors[step.status] || 'default'}
                            size="small"
                          />
                          {step.retryCount > 0 && (
                            <Typography variant="caption" display="block">
                              Retries: {step.retryCount}
                            </Typography>
                          )}
                        </Box>
                      </Box>
                      {step.errorMessage && (
                        <Alert severity="error" sx={{ mt: 1 }}>
                          {step.errorMessage}
                        </Alert>
                      )}
                    </CardContent>
                  </Card>
                ))}
              </Box>
            )}
          </DialogContent>
        </Dialog>
      </Box>
    </LocalizationProvider>
  );
};

export default ExecutionsList;

