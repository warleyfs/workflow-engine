import * as signalR from '@microsoft/signalr';
import {
  WorkflowExecutionStatusUpdate,
  StepExecutionStatusUpdate,
  NewExecutionNotification,
  ExecutionCompletedNotification,
  DashboardUpdateNotification,
} from '../types/monitoring';

class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private callbacks: Map<string, Function[]> = new Map();

  constructor() {
    this.init();
  }

  private init() {
    const hubUrl = process.env.REACT_APP_HUB_URL || 'http://localhost:5000/monitoring-hub';
    
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl)
      .withAutomaticReconnect()
      .build();

    this.setupEventHandlers();
  }

  private setupEventHandlers() {
    if (!this.connection) return;

    // Workflow execution status changes
    this.connection.on('WorkflowExecutionStatusChanged', (update: WorkflowExecutionStatusUpdate) => {
      this.emit('workflowStatusChanged', update);
    });

    // Step execution status changes
    this.connection.on('StepExecutionStatusChanged', (update: StepExecutionStatusUpdate) => {
      this.emit('stepStatusChanged', update);
    });

    // New execution started
    this.connection.on('NewExecutionStarted', (notification: NewExecutionNotification) => {
      this.emit('newExecution', notification);
    });

    // Execution completed
    this.connection.on('ExecutionCompleted', (notification: ExecutionCompletedNotification) => {
      this.emit('executionCompleted', notification);
    });

    // Dashboard metrics updated
    this.connection.on('DashboardMetricsUpdated', (notification: DashboardUpdateNotification) => {
      this.emit('dashboardUpdated', notification);
    });

    // Connection events
    this.connection.onreconnecting(() => {
      console.log('SignalR reconnecting...');
      this.emit('reconnecting');
    });

    this.connection.onreconnected(() => {
      console.log('SignalR reconnected');
      this.emit('reconnected');
    });

    this.connection.onclose(() => {
      console.log('SignalR connection closed');
      this.emit('disconnected');
    });
  }

  async start(): Promise<void> {
    if (!this.connection) {
      throw new Error('Connection not initialized');
    }

    try {
      await this.connection.start();
      console.log('SignalR connection started');
      this.emit('connected');
    } catch (error) {
      console.error('Error starting SignalR connection:', error);
      throw error;
    }
  }

  async stop(): Promise<void> {
    if (this.connection) {
      try {
        await this.connection.stop();
        console.log('SignalR connection stopped');
      } catch (error) {
        console.error('Error stopping SignalR connection:', error);
      }
    }
  }

  // Subscribe to dashboard updates
  async subscribeToDashboard(): Promise<void> {
    if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
      try {
        await this.connection.invoke('SubscribeToDashboard');
        console.log('Subscribed to dashboard updates');
      } catch (error) {
        console.error('Error subscribing to dashboard:', error);
      }
    }
  }

  // Subscribe to specific workflow execution
  async subscribeToExecution(executionId: string): Promise<void> {
    if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
      try {
        await this.connection.invoke('SubscribeToWorkflowExecution', executionId);
        console.log(`Subscribed to execution ${executionId}`);
      } catch (error) {
        console.error(`Error subscribing to execution ${executionId}:`, error);
      }
    }
  }

  // Unsubscribe from specific workflow execution
  async unsubscribeFromExecution(executionId: string): Promise<void> {
    if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
      try {
        await this.connection.invoke('UnsubscribeFromWorkflowExecution', executionId);
        console.log(`Unsubscribed from execution ${executionId}`);
      } catch (error) {
        console.error(`Error unsubscribing from execution ${executionId}:`, error);
      }
    }
  }

  // Event handling
  on(event: string, callback: Function): void {
    if (!this.callbacks.has(event)) {
      this.callbacks.set(event, []);
    }
    this.callbacks.get(event)!.push(callback);
  }

  off(event: string, callback?: Function): void {
    if (!this.callbacks.has(event)) return;

    if (callback) {
      const callbacks = this.callbacks.get(event)!;
      const index = callbacks.indexOf(callback);
      if (index > -1) {
        callbacks.splice(index, 1);
      }
    } else {
      this.callbacks.delete(event);
    }
  }

  private emit(event: string, data?: any): void {
    const callbacks = this.callbacks.get(event);
    if (callbacks) {
      callbacks.forEach(callback => {
        try {
          callback(data);
        } catch (error) {
          console.error(`Error in event callback for ${event}:`, error);
        }
      });
    }
  }

  // Connection state
  get isConnected(): boolean {
    return this.connection?.state === signalR.HubConnectionState.Connected;
  }

  get connectionState(): signalR.HubConnectionState | null {
    return this.connection?.state || null;
  }
}

// Singleton instance
const signalRService = new SignalRService();
export default signalRService;

