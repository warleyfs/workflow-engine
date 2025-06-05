import React, { useState, useEffect } from 'react';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import { CssBaseline, Box, AppBar, Toolbar, Typography, Tabs, Tab } from '@mui/material';
import WorkflowDesigner from './components/WorkflowDesigner';
import WorkflowList from './components/WorkflowList';
import ExecutionMonitor from './components/ExecutionMonitor';
import Dashboard from './components/Dashboard';
import ExecutionsList from './components/ExecutionsList';
import WorkflowMetrics from './components/WorkflowMetrics';
import signalRService from './services/signalr';
import './App.css';

const theme = createTheme({
  palette: {
    primary: {
      main: '#1976d2',
    },
    secondary: {
      main: '#dc004e',
    },
  },
});

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`simple-tabpanel-${index}`}
      aria-labelledby={`simple-tab-${index}`}
      {...other}
    >
      {value === index && (
        <Box sx={{ p: 3 }}>
          {children}
        </Box>
      )}
    </div>
  );
}

function App() {
  const [tabValue, setTabValue] = useState(0);

  useEffect(() => {
    // Initialize SignalR connection
    const initSignalR = async () => {
      try {
        if (!signalRService.isConnected) {
          await signalRService.start();
        }
      } catch (error) {
        console.error('Failed to start SignalR connection:', error);
      }
    };

    initSignalR();

    // Cleanup on unmount
    return () => {
      signalRService.stop();
    };
  }, []);

  const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <Box sx={{ flexGrow: 1 }}>
        <AppBar position="static">
          <Toolbar>
            <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
              Workflow Engine
            </Typography>
          </Toolbar>
        </AppBar>
        
        <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tabs 
            value={tabValue} 
            onChange={handleTabChange} 
            aria-label="workflow tabs"
            variant="scrollable"
            scrollButtons="auto"
          >
            <Tab label="Dashboard" />
            <Tab label="Designer" />
            <Tab label="Workflows" />
            <Tab label="Executions" />
            <Tab label="Metrics" />
            <Tab label="Monitor" />
          </Tabs>
        </Box>
        
        <TabPanel value={tabValue} index={0}>
          <Dashboard />
        </TabPanel>
        
        <TabPanel value={tabValue} index={1}>
          <WorkflowDesigner />
        </TabPanel>
        
        <TabPanel value={tabValue} index={2}>
          <WorkflowList />
        </TabPanel>
        
        <TabPanel value={tabValue} index={3}>
          <ExecutionsList />
        </TabPanel>
        
        <TabPanel value={tabValue} index={4}>
          <WorkflowMetrics />
        </TabPanel>
        
        <TabPanel value={tabValue} index={5}>
          <ExecutionMonitor />
        </TabPanel>
      </Box>
    </ThemeProvider>
  );
}

export default App;
