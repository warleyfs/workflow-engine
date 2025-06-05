# Workflow Engine Frontend

Interface web completa para o Workflow Engine com monitoramento em tempo real.

## 🚀 Funcionalidades Implementadas

### 📊 **Dashboard Principal**
- **Métricas em tempo real** via SignalR
- **Visão geral** do sistema com KPIs
- **Gráficos interativos** (Pizza, Barras)
- **Execuções recentes** com status
- **Indicador de conexão** em tempo real

### 📋 **Lista de Execuções**
- **Filtros avançados**: Status, Workflow, Data
- **Paginação** com controle de tamanho
- **Busca em tempo real**
- **Controles de execução**: Pause, Resume, Cancel
- **Logs detalhados** de cada execução

### 📈 **Métricas de Workflow**
- **Seleção por workflow** específico
- **Taxa de sucesso/falha**
- **Tempo médio de execução**
- **Distribuição por status**
- **Gráficos comparativos**

### 🔧 **Designer de Workflows**
- **Interface drag-and-drop**
- **Criação visual** de workflows
- **Configuração de steps**
- **Preview em tempo real**

### 📱 **Monitor de Execuções**
- **Auto-refresh** configurável
- **Progresso visual** de execuções
- **Controles em tempo real**
- **Detalhes completos** de cada step

### 🔔 **Notificações em Tempo Real**
- **SignalR** integrado
- **Updates automáticos** de status
- **Notificações** de novas execuções
- **Reconexão automática**

## 🛠️ Tecnologias Utilizadas

- **React 19** com TypeScript
- **Material-UI (MUI)** para componentes
- **SignalR** para tempo real
- **Recharts** para gráficos
- **React Flow** para designer visual
- **Axios** para APIs
- **Date-fns** para manipulação de datas

## 🏃‍♂️ Como Executar

### 1. Instalar Dependências
```bash
cd workflow-engine-web
npm install
```

### 2. Configurar Variáveis de Ambiente
Crie um arquivo `.env` na raiz do projeto:

```env
REACT_APP_API_URL=http://localhost:5000/api
REACT_APP_HUB_URL=http://localhost:5000/monitoring-hub
```

### 3. Executar o Frontend
```bash
npm start
```

O frontend estará disponível em: `http://localhost:3000`

### 4. Build para Produção
```bash
npm run build
```

## 📋 Estrutura de Componentes

```
src/
├── components/
│   ├── Dashboard.tsx           # Dashboard principal
│   ├── ExecutionsList.tsx      # Lista com filtros
│   ├── WorkflowMetrics.tsx     # Métricas específicas
│   ├── ExecutionMonitor.tsx    # Monitor original
│   ├── WorkflowDesigner.tsx    # Designer visual
│   ├── WorkflowList.tsx        # Lista de workflows
│   └── CustomNode.tsx          # Nós customizados
├── services/
│   ├── api.ts                  # APIs REST
│   └── signalr.ts             # Serviço SignalR
├── types/
│   ├── workflow.ts             # Tipos de workflow
│   └── monitoring.ts           # Tipos de monitoramento
└── App.tsx                     # Aplicação principal
```

## 🎨 Interface do Usuário

### Abas Principais

1. **📊 Dashboard**
   - Métricas principais
   - Status em tempo real
   - Gráficos de distribuição
   - Execuções recentes

2. **🔧 Designer**
   - Criação visual de workflows
   - Drag-and-drop de componentes
   - Configuração de steps

3. **📋 Workflows**
   - Lista de workflows definidos
   - Execução manual
   - Gerenciamento de workflows

4. **⚡ Executions**
   - Lista paginada com filtros
   - Controle em tempo real
   - Logs detalhados
   - Ações de controle

5. **📈 Metrics**
   - Métricas por workflow
   - Análise de performance
   - Gráficos comparativos

6. **📱 Monitor**
   - Visão em tempo real
   - Auto-refresh
   - Controles rápidos

## 🔄 SignalR - Tempo Real

### Eventos Suportados

- `WorkflowExecutionStatusChanged`
- `StepExecutionStatusChanged`
- `NewExecutionStarted`
- `ExecutionCompleted`
- `DashboardMetricsUpdated`

### Uso do SignalR Service

```typescript
import signalRService from './services/signalr';

// Inicializar conexão
await signalRService.start();

// Subscrever a eventos
signalRService.on('workflowStatusChanged', (update) => {
  console.log('Status changed:', update);
});

// Subscrever ao dashboard
await signalRService.subscribeToDashboard();

// Subscrever a execução específica
await signalRService.subscribeToExecution(executionId);
```

## 📊 Filtros e Pesquisa

### ExecutionsList - Filtros Disponíveis

