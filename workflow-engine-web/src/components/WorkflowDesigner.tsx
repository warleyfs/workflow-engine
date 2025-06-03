import React, { useState, useCallback } from 'react';
import {
  ReactFlow,
  Node,
  Edge,
  Controls,
  Background,
  useNodesState,
  useEdgesState,
  addEdge,
  Connection,
  NodeTypes,
} from '@xyflow/react';
import {
  Box,
  Paper,
  Typography,
  Button,
  Drawer,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  TextField,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  IconButton,
  Toolbar,
  Divider,
} from '@mui/material';
import {
  Add as AddIcon,
  Save as SaveIcon,
  Code as CodeIcon,
  Email as EmailIcon,
  Schedule as ScheduleIcon,
  Close as CloseIcon,
} from '@mui/icons-material';
import '@xyflow/react/dist/style.css';
import { StepType } from '../types/workflow';
import { workflowApi } from '../services/api';
import CustomNode from './CustomNode';

const stepTypes: StepType[] = [
  {
    type: 'LogStep',
    name: 'Log Step',
    description: 'Registra uma mensagem no log',
    icon: 'code',
    configurationSchema: {
      message: { type: 'string', required: true, description: 'Mensagem para log' }
    }
  },
  {
    type: 'EmailStep',
    name: 'Email Step',
    description: 'Envia um email',
    icon: 'email',
    configurationSchema: {
      to: { type: 'string', required: true, description: 'Destinatário' },
      subject: { type: 'string', required: true, description: 'Assunto' },
      body: { type: 'string', required: true, description: 'Corpo do email' }
    }
  },
  {
    type: 'DelayStep',
    name: 'Delay Step',
    description: 'Adiciona um atraso',
    icon: 'schedule',
    configurationSchema: {
      seconds: { type: 'number', required: true, description: 'Segundos de atraso' }
    }
  },
];

const nodeTypes: NodeTypes = {
  customNode: CustomNode,
};

