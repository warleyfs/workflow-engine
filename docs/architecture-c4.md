# Diagrama C4 Model - Sistema de Workflow Engine

## 1. Context Diagram (Nível 1)

```mermaid
graph TB
    User[👤 Usuário/Sistema Cliente]
    Admin[👤 Administrador]
    
    WorkflowSystem[📦 Workflow Engine System<br/>Sistema de orquestração de workflows<br/>com agendamento e monitoramento]
    
    EmailService[📧 Serviço de Email<br/>Sistema externo]
    Database[(🗄️ Banco de Dados<br/>PostgreSQL/SQL Server)]
    
    User -->|Cria e executa workflows| WorkflowSystem
    Admin -->|Monitora execuções| WorkflowSystem
    WorkflowSystem -->|Envia notificações| EmailService
    WorkflowSystem -->|Persiste dados| Database
    
    style WorkflowSystem fill:#e1f5fe,stroke:#01579b,stroke-width:3px
    style User fill:#f3e5f5,stroke:#4a148c
    style Admin fill:#f3e5f5,stroke:#4a148c
    style EmailService fill:#fff3e0,stroke:#e65100
    style Database fill:#e8f5e8,stroke:#2e7d32
```

## 2. Container Diagram (Nível 2)

```mermaid
graph TB
    User[👤 Usuário]
    Admin[👤 Administrador]
    
    subgraph "Workflow Engine System"
        WebAPI[🌐 Web API<br/>ASP.NET Core 8<br/>REST API para gerenciar workflows]
        
        HangfireDash[📊 Hangfire Dashboard<br/>Interface web para<br/>monitoramento de jobs]
        
        WorkflowCore[⚙️ Workflow Engine Core<br/>.NET 8 Library<br/>Lógica de orquestração e execução]
        
        HangfireWorker[🔄 Hangfire Background Jobs<br/>Processamento assíncrono<br/>de workflows e etapas]
        
        Database[(🗄️ Workflow Database<br/>Entity Framework Core<br/>Definições e execuções)]
        
        HangfireDB[(📋 Hangfire Storage<br/>Armazenamento de jobs<br/>e status de execução)]
    end
    
    EmailService[📧 Serviço de Email]
    
    User -->|HTTP/REST| WebAPI
    Admin -->|HTTP| HangfireDash
    
    WebAPI -->|Usa| WorkflowCore
    WebflowCore -->|Agenda jobs| HangfireWorker
    HangfireWorker -->|Executa etapas| WorkflowCore
    
    WorkflowCore -->|EF Core| Database
    HangfireWorker -->|Persiste jobs| HangfireDB
    
    WorkflowCore -->|Integra| EmailService
    
    style WebAPI fill:#e3f2fd,stroke:#1976d2,stroke-width:2px
    style WorkflowCore fill:#e8f5e8,stroke:#388e3c,stroke-width:2px
    style HangfireWorker fill:#fff3e0,stroke:#f57c00,stroke-width:2px
    style HangfireDash fill:#fce4ec,stroke:#c2185b,stroke-width:2px
```

## 3. Component Diagram (Nível 3) - Workflow Engine Core

```mermaid
graph TB
    subgraph "Web API Container"
        Controller[🎮 WorkflowController<br/>Endpoints REST]
    end
    
    subgraph "Workflow Engine Core Container"
        IWorkflowEngine[🔧 IWorkflowEngine<br/>Interface principal]
        WorkflowEngine[⚙️ WorkflowEngine<br/>Orquestrador principal]
        
        subgraph "Step Management"
            IWorkflowStep[📋 IWorkflowStep<br/>Interface de etapas]
            LogStep[📝 LogStep<br/>Etapa de logging]
            EmailStep[📧 EmailStep<br/>Etapa de email]
            DelayStep[⏱️ DelayStep<br/>Etapa de delay]
        end
        
        subgraph "Data Models"
            Entities[🗂️ Entities<br/>WorkflowDefinition<br/>StepDefinition<br/>WorkflowExecution]
            Models[📊 Models<br/>DTOs e ViewModels]
            Enums[🏷️ Enums<br/>Status e tipos]
        end
        
        subgraph "Data Access"
            DbContext[🗄️ WorkflowDbContext<br/>Entity Framework]
        end
    end
    
    subgraph "Hangfire Container"
        BackgroundJobs[🔄 Background Jobs<br/>Processamento assíncrono]
    end
    
    Controller -->|Usa| IWorkflowEngine
    IWorkflowEngine -->|Implementa| WorkflowEngine
    
    WorkflowEngine -->|Agenda| BackgroundJobs
    WorkflowEngine -->|Persiste| DbContext
    WorkflowEngine -->|Executa| IWorkflowStep
    
    IWorkflowStep -->|Implementa| LogStep
    IWorkflowStep -->|Implementa| EmailStep
    IWorkflowStep -->|Implementa| DelayStep
    
    WorkflowEngine -->|Usa| Entities
    WorkflowEngine -->|Usa| Models
    WorkflowEngine -->|Usa| Enums
    
    DbContext -->|Mapeia| Entities
    
    style IWorkflowEngine fill:#e8f5e8,stroke:#2e7d32,stroke-width:2px
    style WorkflowEngine fill:#e8f5e8,stroke:#2e7d32,stroke-width:3px
    style IWorkflowStep fill:#fff3e0,stroke:#f57c00,stroke-width:2px
```

