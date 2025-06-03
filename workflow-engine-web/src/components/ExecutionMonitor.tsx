import React, { useState, useEffect } from 'react';
import {
  Box,
  Paper,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Button,
  Chip,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Alert,
  CircularProgress,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Grid,
  Card,
  CardContent,
  LinearProgress,
} from '@mui/material';
import {
  PlayArrow as PlayIcon,
  Pause as PauseIcon,
  Stop as StopIcon,
  Refresh as RefreshIcon,
  ExpandMore as ExpandMoreIcon,
  Visibility as VisibilityIcon,
} from '@mui/icons-material';
import {
  WorkflowExecution,
  StepExecution,
  WorkflowExecutionStatus,
  StepExecutionStatus,
} from '../types/workflow';
import { workflowApi } from '../services/api';

const ExecutionMonitor: React.FC = () => {
  const [executions, setExecutions] = useState<WorkflowExecution[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedExecution, setSelectedExecution] = useState<WorkflowExecution | null>(null);
  const [stepExecutions, setStepExecutions] = useState<StepExecution[]>([]);
  const [detailsDialogOpen, setDetailsDialogOpen] = useState(false);
  const [autoRefresh, setAutoRefresh] = useState(true);

  const loadExecutions = async () => {
    try {
      setError(null);
      const data = await workflowApi.getExecutions();
      setExecutions(data);
    } catch (err) {
      setError('Erro ao carregar execuções');
      console.error('Erro ao carregar execuções:', err);
    } finally {
      setLoading(false);
    }
  };

  const loadStepExecutions = async (executionId: string) => {
    try {
      const steps = await workflowApi.getStepExecutions(executionId);
      setStepExecutions(steps);
    } catch (err) {
      console.error('Erro ao carregar steps da execução:', err);
    }
  };

  const handlePauseExecution = async (execution: WorkflowExecution) => {
    try {
      await workflowApi.pauseExecution(execution.id);
      await loadExecutions();
    } catch (err) {
      setError('Erro ao pausar execução');
      console.error('Erro ao pausar execução:', err);
    }
  };

  const handleResumeExecution = async (execution: WorkflowExecution) => {
    try {
      await workflowApi.resumeExecution(execution.id);
      await loadExecutions();
    } catch (err) {
      setError('Erro ao retomar execução');
      console.error('Erro ao retomar execução:', err);
    }
  };

  const handleCancelExecution = async (execution: WorkflowExecution) => {
    if (window.confirm('Tem certeza que deseja cancelar esta execução?')) {
      try {
        await workflowApi.cancelExecution(execution.id);
        await loadExecutions();
      } catch (err) {
        setError('Erro ao cancelar execução');
        console.error('Erro ao cancelar execução:', err);
      }
    }
  };

  const handleViewDetails = async (execution: WorkflowExecution) => {
    setSelectedExecution(execution);
    await loadStepExecutions(execution.id);
    setDetailsDialogOpen(true);
  };

  useEffect(() => {
    loadExecutions();
  }, []);

  useEffect(() => {
    if (autoRefresh) {
      const interval = setInterval(loadExecutions, 5000); // Refresh a cada 5 segundos
      return () => clearInterval(interval);
    }
  }, [autoRefresh]);

  const getStatusColor = (status: WorkflowExecutionStatus | StepExecutionStatus) => {
    switch (status) {
      case WorkflowExecutionStatus.Completed:
      case StepExecutionStatus.Completed:
        return 'success';
      case WorkflowExecutionStatus.Failed:
      case StepExecutionStatus.Failed:
        return 'error';
      case WorkflowExecutionStatus.Running:
      case StepExecutionStatus.Running:
        return 'primary';
      case WorkflowExecutionStatus.Pending:
      case StepExecutionStatus.Pending:
        return 'default';
      case WorkflowExecutionStatus.Paused:
        return 'warning';
      case WorkflowExecutionStatus.Cancelled:
        return 'error';
      case StepExecutionStatus.Skipped:
        return 'warning';
      case StepExecutionStatus.Retrying:
        return 'info';
      default:
        return 'default';
    }
  };

  const formatDate = (dateString?: string | Date) => {
    if (!dateString) return 'N/A';
    const date = typeof dateString === 'string' ? new Date(dateString) : dateString;
    return date.toLocaleDateString('pt-BR') + ' ' + date.toLocaleTimeString('pt-BR');
  };

  const formatDuration = (startDate?: string | Date, endDate?: string | Date) => {
    if (!startDate) return 'N/A';
    const start = typeof startDate === 'string' ? new Date(startDate) : startDate;
    const end = endDate ? (typeof endDate === 'string' ? new Date(endDate) : endDate) : new Date();
    const diffMs = end.getTime() - start.getTime();
    const diffSecs = Math.floor(diffMs / 1000);
    
    if (diffSecs < 60) return `${diffSecs}s`;
    if (diffSecs < 3600) return `${Math.floor(diffSecs / 60)}m ${diffSecs % 60}s`;
    return `${Math.floor(diffSecs / 3600)}h ${Math.floor((diffSecs % 3600) / 60)}m`;
  };

  const getExecutionProgress = (execution: WorkflowExecution) => {
    if (!execution.currentStep) return 0;
    // Assumindo que temos informação sobre o total de steps (precisa ser adicionado ao backend)
    return (execution.currentStep / 4) * 100; // Exemplo com 4 steps
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: 400 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h5">Monitor de Execuções</Typography>
        <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
          <Button
            startIcon={<RefreshIcon />}
            onClick={loadExecutions}
            variant="outlined"
          >
            Atualizar
          </Button>
          <Button
            variant={autoRefresh ? 'contained' : 'outlined'}
            onClick={() => setAutoRefresh(!autoRefresh)}
            size="small"
          >
            Auto Refresh: {autoRefresh ? 'ON' : 'OFF'}
          </Button>
        </Box>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      {/* Summary Cards */}
      <Grid container spacing={2} sx={{ mb: 3 }}>
        {Object.values(WorkflowExecutionStatus).map((status) => {
          const count = executions.filter(e => e.status === status).length;
          return (
            <Grid size={{ xs: 12, sm: 6, md: 2 }} key={status}>
              <Card>
                <CardContent sx={{ textAlign: 'center', py: 2 }}>
                  <Typography variant="h4" color={`${getStatusColor(status)}.main`}>
                    {count}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    {status}
                  </Typography>
                </CardContent>
              </Card>
            </Grid>
          );
        })}
      </Grid>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>ID</TableCell>
              <TableCell>Workflow</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Progresso</TableCell>
              <TableCell>Iniciado em</TableCell>
              <TableCell>Duração</TableCell>
              <TableCell align="right">Ações</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {executions.length === 0 ? (
              <TableRow>
                <TableCell colSpan={7} align="center">
                  <Typography variant="body2" color="text.secondary">
                    Nenhuma execução encontrada.
                  </Typography>
                </TableCell>
              </TableRow>
            ) : (
              executions.map((execution) => (
                <TableRow key={execution.id} hover>
                  <TableCell>
                    <Typography variant="body2" sx={{ fontFamily: 'monospace' }}>
                      {execution.id.substring(0, 8)}...
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2">
                      {execution.workflowDefinitionId.substring(0, 8)}...
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={execution.status}
                      color={getStatusColor(execution.status) as any}
                      size="small"
                    />
                  </TableCell>
                  <TableCell>
                    <Box sx={{ width: '100px' }}>
                      <LinearProgress
                        variant="determinate"
                        value={getExecutionProgress(execution)}
                        color={getStatusColor(execution.status) as any}
                      />
                      <Typography variant="caption" color="text.secondary">
                        Step {execution.currentStep || 0}
                      </Typography>
                    </Box>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2">
                      {formatDate(execution.startedAt)}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2">
                      {formatDuration(execution.startedAt, execution.completedAt)}
                    </Typography>
                  </TableCell>
                  <TableCell align="right">
                    <Box sx={{ display: 'flex', gap: 1 }}>
                      <IconButton
                        onClick={() => handleViewDetails(execution)}
                        title="Ver detalhes"
                        size="small"
                      >
                        <VisibilityIcon fontSize="small" />
                      </IconButton>
                      
                      {execution.status === WorkflowExecutionStatus.Running && (
                        <IconButton
                          color="warning"
                          onClick={() => handlePauseExecution(execution)}
                          title="Pausar"
                          size="small"
                        >
                          <PauseIcon fontSize="small" />
                        </IconButton>
                      )}
                      
                      {execution.status === WorkflowExecutionStatus.Paused && (
                        <IconButton
                          color="primary"
                          onClick={() => handleResumeExecution(execution)}
                          title="Retomar"
                          size="small"
                        >
                          <PlayIcon fontSize="small" />
                        </IconButton>
                      )}
                      
                      {(execution.status === WorkflowExecutionStatus.Running ||
                        execution.status === WorkflowExecutionStatus.Paused) && (
                        <IconButton
                          color="error"
                          onClick={() => handleCancelExecution(execution)}
                          title="Cancelar"
                          size="small"
                        >
                          <StopIcon fontSize="small" />
                        </IconButton>
                      )}
                    </Box>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>

      {/* Details Dialog */}
      <Dialog
        open={detailsDialogOpen}
        onClose={() => setDetailsDialogOpen(false)}
        maxWidth="lg"
        fullWidth
      >
        <DialogTitle>
          Detalhes da Execução: {selectedExecution?.id}
        </DialogTitle>
        <DialogContent>
          {selectedExecution && (
            <Box>
              <Grid container spacing={2} sx={{ mb: 3 }}>
                <Grid size={6}>
                  <Typography variant="subtitle2">Status:</Typography>
                  <Chip
                    label={selectedExecution.status}
                    color={getStatusColor(selectedExecution.status) as any}
                    sx={{ mt: 0.5 }}
                  />
                </Grid>
                <Grid size={6}>
                  <Typography variant="subtitle2">Duração:</Typography>
                  <Typography variant="body2">
                    {formatDuration(selectedExecution.startedAt, selectedExecution.completedAt)}
                  </Typography>
                </Grid>
              </Grid>

              <Accordion defaultExpanded>
                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                  <Typography variant="h6">Dados de Entrada</Typography>
                </AccordionSummary>
                <AccordionDetails>
                  <Paper sx={{ p: 2, bgcolor: 'grey.50' }}>
                    <pre style={{ margin: 0, fontSize: '0.875rem' }}>
                      {JSON.stringify(selectedExecution.inputData, null, 2)}
                    </pre>
                  </Paper>
                </AccordionDetails>
              </Accordion>

              <Accordion>
                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                  <Typography variant="h6">Steps da Execução</Typography>
                </AccordionSummary>
                <AccordionDetails>
                  <TableContainer>
                    <Table size="small">
                      <TableHead>
                        <TableRow>
                          <TableCell>Step</TableCell>
                          <TableCell>Status</TableCell>
                          <TableCell>Iniciado</TableCell>
                          <TableCell>Concluído</TableCell>
                          <TableCell>Duração</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {stepExecutions.map((step, index) => (
                          <TableRow key={step.id}>
                            <TableCell>{index + 1}</TableCell>
                            <TableCell>
                              <Chip
                                label={step.status}
                                color={getStatusColor(step.status) as any}
                                size="small"
                              />
                            </TableCell>
                            <TableCell>{formatDate(step.startedAt)}</TableCell>
                            <TableCell>{formatDate(step.completedAt)}</TableCell>
                            <TableCell>
                              {formatDuration(step.startedAt, step.completedAt)}
                            </TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </TableContainer>
                </AccordionDetails>
              </Accordion>

              {selectedExecution.outputData && (
                <Accordion>
                  <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                    <Typography variant="h6">Dados de Saída</Typography>
                  </AccordionSummary>
                  <AccordionDetails>
                    <Paper sx={{ p: 2, bgcolor: 'grey.50' }}>
                      <pre style={{ margin: 0, fontSize: '0.875rem' }}>
                        {JSON.stringify(selectedExecution.outputData, null, 2)}
                      </pre>
                    </Paper>
                  </AccordionDetails>
                </Accordion>
              )}

              {selectedExecution.error && (
                <Accordion>
                  <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                    <Typography variant="h6" color="error">Erro</Typography>
                  </AccordionSummary>
                  <AccordionDetails>
                    <Alert severity="error">
                      {selectedExecution.error}
                    </Alert>
                  </AccordionDetails>
                </Accordion>
              )}
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDetailsDialogOpen(false)}>Fechar</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default ExecutionMonitor;

