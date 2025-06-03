# Sistema de Workflow Engine

Um sistema completo de workflow engine construído com .NET 8, Entity Framework Core e Hangfire.

## 🏗️ Arquitetura

O sistema foi projetado seguindo os padrões **Chain of Responsibility** e **Strategy Pattern**, permitindo:

- **Workflows flexíveis**: Etapas podem ser reutilizadas em diferentes workflows
- **Agendamento**: Suporte a execução imediata ou agendada (minutos a dias)
- **Retry automático**: Falhas são automaticamente reprocessadas
- **Monitoramento**: Status detalhado de cada etapa
- **Pause/Resume**: Controle total sobre a execução

## 🚀 Estrutura do Projeto

```
WorkflowEngine/
├── WorkflowEngine.Core/        # Lógica principal e entidades
│   ├── Entities/              # Modelos de dados
│   ├── Enums/                 # Status e tipos
│   ├── Interfaces/            # Contratos das interfaces
│   ├── Models/                # DTOs e modelos
│   ├── Services/              # WorkflowEngine principal
│   ├── Steps/                 # Etapas implementadas
│   └── Data/                  # DbContext
├── WorkflowEngine.Api/         # API REST
├── WorkflowEngine.Data/        # (Removido - integrado ao Core)
└── WorkflowEngine.Worker/      # Worker service (futuro)
```

## 🎯 Funcionalidades Principais

### Etapas Disponíveis
1. **LogStep**: Registra mensagens no log
2. **EmailStep**: Simula envio de emails
3. **DelayStep**: Adiciona atraso configurável

### APIs Disponíveis
- `POST /api/workflow/demo` - Cria workflow de demonstração
- `GET /api/workflow/definitions` - Lista workflows disponíveis
- `POST /api/workflow/{id}/execute` - Executa workflow
- `GET /api/workflow/execution/{id}` - Status da execução
- `POST /api/workflow/execution/{id}/pause` - Pausa execução
- `POST /api/workflow/execution/{id}/resume` - Retoma execução
- `POST /api/workflow/execution/{id}/cancel` - Cancela execução

## 🏃‍♂️ Como Executar

### 1. Clone e compile o projeto
```bash
git clone <repository>
cd workflow-engine
dotnet build
```

### 2. Configure o PostgreSQL

**Opção A - Setup rápido com Docker (Recomendado):**
```bash
# Execute o script de inicialização
./start-postgres.sh

# Ou manualmente:
docker-compose up -d postgres
```

**Opção B - PostgreSQL local:**
Veja [PostgreSQL.md](PostgreSQL.md) para instruções detalhadas de instalação manual.

### 3. Execute as migrações
```bash
cd WorkflowEngine.Api
dotnet ef database update
```

### 4. Execute a API
```bash
cd WorkflowEngine.Api
dotnet run
```

### 5. Acesse os endpoints
- API: `http://localhost:5000`
- Swagger: `http://localhost:5000/swagger`
- Hangfire Dashboard: `http://localhost:5000/hangfire`
- PgAdmin (se executado): `http://localhost:8080`

## 📋 Exemplo de Uso

### 1. Criar workflow de demonstração
```bash
curl -X POST http://localhost:5000/api/workflow/demo
```

### 2. Listar workflows disponíveis
```bash
curl http://localhost:5000/api/workflow/definitions
```

### 3. Executar workflow
```bash
curl -X POST http://localhost:5000/api/workflow/{workflow-id}/execute \
  -H "Content-Type: application/json" \
  -d '{
    "inputData": {"userId": 123},
    "scheduledTime": null
  }'
```

### 4. Monitorar execução
```bash
curl http://localhost:5000/api/workflow/execution/{execution-id}
```

## 🔧 Exemplo de Workflow

O workflow de demonstração inclui:

1. **Log Start** (imediato)
2. **Wait 5 seconds** (imediato)
3. **Send notification** (delay de 1 minuto)
4. **Log End** (imediato)

## 🎨 Customização

### Criando uma Nova Etapa

