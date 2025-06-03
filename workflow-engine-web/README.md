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
