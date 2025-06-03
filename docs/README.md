# Documentação de Arquitetura - Workflow Engine

Esta pasta contém a documentação detalhada da arquitetura do sistema Workflow Engine.

## 📄 Arquivos Disponíveis

### 1. `architecture-c4.md`
Documentação completa usando o **C4 Model** com 4 níveis:
- **Context Diagram**: Visão geral do sistema e atores
- **Container Diagram**: Principais componentes e tecnologias
- **Component Diagram**: Detalhes internos do Workflow Engine Core
- **Code Diagram**: Fluxo de execução e sequência de chamadas

### 2. Diagramas Mermaid

#### `architecture-diagram.mermaid`
Diagrama principal da arquitetura mostrando:
- Camadas do sistema
- Fluxo de dados
- Interações entre componentes

#### `workflow-execution-flow.mermaid`
Diagrama de sequência detalhado mostrando:
- Criação de workflows
- Execução de etapas
- Processamento em background
- Monitoramento e controle

#### `data-model.mermaid`
Modelo de dados Entity-Relationship mostrando:
- Entidades principais
- Relacionamentos
- Status e enums

## 🔍 Como Visualizar os Diagramas

### Opção 1: GitHub/GitLab
Os arquivos `.mermaid` são automaticamente renderizados no GitHub/GitLab.

### Opção 2: VS Code
Instale a extensão "Mermaid Preview" para visualizar os diagramas.

### Opção 3: Online
Cole o conteúdo dos arquivos `.mermaid` em:
- [Mermaid Live Editor](https://mermaid-js.github.io/mermaid-live-editor/)
- [Draw.io](https://app.diagrams.net/) (suporta Mermaid)

### Opção 4: Documentação Markdown
O arquivo `architecture-c4.md` contém todos os diagramas integrados.

## 🎯 Principais Conceitos Arquiteturais

### **Separation of Concerns**
- **API Layer**: Exposição REST
- **Core Engine**: Lógica de negócio
- **Background Processing**: Execução assíncrona
- **Data Layer**: Persistência

### **Design Patterns Utilizados**
- **Strategy Pattern**: Diferentes implementações de etapas
- **Chain of Responsibility**: Sequenciamento de etapas
- **Repository Pattern**: Abstração de dados
- **Dependency Injection**: Inversão de controle

### **Características de Qualidade**
- ✅ **Escalabilidade**: Horizontal e vertical
- ✅ **Resiliência**: Retry automático e circuit breaker
- ✅ **Observabilidade**: Logs, métricas e monitoramento
- ✅ **Extensibilidade**: Plugin architecture
- ✅ **Testabilidade**: Interfaces bem definidas

## 📚 Referências

- [C4 Model](https://c4model.com/)
- [Mermaid Documentation](https://mermaid-js.github.io/mermaid/)
- [.NET Architecture Guides](https://docs.microsoft.com/en-us/dotnet/architecture/)
- [Hangfire Documentation](https://docs.hangfire.io/)