```csharp
public class CustomStep : IWorkflowStep
{
    public string StepType => "CustomStep";

    public async Task<StepResult> ExecuteAsync(StepContext context, CancellationToken cancellationToken)
    {
        // Sua lógica aqui
        return StepResult.Success(new { Message = "Success!" });
    }

    public async Task<bool> CanExecuteAsync(StepContext context, CancellationToken cancellationToken)
    {
        return true; // Condições para execução
    }

    public async Task<StepResult> ValidateInputAsync(StepContext context, CancellationToken cancellationToken)
    {
        return StepResult.Success(); // Validação de entrada
    }
}
```

### Registrando a Nova Etapa

```csharp
// No Program.cs
builder.Services.AddScoped<CustomStep>();
```

## 🔍 Monitoramento

### Hangfire Dashboard
Acesse `http://localhost:5000/hangfire` para:
- Visualizar jobs em execução
- Monitorar jobs agendados
- Ver histórico de execuções
- Controlar jobs manualmente

### Status de Workflow
- **Pending**: Aguardando execução
- **Running**: Em execução
- **Completed**: Concluído com sucesso
- **Failed**: Falhou
- **Cancelled**: Cancelado
- **Paused**: Pausado

### Status de Etapa
- **Pending**: Aguardando execução
- **Running**: Em execução
- **Completed**: Concluída
- **Failed**: Falhou
- **Skipped**: Pulada (condição não atendida)
- **Retrying**: Tentativa de retry

## 🎨 Interface Gráfica

O projeto inclui uma **interface web completa** para criar, gerenciar e monitorar workflows de forma visual.

### 🚀 Funcionalidades da Interface

- **Designer Visual**: Interface drag-and-drop para criar workflows
- **Gerenciamento**: Lista, executa e exclui workflows
- **Monitor em Tempo Real**: Acompanha execuções com auto-refresh
- **Controle de Execução**: Pausa, retoma e cancela workflows
- **Dashboard**: Métricas e visualizações de status

### 🏃‍♂️ Como Executar a Interface

1. **Execute o backend:**
```bash
cd WorkflowEngine.Api
dotnet run
```

2. **Em outro terminal, execute o frontend:**
```bash
cd workflow-engine-web
npm install
npm start
```

3. **Acesse:** [http://localhost:3000](http://localhost:3000)

### 📱 Capturas de Tela

#### Designer de Workflows
- Interface drag-and-drop com componentes reutilizáveis
- Configuração visual de cada step
- Conexões entre steps com validação

#### Monitor de Execuções
- Dashboard com métricas em tempo real
- Controles de execução (play, pause, stop)
- Detalhes completos de cada execução

Para mais detalhes, veja: [workflow-engine-web/README.md](workflow-engine-web/README.md)

## 🔧 Configuração

### Banco de Dados
O projeto está configurado para usar **PostgreSQL** como banco de dados principal.

**Setup rápido com Docker:**
```bash
docker-compose up -d postgres
```

**Para mais detalhes de configuração, veja:** [PostgreSQL.md](PostgreSQL.md)

### Hangfire Storage
Usa **PostgreSQL Storage** para persistência dos jobs.
Configuração no `Program.cs`:

```csharp
builder.Services.AddHangfire(configuration =>
    configuration.UsePostgreSqlStorage(connectionString));
```

## 🎯 Casos de Uso

- **Processamento de pedidos**: Validação → Pagamento → Envio → Notificação
- **Onboarding de usuários**: Registro → Email de boas-vindas → Setup inicial
- **Relatórios**: Coleta de dados → Processamento → Geração → Envio
- **Integração de sistemas**: ETL → Validação → Transformação → Load

## 🚧 Próximos Passos

- [ ] Implementar WorkflowBuilder para criação dinâmica
- [ ] Adicionar suporte a condições complexas
- [ ] Implementar branching (if/else)
- [ ] Adicionar mais tipos de etapas
- [ ] Implementar métricas e dashboards
- [ ] Adicionar testes unitários
- [ ] Suporte a workflows paralelos

## 📄 Licença

Este projeto é open source e está disponível sob a licença MIT.