- **Status**: Pending, Running, Completed, Failed, Cancelled, Paused
- **Workflow**: Seleção por workflow específico
- **Data de Início**: Filtro por data inicial
- **Data de Fim**: Filtro por data final
- **Paginação**: 10, 20, 50, 100 itens por página

### Exemplos de Uso

```typescript
const filters: ExecutionFilters = {
  status: 'Running',
  workflowDefinitionId: 'some-guid',
  startDate: new Date('2023-01-01'),
  endDate: new Date('2023-12-31'),
  page: 1,
  pageSize: 20
};

const executions = await monitoringApi.getExecutions(filters);
```

## 🎯 Componentes Principais

### Dashboard
- **Cards de métricas**: Total workflows, execuções, taxa de sucesso
- **Gráfico de pizza**: Distribuição por status
- **Performance**: Execuções 24h/7dias, tempo médio
- **Lista recente**: Últimas 10 execuções

### ExecutionsList
- **Tabela paginada** com todas as execuções
- **Filtros em tempo real**
- **Ações**: View, Pause, Resume, Cancel
- **Modal de detalhes** com logs completos

### WorkflowMetrics
- **Seleção por workflow**
- **Métricas específicas**
- **Gráficos comparativos**
- **Estatísticas detalhadas**

## 🔧 Configuração Avançada

### Customização de Cores

```typescript
const COLORS = {
  Pending: '#ff9800',
  Running: '#2196f3',
  Completed: '#4caf50',
  Failed: '#f44336',
  Cancelled: '#9e9e9e',
  Paused: '#ff5722',
};
```

### Configuração do Tema MUI

```typescript
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
```

## 📱 Responsividade

- **Mobile-first** design
- **Breakpoints** Material-UI
- **Navegação adaptável**
- **Tabelas responsivas**
- **Gráficos redimensionáveis**

## 🚀 Deploy

### Build para Produção

```bash
npm run build
```

### Nginx Configuration

```nginx
server {
    listen 80;
    server_name workflow-frontend.com;
    
    location / {
        root /var/www/workflow-frontend/build;
        index index.html;
        try_files $uri $uri/ /index.html;
    }
    
    location /api {
        proxy_pass http://backend:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
    
    location /monitoring-hub {
        proxy_pass http://backend:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
}
```

## 🔍 Debugging

### Console Logs
- SignalR connection events
- API call responses
- Component state changes

### Development Tools
```bash
# React DevTools
# Redux DevTools (se usado)
# Browser Network tab
```

## 📦 Dependências Principais

```json
{
  "@mui/material": "^7.1.1",
  "@mui/icons-material": "^7.1.1",
  "@mui/x-date-pickers": "^8.5.0",
  "@microsoft/signalr": "^8.0.7",
  "@xyflow/react": "^12.6.4",
  "axios": "^1.9.0",
  "recharts": "^2.15.3",
  "date-fns": "^4.1.0",
  "react": "^19.1.0",
  "typescript": "^4.9.5"
}
```

## ✨ Próximas Funcionalidades

- [ ] **Dark mode** toggle
- [ ] **Notificações** push no browser
- [ ] **Export** de dados em CSV/Excel
- [ ] **Filtros salvos** pelo usuário
- [ ] **Dashboards personalizáveis**
- [ ] **Alertas customizados**
- [ ] **PWA** para uso offline

## 🎉 **Frontend Totalmente Implementado!**

O frontend agora possui:

✅ **Dashboard completo** com métricas em tempo real
✅ **Listagem avançada** com filtros e paginação
✅ **Métricas específicas** por workflow
✅ **SignalR integrado** para updates em tempo real
✅ **Interface responsiva** e moderna
✅ **Controles de execução** em tempo real
✅ **Logs detalhados** de execuções

**Pronto para uso em produção!** 🚀

# Workflow Engine - Interface Web

Interface gráfica para criação, edição e monitoramento de workflows do Workflow Engine.

## 🚀 Funcionalidades

### 🎨 Designer de Workflows
- **Interface visual drag-and-drop** para criar workflows
- **Componentes pré-definidos**: Log Step, Email Step, Delay Step
- **Configuração de steps** com formulários dinâmicos
- **Conexões visuais** entre steps usando React Flow
- **Salvamento de workflows** com nome e descrição

### 📋 Gerenciamento de Workflows
- **Lista de workflows** com informações detalhadas
- **Execução de workflows** com entrada de dados JSON
- **Criação de workflows demo** para testes
- **Exclusão de workflows** não utilizados

### 📊 Monitor de Execuções
- **Dashboard em tempo real** com auto-refresh
- **Visualização de status** de todas as execuções
- **Controle de execução**: pausar, retomar, cancelar
- **Detalhes completos** de cada execução
- **Progresso visual** com barras de progresso
- **Métricas resumidas** por status

## 🛠️ Tecnologias Utilizadas

