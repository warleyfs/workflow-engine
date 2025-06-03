#!/bin/bash

# Script para inicializar o ambiente de desenvolvimento
# Executa o backend (.NET) e frontend (React) simultaneamente

echo "🚀 Iniciando Workflow Engine - Ambiente de Desenvolvimento"
echo "================================================"

# Verifica se o PostgreSQL está rodando
echo "📋 Verificando PostgreSQL..."
if ! docker ps | grep -q postgres; then
    echo "🐳 Iniciando PostgreSQL com Docker..."
    docker-compose up -d postgres
    echo "⏳ Aguardando PostgreSQL inicializar..."
    sleep 5
else
    echo "✅ PostgreSQL já está rodando"
fi

# Verifica se as dependências do frontend estão instaladas
if [ ! -d "workflow-engine-web/node_modules" ]; then
    echo "📦 Instalando dependências do frontend..."
    cd workflow-engine-web
    npm install
    cd ..
fi

# Função para limpar processos ao sair
cleanup() {
    echo "\n🛑 Parando serviços..."
    kill $BACKEND_PID $FRONTEND_PID 2>/dev/null
    exit 0
}

# Registra função de limpeza
trap cleanup SIGINT SIGTERM

echo "\n🔧 Iniciando Backend (.NET)..."
cd WorkflowEngine.Api
dotnet run &
BACKEND_PID=$!
cd ..

echo "⏳ Aguardando backend inicializar..."
sleep 10

echo "🎨 Iniciando Frontend (React)..."
cd workflow-engine-web
npm start &
FRONTEND_PID=$!
cd ..

echo "\n🎉 Ambiente iniciado com sucesso!"
echo "📍 URLs disponíveis:"
echo "   - Frontend: http://localhost:3000"
echo "   - Backend API: http://localhost:5000"
echo "   - Swagger: http://localhost:5000/swagger"
echo "   - Hangfire: http://localhost:5000/hangfire"
echo "   - PgAdmin: http://localhost:8080 (se configurado)"
echo "\n💡 Pressione Ctrl+C para parar todos os serviços"
echo "================================================"

# Aguarda os processos
wait

