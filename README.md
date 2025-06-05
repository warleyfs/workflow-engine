# Workflow Engine

Um sistema de execução de workflows distribuído construído com .NET 8, PostgreSQL e Hangfire.

## Características

- ✅ Execução assíncrona de workflows com steps sequenciais
- ✅ Sistema de retry automático com backoff exponencial
- ✅ Suporte a delays entre steps
- ✅ Estados de workflow (Pending, Running, Completed, Failed, Cancelled, Paused)
- ✅ Interface web para monitoramento (Hangfire Dashboard)
- ✅ Steps extensíveis (Log, Email, Delay)
- ✅ Persistência em PostgreSQL
- ✅ API REST completa

## Estrutura do Projeto

```
WorkflowEngine/
├── WorkflowEngine.Api/          # API REST
├── WorkflowEngine.Core/         # Lógica principal e modelos
├── WorkflowEngine.Worker/       # Worker em background
└── WorkflowEngine.Data/         # Projeto separado para dados (se necessário)
```

## Pré-requisitos

- .NET 8 SDK
- PostgreSQL 12+
- Docker (opcional, para PostgreSQL)

## Configuração

### 1. Banco de Dados PostgreSQL

#### Opção A: Docker
```bash
docker run --name workflow-postgres \
  -e POSTGRES_PASSWORD=workflow123 \
  -e POSTGRES_USER=workflow \
  -e POSTGRES_DB=workflowengine \
  -p 5432:5432 -d postgres:15
```

#### Opção B: PostgreSQL Local
Crie um banco de dados chamado `workflowengine` com um usuário `workflow`.

### 2. String de Conexão

Edite `appsettings.json` na API:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=workflowengine;Username=workflow;Password=workflow123"
  }
}
```

### 3. Executar a Aplicação

```bash
# Executar a API
cd WorkflowEngine.Api
dotnet run

# Em outro terminal, executar o Worker (opcional)
cd WorkflowEngine.Worker
dotnet run
```

A API estará disponível em: `https://localhost:7000` ou `http://localhost:5000`

Hangfire Dashboard: `http://localhost:5000/hangfire`

## Uso da API

### 1. Criar um Workflow de Demonstração

```bash
curl -X POST "http://localhost:5000/api/workflow/demo" \
  -H "Content-Type: application/json"
```

Este endpoint cria um workflow de exemplo com 4 steps:
1. Log Start
2. Wait 5 seconds
3. Send notification (com 1 minuto de delay)
4. Log End

### 2. Listar Workflows Disponíveis

```bash
curl "http://localhost:5000/api/workflow/definitions"
```

### 3. Executar um Workflow

```bash
curl -X POST "http://localhost:5000/api/workflow/{workflowDefinitionId}/execute" \
  -H "Content-Type: application/json" \
  -d '{
    "inputData": {
      "userName": "João",
      "email": "joao@exemplo.com"
    },
    "scheduledTime": null
  }'
```

### 4. Verificar Status de Execução

```bash
curl "http://localhost:5000/api/workflow/execution/{executionId}"
```

### 5. Controlar Execução

```bash
# Pausar
curl -X POST "http://localhost:5000/api/workflow/execution/{executionId}/pause"

# Retomar
curl -X POST "http://localhost:5000/api/workflow/execution/{executionId}/resume"

# Cancelar
curl -X POST "http://localhost:5000/api/workflow/execution/{executionId}/cancel"
```

## Steps Disponíveis

### LogStep
Registra mensagens no log da aplicação.

**Configuração:**
```json
{
  "Message": "Mensagem a ser logada",
  "Level": "Information" // Information, Warning, Error, Debug
}
```

### EmailStep
Simula o envio de email (não envia realmente).

**Configuração:**
```json
{
  "To": "destinatario@exemplo.com",
  "Subject": "Assunto do email",
  "Body": "Corpo do email",
  "Cc": "copia@exemplo.com",
  "Bcc": "copiaoculta@exemplo.com"
}
```

### DelayStep
Introduz um delay na execução.

**Configuração:**
```json
{
  "DelaySeconds": 5
}
```

## Estrutura de Dados

### Workflow Definition
Define a estrutura de um workflow com seus steps.

### Workflow Execution
Representa uma instância de execução de um workflow.

### Step Execution
Representa a execução de um step específico dentro de um workflow.

## Estados

### Workflow Execution Status
- `Pending`: Aguardando execução
- `Running`: Em execução
- `Completed`: Concluído com sucesso
- `Failed`: Falhou
- `Cancelled`: Cancelado
- `Paused`: Pausado

### Step Execution Status
- `Pending`: Aguardando execução
- `Running`: Em execução
- `Completed`: Concluído com sucesso
- `Failed`: Falhou
- `Skipped`: Pulado (condições não atendidas)
- `Retrying`: Tentando novamente

## Monitoramento

### Hangfire Dashboard
Acesse `http://localhost:5000/hangfire` para:
- Visualizar jobs em execução
- Ver estatísticas de jobs
- Reprocessar jobs falhados
- Ver logs detalhados

### Logs
A aplicação gera logs detalhados sobre:
- Início e fim de workflows
- Execução de steps
- Erros e retries
- Estado das execuções

## Desenvolvimento

### Criando Novos Steps

1. Implemente a interface `IWorkflowStep`:

```csharp
public class MeuCustomStep : IWorkflowStep
{
    public string StepType => "MeuCustomStep";
    
    public async Task<StepResult> ExecuteAsync(StepContext context, CancellationToken cancellationToken = default)
    {
        // Sua lógica aqui
        return StepResult.Success();
    }
    
    public async Task<bool> CanExecuteAsync(StepContext context, CancellationToken cancellationToken = default)
    {
        // Lógica de condições
        return true;
    }
    
    public async Task<StepResult> ValidateInputAsync(StepContext context, CancellationToken cancellationToken = default)
    {
        // Validação de entrada
        return StepResult.Success();
    }
}
```

2. Registre no `Program.cs`:

```csharp
builder.Services.AddScoped<MeuCustomStep>();
```

### Testando

```bash
# Executar testes (se existirem)
dotnet test

# Build de produção
dotnet build --configuration Release
```

## Deployment

### Docker

Crie um `Dockerfile` para a API:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet build --configuration Release

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WorkflowEngine.Api.dll"]
```

### Variáveis de Ambiente

```bash
CONNECTIONSTRINGS__DEFAULTCONNECTION="Host=postgres;Database=workflowengine;Username=workflow;Password=workflow123"
ASPNETCORE_ENVIRONMENT=Production
```

## Troubleshooting

### Problemas Comuns

1. **Erro de conexão com PostgreSQL**
   - Verifique se o PostgreSQL está rodando
   - Confirme a string de conexão
   - Verifique as credenciais

2. **Jobs não executam**
   - Verifique se o Hangfire está configurado corretamente
   - Confirme se o banco de dados do Hangfire foi criado
   - Verifique os logs da aplicação

3. **Steps não são encontrados**
   - Confirme se os steps estão registrados no DI container
   - Verifique se o nome do step type está correto

### Logs

Para debuggar problemas, ajuste o nível de log em `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "WorkflowEngine": "Debug",
      "Hangfire": "Information"
    }
  }
}
```

## Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## Licença

Este projeto está sob a licença MIT. Veja o arquivo `LICENSE` para mais detalhes.

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

