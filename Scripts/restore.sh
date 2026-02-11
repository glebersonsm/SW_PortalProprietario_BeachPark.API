#!/bin/bash
# restore.sh
# Script para restaurar backup do sistema

set -e

# Verificar argumentos
if [ -z "$1" ]; then
    echo "? Uso: ./restore.sh <data_backup>"
    echo ""
    echo "Exemplo: ./restore.sh 20240101_120000"
    echo ""
    echo "?? Backups disponíveis:"
    ls -lh /backup/swportal/db_*.sql.gz 2>/dev/null | awk '{print "   "$9}' | sed 's/.*db_/   /' | sed 's/.sql.gz//' || echo "   Nenhum backup encontrado"
    exit 1
fi

BACKUP_DIR="/backup/swportal"
BACKUP_DATE=$1

# Configurações do banco
DB_USER="swportal_user"
DB_NAME="swportal_prod"
DB_HOST="localhost"

# Verificar se os arquivos de backup existem
if [ ! -f "$BACKUP_DIR/db_$BACKUP_DATE.sql.gz" ]; then
    echo "? Erro: Arquivo de backup do banco de dados não encontrado!"
    echo "   Procurado: $BACKUP_DIR/db_$BACKUP_DATE.sql.gz"
    exit 1
fi

echo "?? Iniciando restore do backup: $BACKUP_DATE"
echo "========================================"
echo ""

# Confirmar operação
read -p "??  Isso irá sobrescrever os dados atuais. Deseja continuar? (s/N): " -n 1 -r
echo ""
if [[ ! $REPLY =~ ^[Ss]$ ]]; then
    echo "? Operação cancelada pelo usuário"
    exit 1
fi

# Parar serviço
echo "??  Parando serviço da API..."
sudo systemctl stop swportal-api.service
sleep 2

# Criar backup antes do restore (segurança)
echo "?? Criando backup de segurança antes do restore..."
SAFETY_BACKUP_DATE=$(date +%Y%m%d_%H%M%S)
pg_dump -U $DB_USER -h $DB_HOST $DB_NAME | gzip > $BACKUP_DIR/pre_restore_$SAFETY_BACKUP_DATE.sql.gz
echo "   ? Backup de segurança criado: pre_restore_$SAFETY_BACKUP_DATE.sql.gz"

# Restore do banco de dados
echo ""
echo "?? Restaurando banco de dados..."
echo "   Dropando banco existente..."
sudo -u postgres psql -c "DROP DATABASE IF EXISTS ${DB_NAME};"

echo "   Criando novo banco..."
sudo -u postgres psql -c "CREATE DATABASE ${DB_NAME};"

echo "   Garantindo permissões..."
sudo -u postgres psql -c "GRANT ALL PRIVILEGES ON DATABASE ${DB_NAME} TO ${DB_USER};"

echo "   Restaurando dados..."
gunzip -c $BACKUP_DIR/db_$BACKUP_DATE.sql.gz | psql -U $DB_USER -h $DB_HOST $DB_NAME
echo "   ? Banco de dados restaurado"

# Restore dos arquivos (se existir)
if [ -f "$BACKUP_DIR/files_$BACKUP_DATE.tar.gz" ]; then
    echo ""
    echo "?? Restaurando arquivos..."
    
    # Fazer backup dos arquivos atuais
    echo "   Criando backup dos arquivos atuais..."
    tar -czf $BACKUP_DIR/pre_restore_files_$SAFETY_BACKUP_DATE.tar.gz \
      -C /var/www/swportal \
      wwwroot documents pdfs certidoes 2>/dev/null || true
    
    # Restaurar arquivos do backup
    echo "   Extraindo arquivos..."
    tar -xzf $BACKUP_DIR/files_$BACKUP_DATE.tar.gz -C /var/www/swportal
    echo "   ? Arquivos restaurados"
else
    echo ""
    echo "??  Arquivo de backup de arquivos não encontrado, pulando..."
fi

# Restore das configurações (se existir)
if [ -f "$BACKUP_DIR/config_$BACKUP_DATE.tar.gz" ]; then
    echo ""
    echo "?? Restaurando configurações..."
    
    # Fazer backup das configurações atuais
    echo "   Criando backup das configurações atuais..."
    tar -czf $BACKUP_DIR/pre_restore_config_$SAFETY_BACKUP_DATE.tar.gz \
      /var/www/swportal/.env \
      /var/www/swportal/appsettings.json \
      /var/www/swportal/appsettings.Production.json 2>/dev/null || true
    
    # Restaurar configurações do backup
    echo "   Extraindo configurações..."
    tar -xzf $BACKUP_DIR/config_$BACKUP_DATE.tar.gz -C /
    echo "   ? Configurações restauradas"
else
    echo ""
    echo "??  Arquivo de backup de configurações não encontrado, pulando..."
fi

# Ajustar permissões
echo ""
echo "?? Ajustando permissões..."
sudo chown -R www-data:www-data /var/www/swportal
sudo chmod -R 755 /var/www/swportal
echo "   ? Permissões ajustadas"

# Reiniciar extensões do PostgreSQL (se necessário)
echo ""
echo "?? Verificando extensões do PostgreSQL..."
sudo -u postgres psql $DB_NAME -c "CREATE EXTENSION IF NOT EXISTS unaccent;" 2>/dev/null || true
echo "   ? Extensões verificadas"

# Iniciar serviço
echo ""
echo "??  Iniciando serviço da API..."
sudo systemctl start swportal-api.service

# Aguardar inicialização
echo "? Aguardando serviço inicializar..."
sleep 5

# Verificar status
echo ""
echo "?? Status do serviço:"
sudo systemctl status swportal-api.service --no-pager || true

echo ""
echo "? Restore concluído com sucesso!"
echo ""
echo "?? Resumo:"
echo "   Backup restaurado: $BACKUP_DATE"
echo "   Banco de dados: ? Restaurado"
echo "   Arquivos: $([ -f "$BACKUP_DIR/files_$BACKUP_DATE.tar.gz" ] && echo "? Restaurados" || echo "??  Não encontrado")"
echo "   Configurações: $([ -f "$BACKUP_DIR/config_$BACKUP_DATE.tar.gz" ] && echo "? Restauradas" || echo "??  Não encontrado")"
echo ""
echo "??  Backups de segurança criados:"
echo "   - pre_restore_$SAFETY_BACKUP_DATE.sql.gz"
echo "   - pre_restore_files_$SAFETY_BACKUP_DATE.tar.gz"
echo "   - pre_restore_config_$SAFETY_BACKUP_DATE.tar.gz"
echo ""
echo "?? Para reverter este restore, use esses backups de segurança"
