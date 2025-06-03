# 🚀 Guia de Início Rápido - Workflow Engine

Este guia vai te ajudar a executar o Workflow Engine com interface gráfica em menos de 5 minutos!

## 📋 Pré-requisitos

✅ **Docker** (para PostgreSQL)  
✅ **.NET 8 SDK**  
✅ **Node.js 18+**  
✅ **Git**

## ⚡ Início Rápido

### 1️⃣ Clone o repositório
```bash
git clone https://github.com/warleyfs/workflow-engine.git
cd workflow-engine
```

### 2️⃣ Execute tudo de uma vez
```bash
./start-dev.sh
```

**Isso vai:**
- ✅ Iniciar PostgreSQL com Docker
- ✅ Instalar dependências do frontend
- ✅ Executar o backend (.NET)
- ✅ Executar o frontend (React)

### 3️⃣ Acesse a interface

🌐 **Frontend:** [http://localhost:3000](http://localhost:3000)

## 🎯 Primeiros Passos na Interface

### 1. Criar um Workflow
1. **Clique na aba "Designer"**
2. **Clique em "Componentes"** para ver a barra lateral
3. **Arraste** um "Log Step" para a área de design
4. **Configure** clicando no ícone de engrenagem
5. **Salve** o workflow

### 2. Executar um Workflow
1. **Vá para a aba "Workflows"**
2. **Clique no botão ▶️** do workflow
3. **Insira dados JSON** (ex: `{"message": "Hello World"}`)
4. **Execute**

### 3. Monitorar Execução
1. **Vá para a aba "Execuções"**
2. **Veja o status** em tempo real
3. **Clique no ícone 👁️** para detalhes

## 🔗 URLs Úteis

| Serviço | URL | Descrição |
|---------|-----|-----------|
| 🎨 **Frontend** | [localhost:3000](http://localhost:3000) | Interface gráfica principal |
| 🔧 **API** | [localhost:5000](http://localhost:5000) | Backend REST API |
| 📚 **Swagger** | [localhost:5000/swagger](http://localhost:5000/swagger) | Documentação da API |
| 📊 **Hangfire** | [localhost:5000/hangfire](http://localhost:5000/hangfire) | Dashboard de jobs |
| 🗄️ **PgAdmin** | [localhost:8080](http://localhost:8080) | Admin do PostgreSQL |

## 🛠️ Executação Manual (Alternativa)

Se preferir executar manualmente:

### Backend
```bash
# Terminal 1 - PostgreSQL
docker-compose up postgres

# Terminal 2 - Backend
cd WorkflowEngine.Api
dotnet run
```

### Frontend
```bash
# Terminal 3 - Frontend
cd workflow-engine-web
npm install
npm start
```

## 🎨 Componentes Disponíveis

| Componente | Descrição | Configuração |
|------------|-----------|-------------|
| 📝 **Log Step** | Registra mensagens | `message`: Texto do log |
| 📧 **Email Step** | Simula envio de email | `to`, `subject`, `body` |
| ⏰ **Delay Step** | Adiciona atraso | `seconds`: Tempo em segundos |

## 🐛 Problemas Comuns

### ❌ Erro de CORS
**Solução:** Certifique-se que o backend está rodando na porta 5000

### ❌ PostgreSQL não conecta
**Solução:** Execute `docker-compose up postgres`

### ❌ Frontend não carrega
**Solução:** 
1. Verifique se o Node.js está instalado
2. Execute `npm install` na pasta `workflow-engine-web`

### ❌ API não responde
**Solução:**
1. Verifique se o .NET 8 está instalado
2. Execute `dotnet run` na pasta `WorkflowEngine.Api`

## 🎯 Próximos Passos

✅ **Experimente criar workflows complexos**  
✅ **Teste diferentes tipos de steps**  
✅ **Monitore execuções em tempo real**  
✅ **Explore o dashboard do Hangfire**  
✅ **Veja a documentação da API no Swagger**

## 📖 Documentação Completa

- 📘 [README Principal](README.md)
- 🎨 [Documentação do Frontend](workflow-engine-web/README.md)
- 🗄️ [Configuração PostgreSQL](PostgreSQL.md)

## 🆘 Precisa de Ajuda?

- 📁 **Verifique os logs** no terminal
- 🔍 **Consulte a documentação** completa
- 🐛 **Reporte bugs** no GitHub Issues

---

**🎉 Pronto! Seu Workflow Engine está funcionando!**

Divirta-se criando workflows incríveis! 🚀