## 4. Code Diagram (Nível 4) - Fluxo de Execução

```mermaid
sequenceDiagram
    participant Client as Cliente
    participant API as Web API
    participant Engine as Workflow Engine
    participant Hangfire as Hangfire Jobs
    participant DB as Database
    participant Step as Workflow Step
    
    Client->>API: POST /api/workflow/{id}/execute
    API->>Engine: StartWorkflowAsync()
    
    Engine->>DB: Criar WorkflowExecution
    Engine->>DB: Criar StepExecutions
    Engine->>Hangfire: Agendar ProcessWorkflowAsync()
    Engine-->>API: Retorna ExecutionId
    API-->>Client: 200 OK + ExecutionId
    
    Note over Hangfire: Background Processing
    Hangfire->>Engine: ProcessWorkflowAsync()
    Engine->>DB: Buscar próxima etapa
    Engine->>Hangfire: Agendar ProcessStepAsync()
    
    Hangfire->>Engine: ProcessStepAsync()
    Engine->>Step: CanExecuteAsync()
    Step-->>Engine: true/false
    
    alt Pode executar
        Engine->>Step: ExecuteAsync()
        Step->>Step: Lógica da etapa
        Step-->>Engine: StepResult
        Engine->>DB: Atualizar status
        
        alt Sucesso
            Engine->>Hangfire: Agendar próxima etapa
        else Falha com retry
            Engine->>Hangfire: Agendar retry
        else Falha definitiva
            Engine->>DB: Marcar workflow como falhou
        end
    else Não pode executar
        Engine->>DB: Marcar etapa como pulada
        Engine->>Hangfire: Agendar próxima etapa
    end
    
    Note over Client: Monitoramento
    Client->>API: GET /api/workflow/execution/{id}
    API->>Engine: GetWorkflowStatusAsync()
    Engine->>DB: Buscar status
    Engine-->>API: WorkflowExecutionResult
    API-->>Client: Status detalhado
```

## 5. Deployment Diagram

```mermaid
graph TB
    subgraph "Production Environment"
        subgraph "Load Balancer"
            LB[🔄 Load Balancer<br/>nginx/HAProxy]
        end
        
        subgraph "Application Tier"
            App1[🖥️ App Server 1<br/>WorkflowEngine.Api<br/>Docker Container]
            App2[🖥️ App Server 2<br/>WorkflowEngine.Api<br/>Docker Container]
        end
        
        subgraph "Background Processing"
            Worker1[⚙️ Background Worker 1<br/>Hangfire Server<br/>Docker Container]
            Worker2[⚙️ Background Worker 2<br/>Hangfire Server<br/>Docker Container]
        end
        
        subgraph "Data Tier"
            MainDB[(🗄️ Main Database<br/>PostgreSQL<br/>Workflow Data)]
            HangfireDB[(📋 Hangfire Database<br/>PostgreSQL<br/>Job Storage)]
            Redis[(🔴 Redis Cache<br/>Distributed Cache)]
        end
        
        subgraph "Monitoring"
            Monitor[📊 Monitoring<br/>Hangfire Dashboard<br/>Application Insights]
        end
    end
    
    subgraph "External Services"
        Email[📧 Email Service<br/>SendGrid/SMTP]
        SMS[📱 SMS Service<br/>Twilio]
    end
    
    LB --> App1
    LB --> App2
    
    App1 --> MainDB
    App2 --> MainDB
    App1 --> Redis
    App2 --> Redis
    
    Worker1 --> MainDB
    Worker2 --> MainDB
    Worker1 --> HangfireDB
    Worker2 --> HangfireDB
    
    Worker1 --> Email
    Worker2 --> Email
    Worker1 --> SMS
    Worker2 --> SMS
    
    Monitor --> HangfireDB
    
    style LB fill:#ffebee,stroke:#c62828,stroke-width:2px
    style App1 fill:#e3f2fd,stroke:#1976d2,stroke-width:2px
    style App2 fill:#e3f2fd,stroke:#1976d2,stroke-width:2px
    style Worker1 fill:#fff3e0,stroke:#f57c00,stroke-width:2px
    style Worker2 fill:#fff3e0,stroke:#f57c00,stroke-width:2px
```

## Principais Características da Arquitetura

### 🎯 **Separação de Responsabilidades**
- **API Layer**: Exposição de endpoints REST
- **Core Engine**: Lógica de negócio e orquestração
- **Background Processing**: Execução assíncrona via Hangfire
- **Data Layer**: Persistência com Entity Framework

### ⚡ **Escalabilidade**
- **Horizontal**: Múltiplas instâncias da API e workers
- **Vertical**: Otimização de recursos por container
- **Assíncrona**: Processamento não-bloqueante

### 🔄 **Resiliência**
- **Retry automático**: Falhas temporárias são reprocessadas
- **Circuit breaker**: Proteção contra falhas em cascata
- **Monitoring**: Visibilidade completa do sistema

### 🔧 **Extensibilidade**
- **Plugin Architecture**: Novas etapas via IWorkflowStep
- **Strategy Pattern**: Diferentes implementações de etapas
- **Dependency Injection**: Facilita testes e manutenção

### 📊 **Observabilidade**
- **Hangfire Dashboard**: Monitoramento em tempo real
- **Structured Logging**: Logs detalhados para debugging
- **Metrics**: Métricas de performance e uso

