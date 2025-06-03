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
  TextField,
  Alert,
  CircularProgress,
} from '@mui/material';
import {
  PlayArrow as PlayIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Add as AddIcon,
  Refresh as RefreshIcon,
} from '@mui/icons-material';
import { WorkflowDefinition, ExecuteWorkflowRequest } from '../types/workflow';
import { workflowApi } from '../services/api';

const WorkflowList: React.FC = () => {
  const [workflows, setWorkflows] = useState<WorkflowDefinition[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [executeDialogOpen, setExecuteDialogOpen] = useState(false);
  const [selectedWorkflow, setSelectedWorkflow] = useState<WorkflowDefinition | null>(null);
  const [inputData, setInputData] = useState('{}');
  const [executing, setExecuting] = useState(false);

  const loadWorkflows = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await workflowApi.getWorkflows();
      setWorkflows(data);
    } catch (err) {
      setError('Erro ao carregar workflows');
      console.error('Erro ao carregar workflows:', err);
    } finally {
      setLoading(false);
    }
  };

  const createDemoWorkflow = async () => {
    try {
      await workflowApi.createDemoWorkflow();
      await loadWorkflows();
    } catch (err) {
      setError('Erro ao criar workflow de demonstração');
      console.error('Erro ao criar demo workflow:', err);
    }
  };

  const handleExecuteWorkflow = async () => {
    if (!selectedWorkflow) return;

    try {
      setExecuting(true);
      const parsedInputData = JSON.parse(inputData);
      
      const result = await workflowApi.executeWorkflow(selectedWorkflow.id, {
        inputData: parsedInputData,
      });

      setExecuteDialogOpen(false);
      setInputData('{}');
      alert(`Workflow executado com sucesso! ID da execução: ${result.id}`);
    } catch (err) {
      setError('Erro ao executar workflow');
      console.error('Erro ao executar workflow:', err);
    } finally {
      setExecuting(false);
    }
  };

  const handleDeleteWorkflow = async (workflow: WorkflowDefinition) => {
    if (window.confirm(`Tem certeza que deseja excluir o workflow "${workflow.name}"?`)) {
      try {
        await workflowApi.deleteWorkflow(workflow.id);
        await loadWorkflows();
      } catch (err) {
        setError('Erro ao excluir workflow');
        console.error('Erro ao excluir workflow:', err);
      }
    }
  };

  useEffect(() => {
    loadWorkflows();
  }, []);

  const formatDate = (dateString: string | Date) => {
    const date = typeof dateString === 'string' ? new Date(dateString) : dateString;
    return date.toLocaleDateString('pt-BR') + ' ' + date.toLocaleTimeString('pt-BR');
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
        <Typography variant="h5">Workflows</Typography>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button
            startIcon={<AddIcon />}
            onClick={createDemoWorkflow}
            variant="outlined"
          >
            Demo Workflow
          </Button>
          <Button
            startIcon={<RefreshIcon />}
            onClick={loadWorkflows}
            variant="outlined"
          >
            Atualizar
          </Button>
        </Box>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Nome</TableCell>
              <TableCell>Descrição</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Criado em</TableCell>
              <TableCell>Atualizado em</TableCell>
              <TableCell align="right">Ações</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {workflows.length === 0 ? (
              <TableRow>
                <TableCell colSpan={6} align="center">
                  <Typography variant="body2" color="text.secondary">
                    Nenhum workflow encontrado. Crie um novo workflow ou use o botão "Demo Workflow".
                  </Typography>
                </TableCell>
              </TableRow>
            ) : (
              workflows.map((workflow) => (
                <TableRow key={workflow.id} hover>
                  <TableCell>
                    <Typography variant="body1" fontWeight="medium">
                      {workflow.name}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2" color="text.secondary">
                      {workflow.description || 'Sem descrição'}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={workflow.isActive ? 'Ativo' : 'Inativo'}
                      color={workflow.isActive ? 'success' : 'default'}
                      size="small"
                    />
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2">
                      {formatDate(workflow.createdAt)}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2">
                      {formatDate(workflow.updatedAt)}
                    </Typography>
                  </TableCell>
                  <TableCell align="right">
                    <Box sx={{ display: 'flex', gap: 1 }}>
                      <IconButton
                        color="primary"
                        onClick={() => {
                          setSelectedWorkflow(workflow);
                          setExecuteDialogOpen(true);
                        }}
                        title="Executar"
                      >
                        <PlayIcon />
                      </IconButton>
                      <IconButton
                        color="error"
                        onClick={() => handleDeleteWorkflow(workflow)}
                        title="Excluir"
                      >
                        <DeleteIcon />
                      </IconButton>
                    </Box>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>

      {/* Execute Dialog */}
      <Dialog
        open={executeDialogOpen}
        onClose={() => setExecuteDialogOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>
          Executar Workflow: {selectedWorkflow?.name}
        </DialogTitle>
        <DialogContent>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            Insira os dados de entrada para o workflow em formato JSON:
          </Typography>
          <TextField
            fullWidth
            multiline
            rows={6}
            label="Dados de Entrada (JSON)"
            value={inputData}
            onChange={(e) => setInputData(e.target.value)}
            placeholder='{
  "userId": 123,
  "email": "usuario@exemplo.com"
}'
            sx={{ fontFamily: 'monospace' }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setExecuteDialogOpen(false)}>Cancelar</Button>
          <Button
            onClick={handleExecuteWorkflow}
            variant="contained"
            disabled={executing}
            startIcon={executing ? <CircularProgress size={16} /> : <PlayIcon />}
          >
            {executing ? 'Executando...' : 'Executar'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default WorkflowList;

