#!/bin/bash

# build-and-publish.sh
# Script para build e publicação da API para produção
set -e

echo "?? Iniciando build do projeto..."

# Definir variáveis
PROJECT_PATH="./SW_PortalCliente_BeachPark.API.csproj"
OUTPUT_PATH="./publish"
CONFIGURATION="Release"

# Limpar build anterior
echo "?? Limpando build anterior..."
rm -rf $OUTPUT_PATH
dotnet clean

# Restaurar dependências
echo "?? Restaurando dependências..."
dotnet restore

# Build do projeto
echo "?? Compilando projeto..."
dotnet build $PROJECT_PATH -c $CONFIGURATION --no-restore

# Publicar
echo "?? Publicando aplicação..."
dotnet publish $PROJECT_PATH \
  -c $CONFIGURATION \
  -o $OUTPUT_PATH \
  --no-build \
  --self-contained false \
  /p:PublishSingleFile=false \
  /p:PublishTrimmed=false

# Copiar arquivo .env de produção
echo "?? Copiando arquivo de configuração..."
if [ -f ".env.production" ]; then
    cp .env.production $OUTPUT_PATH/.env
    echo "? Arquivo .env.production copiado"
else
    echo "??  Aviso: Arquivo .env.production não encontrado"
fi

echo "? Build e publicação concluídos com sucesso!"
echo "?? Arquivos publicados em: $OUTPUT_PATH"