- **React 19** com TypeScript
- **Material-UI (MUI)** para componentes de interface
- **React Flow** para o designer visual de workflows
- **Axios** para comunicação com a API
- **React Hooks** para gerenciamento de estado

## 📦 Instalação e Configuração

### Pré-requisitos
- Node.js 18+
- npm ou yarn
- Backend do Workflow Engine rodando

### Configuração

1. **Instale as dependências:**
```bash
npm install
```

2. **Configure as variáveis de ambiente:**
Edite o arquivo `.env` se necessário:
```env
REACT_APP_API_URL=http://localhost:5000/api
REACT_APP_NAME=Workflow Engine
```

3. **Execute o desenvolvimento:**
```bash
npm start
```

A aplicação estará disponível em [http://localhost:3000](http://localhost:3000)

## 🎯 Como Usar

### 1. Designer de Workflows

1. **Acesse a aba "Designer"**
2. **Clique em "Componentes"** para abrir a barra lateral
3. **Arraste componentes** para a área de design:
   - **Log Step**: Para registrar mensagens
   - **Email Step**: Para envio de emails
   - **Delay Step**: Para adicionar atrasos
4. **Configure cada step** clicando no ícone de configuração
5. **Conecte os steps** arrastando das bolinhas de conexão
6. **Salve o workflow** clicando em "Salvar"

### 2. Execução de Workflows

1. **Acesse a aba "Workflows"**
2. **Clique no botão de play** no workflow desejado
3. **Insira os dados de entrada** em formato JSON
4. **Execute** e acompanhe na aba "Execuções"

### 3. Monitoramento

1. **Acesse a aba "Execuções"**
2. **Visualize o dashboard** com métricas resumidas
3. **Controle execuções** usando os botões de ação
4. **Veja detalhes** clicando no ícone de visualização
5. **Auto-refresh** mantém os dados atualizados

## 🏗️ Estrutura do Projeto

```
src/
├── components/           # Componentes React
│   ├── WorkflowDesigner.tsx    # Designer visual
│   ├── WorkflowList.tsx        # Lista de workflows
│   ├── ExecutionMonitor.tsx    # Monitor de execuções
│   └── CustomNode.tsx          # Nó customizado do React Flow
├── services/            # Serviços de API
│   └── api.ts          # Cliente HTTP
├── types/              # Definições TypeScript
│   └── workflow.ts     # Tipos do workflow
└── App.tsx             # Componente principal
```

## 🎨 Componentes Principais

### WorkflowDesigner
- Interface drag-and-drop
- Sidebar com componentes disponíveis
- Configuração de steps
- Salvamento de workflows

### WorkflowList
- Tabela com todos os workflows
- Execução com entrada de dados
- Gerenciamento (criar/excluir)

### ExecutionMonitor
- Dashboard em tempo real
- Controles de execução
- Detalhes completos
- Auto-refresh configurável

### CustomNode
- Nó visual para o React Flow
- Indicadores de status
- Botão de configuração
- Ícones por tipo de step

## 🔧 Scripts Disponíveis

### `npm start`
Executa a aplicação em modo de desenvolvimento.
Abra [http://localhost:3000](http://localhost:3000) para visualizar.

### `npm run build`
Cria a build de produção na pasta `build/`.
Otimizada e minificada para melhor performance.

### `npm test`
Executa os testes em modo interativo.

## 🌐 API Integration

A aplicação se comunica com o backend através dos seguintes endpoints:

- `GET /api/workflow/definitions` - Lista workflows
- `POST /api/workflow/definitions` - Cria workflow
- `POST /api/workflow/{id}/execute` - Executa workflow
- `GET /api/workflow/executions` - Lista execuções
- `GET /api/workflow/execution/{id}` - Detalhes da execução
- `POST /api/workflow/execution/{id}/pause` - Pausa execução
- `POST /api/workflow/execution/{id}/resume` - Retoma execução
- `POST /api/workflow/execution/{id}/cancel` - Cancela execução

## 🎯 Próximas Funcionalidades

- [ ] Editor de código para steps customizados
- [ ] Versionamento de workflows
- [ ] Templates de workflows
- [ ] Importação/exportação de workflows
- [ ] Métricas avançadas e relatórios
- [ ] Notificações em tempo real
- [ ] Modo escuro
- [ ] Workflow scheduling interface

## 🐛 Solução de Problemas

### Erro de CORS
Certifique-se de que o backend está configurado para aceitar requisições do frontend:
```csharp
// No Program.cs do backend
app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());
```

### API não encontrada
Verifique se:
1. O backend está rodando na porta 5000
2. A variável `REACT_APP_API_URL` está correta
3. As rotas da API estão funcionando

## 📄 Licença

Este projeto é open source e está disponível sob a licença MIT.
