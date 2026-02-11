#!/bin/bash
# backup.sh
# Script para backup automático do sistema

set -e

# Variáveis
BACKUP_DIR="/backup/swportal"
DATE=$(date +%Y%m%d_%H%M%S)
RETENTION_DAYS=7

# Configurações do banco
DB_USER="swportal_user"
DB_NAME="swportal_prod"
DB_HOST="localhost"

# Criar diretório de backup
mkdir -p $BACKUP_DIR

echo "?? Iniciando backup - $(date)"
echo "========================================"
echo ""

# Backup do banco de dados PostgreSQL
echo "?? Realizando backup do banco de dados..."
PGPASSWORD=$DB_PASSWORD pg_dump -U $DB_USER -h $DB_HOST $DB_NAME | gzip > $BACKUP_DIR/db_$DATE.sql.gz

if [ -f "$BACKUP_DIR/db_$DATE.sql.gz" ]; then
    DB_SIZE=$(du -h "$BACKUP_DIR/db_$DATE.sql.gz" | cut -f1)
    echo "   ? Banco de dados: $DB_SIZE"
else
    echo "   ? Erro ao fazer backup do banco de dados!"
    exit 1
fi

# Backup dos arquivos (wwwroot, documents, pdfs, certidoes)
echo "?? Realizando backup dos arquivos..."
tar -czf $BACKUP_DIR/files_$DATE.tar.gz \
  -C /var/www/swportal \
  wwwroot \
  documents \
  pdfs \
  certidoes 2>/dev/null || true

if [ -f "$BACKUP_DIR/files_$DATE.tar.gz" ]; then
    FILES_SIZE=$(du -h "$BACKUP_DIR/files_$DATE.tar.gz" | cut -f1)
    echo "   ? Arquivos: $FILES_SIZE"
else
    echo "   ??  Aviso: Alguns arquivos podem não ter sido copiados"
fi

# Backup das configurações
echo "?? Realizando backup das configurações..."
tar -czf $BACKUP_DIR/config_$DATE.tar.gz \
  /var/www/swportal/.env \
  /var/www/swportal/appsettings.json \
  /var/www/swportal/appsettings.Production.json \
  /etc/nginx/sites-available/swportal-api \
  /etc/systemd/system/swportal-api.service 2>/dev/null || true

if [ -f "$BACKUP_DIR/config_$DATE.tar.gz" ]; then
    CONFIG_SIZE=$(du -h "$BACKUP_DIR/config_$DATE.tar.gz" | cut -f1)
    echo "   ? Configurações: $CONFIG_SIZE"
else
    echo "   ??  Aviso: Algumas configurações podem não ter sido copiadas"
fi

# Manter apenas últimos X dias
echo ""
echo "?? Limpando backups antigos (mantendo últimos $RETENTION_DAYS dias)..."
find $BACKUP_DIR -name "*.gz" -mtime +$RETENTION_DAYS -type f -delete
REMOVED_COUNT=$(find $BACKUP_DIR -name "*.gz" -mtime +$RETENTION_DAYS -type f 2>/dev/null | wc -l)
echo "   ???  Removidos: $REMOVED_COUNT arquivo(s)"

echo ""
echo "? Backup concluído: $BACKUP_DIR"
echo ""
echo "?? Resumo do backup:"
echo "   Data/Hora: $(date)"
echo "   Localização: $BACKUP_DIR"
echo "   Banco de dados: db_$DATE.sql.gz ($DB_SIZE)"
echo "   Arquivos: files_$DATE.tar.gz ($FILES_SIZE)"
echo "   Configurações: config_$DATE.tar.gz ($CONFIG_SIZE)"
echo ""
echo "?? Tamanho total do diretório de backup:"
du -sh $BACKUP_DIR

# Criar arquivo de log
echo "$(date): Backup concluído - db_$DATE.sql.gz, files_$DATE.tar.gz, config_$DATE.tar.gz" >> $BACKUP_DIR/backup.log
