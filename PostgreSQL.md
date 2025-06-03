# Configuração do PostgreSQL para Workflow Engine

Este projeto está configurado para usar PostgreSQL como banco de dados principal.

## Setup Rápido com Docker

### 1. Iniciar o PostgreSQL com Docker Compose

```bash
docker-compose up -d postgres
```

Isto irá:
- Criar um container PostgreSQL na porta 5432
- Criar os bancos de dados `WorkflowEngineDb` e `WorkflowEngineDb_Dev`
- Configurar o schema `workflow` em ambos os bancos
- Configurar as permissões necessárias

### 2. (Opcional) Iniciar o PgAdmin

```bash
docker-compose up -d pgadmin
```

Acesse o PgAdmin em: http://localhost:8080
- Email: admin@workflowengine.com
- Senha: admin

### 3. Executar as Migrações

```bash
# Navegue para o projeto da API
cd WorkflowEngine.Api

# Execute as migrações
dotnet ef database update
```

## Configuração Manual do PostgreSQL

Se você preferir instalar o PostgreSQL manualmente:

### 1. Instalar PostgreSQL

**macOS (com Homebrew):**
```bash
brew install postgresql@16
brew services start postgresql@16
```

**Ubuntu/Debian:**
```bash
sudo apt update
sudo apt install postgresql-16 postgresql-contrib
sudo systemctl start postgresql
sudo systemctl enable postgresql
```

**Windows:**
Baixe o instalador do site oficial: https://www.postgresql.org/download/windows/

### 2. Criar Usuário e Bancos de Dados

```sql
-- Conectar como postgres
sudo -u postgres psql

-- Criar os bancos de dados
CREATE DATABASE "WorkflowEngineDb" WITH OWNER = postgres;
CREATE DATABASE "WorkflowEngineDb_Dev" WITH OWNER = postgres;

-- Conectar ao banco principal
\c WorkflowEngineDb;
CREATE SCHEMA IF NOT EXISTS workflow;
ALTER SCHEMA workflow OWNER TO postgres;

-- Conectar ao banco de desenvolvimento
\c WorkflowEngineDb_Dev;
CREATE SCHEMA IF NOT EXISTS workflow;
ALTER SCHEMA workflow OWNER TO postgres;
```

## Connection Strings

### Produção
```
Host=localhost;Database=WorkflowEngineDb;Username=postgres;Password=postgres;Port=5432
```

### Desenvolvimento
```
Host=localhost;Database=WorkflowEngineDb_Dev;Username=postgres;Password=postgres;Port=5432
```

## Comandos Úteis

### Entity Framework

```bash
# Adicionar nova migração
dotnet ef migrations add NomeDaMigracao --project WorkflowEngine.Api

# Aplicar migrações
dotnet ef database update --project WorkflowEngine.Api

# Remover última migração
dotnet ef migrations remove --project WorkflowEngine.Api

# Gerar script SQL das migrações
dotnet ef migrations script --project WorkflowEngine.Api
```

### Docker

```bash
# Parar os serviços
docker-compose down

# Parar e remover volumes (CUIDADO: apaga todos os dados)
docker-compose down -v

# Ver logs do PostgreSQL
docker-compose logs postgres

# Executar comando no container PostgreSQL
docker-compose exec postgres psql -U postgres -d WorkflowEngineDb
```

## Configurações do PostgreSQL

O projeto está configurado com:

- **Schema padrão**: `workflow`
- **Naming convention**: snake_case para colunas e tabelas
- **JSON support**: Usando tipo `jsonb` para campos de configuração
- **Indexes**: Otimizados para queries comuns
- **Foreign Keys**: Com políticas adequadas de DELETE

## Troubleshooting

### Erro de conexão

1. Verifique se o PostgreSQL está rodando:
   ```bash
   docker-compose ps
   # ou
   sudo systemctl status postgresql
   ```

2. Verifique as credenciais no appsettings.json

3. Teste a conexão manualmente:
   ```bash
   psql -h localhost -U postgres -d WorkflowEngineDb
   ```

### Problemas com Migrações

1. Certifique-se de que o banco existe
2. Verifique se o schema `workflow` foi criado
3. Execute as migrações com verbose:
   ```bash
   dotnet ef database update --verbose
   ```

### Performance

Para ambientes de produção, considere ajustar:

- `shared_buffers`
- `work_mem`
- `maintenance_work_mem`
- `effective_cache_size`

Veja a documentação oficial do PostgreSQL para tuning: https://www.postgresql.org/docs/current/runtime-config.html

