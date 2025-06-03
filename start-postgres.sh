#!/bin/bash

# Script para iniciar o PostgreSQL para o Workflow Engine

echo "🐘 Iniciando PostgreSQL para Workflow Engine..."

# Verificar se Docker está instalado
if ! command -v docker &> /dev/null; then
    echo "❌ Docker não está instalado. Por favor, instale o Docker primeiro."
    exit 1
fi

# Verificar se docker-compose está disponível
if ! command -v docker-compose &> /dev/null; then
    echo "❌ docker-compose não está instalado. Por favor, instale o docker-compose primeiro."
    exit 1
fi

# Iniciar PostgreSQL
echo "📦 Iniciando container PostgreSQL..."
docker-compose up -d postgres

# Aguardar PostgreSQL inicializar
echo "⏳ Aguardando PostgreSQL inicializar..."
sleep 10

# Verificar se PostgreSQL está rodando
if docker-compose ps postgres | grep -q "Up"; then
    echo "✅ PostgreSQL iniciado com sucesso!"
    echo ""
    echo "📋 Informações de conexão:"
    echo "   Host: localhost"
    echo "   Port: 5432"
    echo "   Database: WorkflowEngineDb"
    echo "   Username: postgres"
    echo "   Password: postgres"
    echo ""
    echo "🎯 Próximos passos:"
    echo "   1. Execute as migrações: cd WorkflowEngine.Api && dotnet ef database update"
    echo "   2. Inicie a API: cd WorkflowEngine.Api && dotnet run"
    echo "   3. (Opcional) Acesse PgAdmin: docker-compose up -d pgadmin"
    echo ""
else
    echo "❌ Erro ao iniciar PostgreSQL. Verifique os logs:"
    docker-compose logs postgres
    exit 1
fi

