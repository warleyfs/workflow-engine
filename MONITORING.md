# Sistema de Monitoramento de Workflows

## Visão Geral

O sistema de monitoramento foi completamente implementado com as seguintes funcionalidades:

### 🎯 **Funcionalidades Implementadas**

#### 1. **MonitoringController** - API REST para Métricas
- **GET** `/api/monitoring/dashboard` - Dashboard completo com métricas
- **GET** `/api/monitoring/executions` - Lista paginada de execuções com filtros
- **GET** `/api/monitoring/workflow/{id}/metrics` - Métricas específicas de um workflow
- **GET** `/api/monitoring/steps/statistics` - Estatísticas de uso dos steps
- **GET** `/api/monitoring/execution/{id}/logs` - Logs detalhados de uma execução

#### 2. **SignalR Hub** - Atualizações em Tempo Real
- **MonitoringHub** (`/monitoring-hub`) - WebSocket para notificações em tempo real
- Eventos:
  - `WorkflowExecutionStatusChanged` - Mudança de status de workflow
  - `StepExecutionStatusChanged` - Mudança de status de step
  - `NewExecutionStarted` - Nova execução iniciada
  - `ExecutionCompleted` - Execução finalizada
  - `DashboardMetricsUpdated` - Métricas atualizadas

#### 3. **Modelos de Dados Estruturados**
- `DashboardModel` - Métricas completas do dashboard
- `ExecutionListResponse` - Lista paginada de execuções
- `ExecutionSummary` - Resumo de uma execução
- `WorkflowPerformanceMetrics` - Métricas de performance

### 📊 **Métricas Disponíveis**

#### Dashboard Principal
- **Contadores Básicos**
  - Total de workflows ativos
  - Total de execuções
  - Execuções por status (com percentuais)

- **Métricas de Performance**
  - Execuções nas últimas 24h e 7 dias
  - Taxa de sucesso e falha
  - Tempo médio de execução
  - Execuções ativas no momento

- **Execuções Recentes**
  - Últimas 10 execuções com detalhes
  - Status, duração e timestamps

#### Métricas por Workflow
- Estatísticas específicas de cada workflow
- Taxa de sucesso/falha individual
- Tempo médio de execução
- Distribuição por status

#### Estatísticas de Steps
- Steps mais utilizados
- Taxa de sucesso por tipo de step
- Média de retries por step

### 🔄 **Notificações em Tempo Real**

O sistema integra automaticamente com o WorkflowEngine para enviar notificações via SignalR quando:

1. **Nova execução é iniciada**
2. **Status de execução muda** (running, completed, failed, etc.)
3. **Status de step muda** (completed, failed, retrying, etc.)
4. **Execução é finalizada** (sucesso ou falha)

### 📡 **Como Usar as APIs**

#### Obter Dashboard Completo
```bash
curl http://localhost:5000/api/monitoring/dashboard
```

#### Listar Execuções com Filtros
```bash
# Todas as execuções
curl "http://localhost:5000/api/monitoring/executions"

# Apenas execuções falhadas
curl "http://localhost:5000/api/monitoring/executions?status=Failed"

# Execuções de um workflow específico
curl "http://localhost:5000/api/monitoring/executions?workflowDefinitionId=guid"

# Com paginação
curl "http://localhost:5000/api/monitoring/executions?page=2&pageSize=10"
```

#### Métricas de Workflow Específico
```bash
curl http://localhost:5000/api/monitoring/workflow/{workflowId}/metrics
```

#### Logs Detalhados de Execução
```bash
curl http://localhost:5000/api/monitoring/execution/{executionId}/logs
```

### 🌐 **SignalR - Tempo Real**

#### Conectar ao Hub
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/monitoring-hub")
    .build();

connection.start();
```

#### Subscrever a Eventos
```javascript
// Dashboard geral
connection.invoke("SubscribeToDashboard");

// Execução específica
connection.invoke("SubscribeToWorkflowExecution", executionId);

// Escutar eventos
connection.on("WorkflowExecutionStatusChanged", (update) => {
    console.log("Status changed:", update);
});

connection.on("NewExecutionStarted", (execution) => {
    console.log("New execution:", execution);
});
```

### 🔧 **Configuração**

O sistema está automaticamente configurado no `Program.cs`:

```csharp
// SignalR
builder.Services.AddSignalR();

// Serviço de notificações
builder.Services.AddScoped<IMonitoringNotificationService>();

// Hub mapping
app.MapHub<MonitoringHub>("/monitoring-hub");
```

### 📈 **Escalabilidade**

- **Paginação**: Todas as listas suportam paginação
- **Filtros**: Múltiplos filtros por data, status, workflow
- **Performance**: Queries otimizadas com Entity Framework
- **Tempo Real**: SignalR com groups para notificações segmentadas

### 🛠️ **Extensibilidade**

O sistema foi projetado para ser facilmente extensível:

1. **Novos Endpoints**: Adicionar facilmente novas métricas ao `MonitoringController`
2. **Novos Eventos**: Adicionar eventos ao `IMonitoringHubClient`
3. **Filtros Customizados**: Estender os filtros de pesquisa
4. **Métricas Customizadas**: Adicionar novas métricas ao dashboard

### 🔒 **Considerações de Segurança**

- Validação de entrada em todos os endpoints
- Limitação de tamanho de página (máx 100)
- Logs de erro sem exposição de dados sensíveis
- SignalR com autenticação configurável

### 🎨 **Frontend Ready**

Todas as APIs foram projetadas para fácil integração com frontends:

- **JSON estruturado** e consistente
- **Paginação padrão** com metadados
- **WebSocket** para atualizações em tempo real
- **CORS configurado** para desenvolvimento

## ✅ **Status de Implementação**

- ✅ **MonitoringController** - 100% implementado
- ✅ **SignalR Hub** - 100% implementado
- ✅ **Modelos de dados** - 100% implementado
- ✅ **Integração com WorkflowEngine** - 100% implementado
- ✅ **Notificações em tempo real** - 100% implementado
- ✅ **Métricas de performance** - 100% implementado
- ✅ **Sistema de logs** - 100% implementado

**O sistema de monitoramento está completamente funcional e pronto para uso!** 🚀