const WorkflowDesigner: React.FC = () => {
  const [nodes, setNodes, onNodesChange] = useNodesState<Node>([]);
  const [edges, setEdges, onEdgesChange] = useEdgesState<Edge>([]);
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [saveDialogOpen, setSaveDialogOpen] = useState(false);
  const [workflowName, setWorkflowName] = useState('');
  const [workflowDescription, setWorkflowDescription] = useState('');
  const [selectedNode, setSelectedNode] = useState<Node | null>(null);
  const [configDialogOpen, setConfigDialogOpen] = useState(false);
  const [nodeConfig, setNodeConfig] = useState<Record<string, any>>({});

  const onConnect = useCallback(
    (params: Connection) => setEdges((eds) => addEdge(params, eds)),
    [setEdges]
  );

  const onDragOver = useCallback((event: React.DragEvent) => {
    event.preventDefault();
    event.dataTransfer.dropEffect = 'move';
  }, []);

  const onDrop = useCallback(
    (event: React.DragEvent) => {
      event.preventDefault();

      const stepType = event.dataTransfer.getData('application/reactflow');
      const step = stepTypes.find(s => s.type === stepType);

      if (!step) return;

      const position = {
        x: event.clientX - 250,
        y: event.clientY - 100,
      };

      const newNode: Node = {
        id: `${stepType}-${Date.now()}`,
        type: 'customNode',
        position,
        data: {
          label: step.name,
          stepType: step.type,
          configuration: {},
          onConfigClick: (nodeId: string) => {
            const node = nodes.find(n => n.id === nodeId);
            if (node) {
              setSelectedNode(node);
              setNodeConfig(node.data.configuration || {});
              setConfigDialogOpen(true);
            }
          }
        },
      };

      setNodes((nds) => nds.concat(newNode));
    },
    [nodes, setNodes]
  );

  const onDragStart = (event: React.DragEvent, stepType: string) => {
    event.dataTransfer.setData('application/reactflow', stepType);
    event.dataTransfer.effectAllowed = 'move';
  };

  const handleSaveWorkflow = async () => {
    try {
      const steps = nodes.map((node, index) => ({
        stepType: node.data.stepType as string,
        name: node.data.label as string,
        description: '',
        configuration: node.data.configuration || {},
        order: index + 1,
      }));

      await workflowApi.createWorkflow({
        name: workflowName,
        description: workflowDescription,
        steps,
      });

      setSaveDialogOpen(false);
      setWorkflowName('');
      setWorkflowDescription('');
      alert('Workflow salvo com sucesso!');
    } catch (error) {
      console.error('Erro ao salvar workflow:', error);
      alert('Erro ao salvar workflow');
    }
  };

  const handleConfigSave = () => {
    if (selectedNode) {
      setNodes((nds) =>
        nds.map((node) =>
          node.id === selectedNode.id
            ? { ...node, data: { ...node.data, configuration: nodeConfig } }
            : node
        )
      );
    }
    setConfigDialogOpen(false);
    setSelectedNode(null);
    setNodeConfig({});
  };

  const getStepIcon = (iconName: string) => {
    switch (iconName) {
      case 'code': return <CodeIcon />;
      case 'email': return <EmailIcon />;
      case 'schedule': return <ScheduleIcon />;
      default: return <CodeIcon />;
    }
  };

  const renderConfigField = (key: string, field: any) => {
    return (
      <TextField
        key={key}
        fullWidth
        margin="normal"
        label={field.description || key}
        value={nodeConfig[key] || ''}
        onChange={(e) => setNodeConfig({ ...nodeConfig, [key]: e.target.value })}
        required={field.required}
        type={field.type === 'number' ? 'number' : 'text'}
      />
    );
  };

  const selectedStepType = selectedNode
    ? stepTypes.find(s => s.type === selectedNode.data.stepType)
    : null;

  return (
    <Box sx={{ height: 'calc(100vh - 200px)', display: 'flex' }}>
      {/* Sidebar */}
      <Drawer
        variant="persistent"
        anchor="left"
        open={drawerOpen}
        sx={{
          width: 300,
          flexShrink: 0,
          '& .MuiDrawer-paper': {
            width: 300,
            boxSizing: 'border-box',
            position: 'relative',
          },
        }}
      >
        <Toolbar>
          <Typography variant="h6">Componentes</Typography>
          <IconButton
            sx={{ ml: 'auto' }}
            onClick={() => setDrawerOpen(false)}
          >
            <CloseIcon />
          </IconButton>
        </Toolbar>
        <Divider />
        <List>
          {stepTypes.map((step) => (
            <ListItem
              key={step.type}
              draggable
              onDragStart={(e) => onDragStart(e, step.type)}
              sx={{
                cursor: 'grab',
                '&:hover': { backgroundColor: 'action.hover' },
                userSelect: 'none',
              }}
            >
              <ListItemIcon>
                {getStepIcon(step.icon)}
              </ListItemIcon>
              <ListItemText
                primary={step.name}
                secondary={step.description}
              />
            </ListItem>
          ))}
        </List>
      </Drawer>

      {/* Main Content */}
      <Box sx={{ flexGrow: 1, display: 'flex', flexDirection: 'column' }}>
        {/* Toolbar */}
        <Paper sx={{ p: 1, mb: 1 }}>
          <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
            <Button
              startIcon={<AddIcon />}
              onClick={() => setDrawerOpen(true)}
              variant="outlined"
            >
              Componentes
            </Button>
            <Button
              startIcon={<SaveIcon />}
              onClick={() => setSaveDialogOpen(true)}
              variant="contained"
              disabled={nodes.length === 0}
            >
              Salvar
            </Button>
            <Typography variant="body2" sx={{ ml: 2, color: 'text.secondary' }}>
              Arraste componentes da barra lateral para criar seu workflow
            </Typography>
          </Box>
        </Paper>

        {/* React Flow */}
        <Box sx={{ flexGrow: 1 }}>
          <ReactFlow
            nodes={nodes}
            edges={edges}
            onNodesChange={onNodesChange}
            onEdgesChange={onEdgesChange}
            onConnect={onConnect}
            onDrop={onDrop}
            onDragOver={onDragOver}
            nodeTypes={nodeTypes}
            fitView
          >
            <Controls />
            <Background />
          </ReactFlow>
        </Box>
      </Box>

      {/* Save Dialog */}
      <Dialog open={saveDialogOpen} onClose={() => setSaveDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Salvar Workflow</DialogTitle>
        <DialogContent>
          <TextField
            autoFocus
            margin="dense"
            label="Nome do Workflow"
            fullWidth
            variant="outlined"
            value={workflowName}
            onChange={(e) => setWorkflowName(e.target.value)}
            sx={{ mb: 2 }}
          />
          <TextField
            margin="dense"
            label="Descrição"
            fullWidth
            multiline
            rows={3}
            variant="outlined"
            value={workflowDescription}
            onChange={(e) => setWorkflowDescription(e.target.value)}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setSaveDialogOpen(false)}>Cancelar</Button>
          <Button onClick={handleSaveWorkflow} disabled={!workflowName.trim()}>
            Salvar
          </Button>
        </DialogActions>
      </Dialog>

      {/* Configuration Dialog */}
      <Dialog open={configDialogOpen} onClose={() => setConfigDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>
          {`Configurar ${selectedNode?.data.label}`}
        </DialogTitle>
        <DialogContent>
          {selectedStepType && Object.entries(selectedStepType.configurationSchema).map(([key, field]) =>
            renderConfigField(key, field)
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setConfigDialogOpen(false)}>Cancelar</Button>
          <Button onClick={handleConfigSave}>Salvar</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default WorkflowDesigner;

