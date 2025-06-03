import React from 'react';
import { Handle, Position, NodeProps } from '@xyflow/react';
import {
  Box,
  Paper,
  Typography,
  IconButton,
  Chip,
} from '@mui/material';
import {
  Settings as SettingsIcon,
  Code as CodeIcon,
  Email as EmailIcon,
  Schedule as ScheduleIcon,
  CheckCircle as CheckCircleIcon,
  Error as ErrorIcon,
  PlayArrow as PlayArrowIcon,
  Pause as PauseIcon,
} from '@mui/icons-material';
import { StepExecutionStatus } from '../types/workflow';

interface CustomNodeData {
  label: string;
  stepType: string;
  configuration: Record<string, any>;
  status?: StepExecutionStatus;
  onConfigClick?: (nodeId: string) => void;
}

const CustomNode: React.FC<NodeProps<CustomNodeData>> = ({ id, data }) => {
  const getStepIcon = (stepType: string) => {
    switch (stepType) {
      case 'LogStep': return <CodeIcon />;
      case 'EmailStep': return <EmailIcon />;
      case 'DelayStep': return <ScheduleIcon />;
      default: return <CodeIcon />;
    }
  };

  const getStatusIcon = (status?: StepExecutionStatus) => {
    if (!status) return null;
    
    switch (status) {
      case StepExecutionStatus.Completed:
        return <CheckCircleIcon color="success" />;
      case StepExecutionStatus.Failed:
        return <ErrorIcon color="error" />;
      case StepExecutionStatus.Running:
        return <PlayArrowIcon color="primary" />;
      case StepExecutionStatus.Pending:
        return <PauseIcon color="disabled" />;
      default:
        return null;
    }
  };

  const getStatusColor = (status?: StepExecutionStatus) => {
    switch (status) {
      case StepExecutionStatus.Completed: return 'success';
      case StepExecutionStatus.Failed: return 'error';
      case StepExecutionStatus.Running: return 'primary';
      case StepExecutionStatus.Pending: return 'default';
      case StepExecutionStatus.Skipped: return 'warning';
      case StepExecutionStatus.Retrying: return 'info';
      default: return 'default';
    }
  };

  return (
    <>
      <Handle type="target" position={Position.Top} />
      
      <Paper
        elevation={3}
        sx={{
          minWidth: 150,
          minHeight: 80,
          padding: 1,
          borderRadius: 2,
          border: data.status ? `2px solid` : 'none',
          borderColor: data.status ? `${getStatusColor(data.status)}.main` : 'transparent',
          backgroundColor: 'background.paper',
        }}
      >
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            {getStepIcon(data.stepType)}
            {data.status && getStatusIcon(data.status)}
          </Box>
          <IconButton
            size="small"
            onClick={() => data.onConfigClick?.(id)}
            sx={{ p: 0.5 }}
          >
            <SettingsIcon fontSize="small" />
          </IconButton>
        </Box>
        
        <Typography variant="body2" sx={{ fontWeight: 'medium', mb: 1 }}>
          {data.label}
        </Typography>
        
        {data.status && (
          <Chip
            label={data.status}
            size="small"
            color={getStatusColor(data.status) as any}
            sx={{ fontSize: '0.7rem' }}
          />
        )}
        
        {Object.keys(data.configuration).length > 0 && (
          <Typography variant="caption" sx={{ display: 'block', mt: 0.5, color: 'text.secondary' }}>
            Configurado
          </Typography>
        )}
      </Paper>
      
      <Handle type="source" position={Position.Bottom} />
    </>
  );
};

export default CustomNode;

