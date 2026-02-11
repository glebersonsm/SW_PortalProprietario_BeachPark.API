#!/bin/bash
# deploy.sh
# Script para deploy automatizado da API no servidor Linux

set -e

echo "?? Iniciando deploy da API..."

# Variáveis (EDITAR CONFORME SEU AMBIENTE)
REMOTE_USER="seu_usuario"
REMOTE_HOST="seu_servidor.com"
REMOTE_PATH="/var/www/swportal"
LOCAL_PUBLISH_PATH="./publish"

# Verificar se as variáveis foram configuradas
if [ "$REMOTE_USER" = "seu_usuario" ] || [ "$REMOTE_HOST" = "seu_servidor.com" ]; then
    echo "? Erro: Configure as variáveis REMOTE_USER e REMOTE_HOST no script antes de executar!"
    exit 1
fi

# Build local
echo "?? Executando build..."
./scripts/build-and-publish.sh

# Verificar se o build foi bem-sucedido
if [ ! -d "$LOCAL_PUBLISH_PATH" ]; then
    echo "? Erro: Diretório de publicação não encontrado!"
    exit 1
fi

# Upload para servidor
echo "?? Enviando arquivos para servidor..."
rsync -avz --delete \
  --exclude='appsettings.Development.json' \
  --exclude='.env.development' \
  --exclude='*.pdb' \
  --progress \
  $LOCAL_PUBLISH_PATH/ \
  $REMOTE_USER@$REMOTE_HOST:$REMOTE_PATH/

# Comandos remotos
echo "?? Executando comandos no servidor..."
ssh $REMOTE_USER@$REMOTE_HOST << 'EOF'
  echo "??  Parando serviço..."
  sudo systemctl stop swportal-api.service
  
  echo "?? Criando diretórios necessários..."
  sudo mkdir -p /var/www/swportal/wwwroot/{images/grupos,documents,pdfs/{comunicacoes,boletos},certidoes/{modelos,pdf,contratos}}
  
  echo "?? Ajustando permissões..."
  sudo chown -R www-data:www-data /var/www/swportal
  sudo chmod -R 755 /var/www/swportal
  
  echo "??  Iniciando serviço..."
  sudo systemctl start swportal-api.service
  
  echo "? Aguardando serviço inicializar..."
  sleep 5
  
  echo "?? Status do serviço:"
  sudo systemctl status swportal-api.service --no-pager || true
  
  echo "? Serviço reiniciado com sucesso!"
EOF

echo ""
echo "? Deploy concluído com sucesso!"
echo "?? API disponível em: https://$REMOTE_HOST"
echo ""
echo "?? Próximos passos:"
echo "  - Verificar logs: ssh $REMOTE_USER@$REMOTE_HOST 'sudo journalctl -u swportal-api.service -f'"
echo "  - Testar API: curl https://$REMOTE_HOST/health"
