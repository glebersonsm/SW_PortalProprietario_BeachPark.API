# ?? Guia Completo: Deploy da API .NET 8 em Servidor Linux

## ?? Índice

- [Pré-requisitos](#-pré-requisitos)
- [1. Preparar o Projeto](#-1-preparar-o-projeto-para-produção)
- [2. Build e Publicação](#-2-build-e-publicação)
- [3. Configurar Systemd Service](#-3-configurar-systemd-service)
- [4. Configurar Nginx](#-4-configurar-nginx-como-reverse-proxy)
- [5. Configurar SSL](#-5-configurar-ssl-com-lets-encrypt)
- [6. Configurar PostgreSQL](#-6-configurar-postgresql)
- [7. Configurar Redis](#-7-configurar-redis)
- [8. Configurar RabbitMQ](#-8-configurar-rabbitmq)
- [9. Deploy Automatizado](#-9-script-de-deploy-automatizado)
- [10. Monitoramento](#-10-monitoramento-e-logs)
- [11. Backup](#-11-backup-e-restore)
- [12. Troubleshooting](#-12-troubleshooting)

---

## ?? Pré-requisitos

### 1. Instalação no Servidor Linux (Ubuntu/Debian)

```bash
# Atualizar sistema
sudo apt update && sudo apt upgrade -y

# Instalar .NET 8 SDK/Runtime
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
sudo chmod +x dotnet-install.sh
sudo ./dotnet-install.sh --channel 8.0 --install-dir /usr/share/dotnet
sudo ln -s /usr/share/dotnet/dotnet /usr/local/bin

# Verificar instalação
dotnet --version

# Instalar Nginx
sudo apt install nginx -y

# Instalar PostgreSQL
sudo apt install postgresql postgresql-contrib -y

# Instalar Redis
sudo apt install redis-server -y

# Instalar RabbitMQ
curl -s https://packagecloud.io/install/repositories/rabbitmq/rabbitmq-server/script.deb.sh | sudo bash
sudo apt install rabbitmq-server -y
sudo systemctl enable rabbitmq-server
sudo systemctl start rabbitmq-server
```

### 2. Verificar Serviços

```bash
# Verificar status dos serviços
sudo systemctl status nginx
sudo systemctl status postgresql
sudo systemctl status redis-server
sudo systemctl status rabbitmq-server
```

---

## ?? 1. Preparar o Projeto para Produção

### Criar arquivo `.env.production`

Crie o arquivo `.env.production` na raiz do projeto:

```env
# ============================================
# AMBIENTE
# ============================================
TIPO_AMBIENTE=PRD
ASPNETCORE_ENVIRONMENT=Production

# ============================================
# JWT
# ============================================
JWT_KEY=SuaChaveSuperSeguraComMinimo32Caracteres!Production
JWT_ISSUER=SW_Portal_Proprietario_API
JWT_AUDIENCE=SW_Portal_Proprietario_API

# ============================================
# CONNECTION STRINGS
# ============================================
DEFAULT_CONNECTION=Host=localhost;Port=5432;Database=swportal_prod;Username=swportal_user;Password=SenhaSegura123!
CM_CONNECTION=Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=seu_host_oracle)(PORT=1521)))(CONNECT_DATA=(SERVICE_NAME=SEU_SERVICE)));User ID=usuario;Password=senha;Connection Timeout=30
REDIS_CONNECTION=localhost:6379,password=SenhaRedis123!

# ============================================
# REDIS
# ============================================
REDIS_HOST=localhost
REDIS_PORT=6379
REDIS_PASSWORD=SenhaRedis123!
REDIS_DATABASE=0

# ============================================
# RABBITMQ
# ============================================
RABBITMQ_HOST=localhost
RABBITMQ_PORT=5672
RABBITMQ_USER=guest
RABBITMQ_PASS=guest

# ============================================
# SMTP (Gerenciado pelo ParametroSistema)
# ============================================
# Nota: As configurações de SMTP agora são gerenciadas 
# pelo ParametroSistema no banco de dados.
# As variáveis abaixo são mantidas apenas para referência.

# ============================================
# CORS
# ============================================
ORIGENS_PERMITIDAS=https://seudominio.com|https://www.seudominio.com

# ============================================
# PATHS
# ============================================
WWWROOT_PATH=/var/www/swportal/wwwroot
WWWROOT_IMAGE_PATH=/var/www/swportal/wwwroot/images
WWWROOT_GRUPO_IMAGE_PATH=/var/www/swportal/wwwroot/images/grupos
NEW_DOCUMENTS_FILE_PATH=/var/www/swportal/documents
PATH_GERACAO_PDF_COMUNICACOES_GERAIS=/var/www/swportal/pdfs/comunicacoes
PATH_GERACAO_BOLETOS=/var/www/swportal/pdfs/boletos
CERTIDOES_MODELOS_PATH=/var/www/swportal/certidoes/modelos
CERTIDOES_GERACAO_PDF_PATH=/var/www/swportal/certidoes/pdf
CERTIDOES_GERACAO_PDF_CONTRATO_PATH=/var/www/swportal/certidoes/contratos

# ============================================
# OUTRAS CONFIGURAÇÕES
# ============================================
USUARIO_SISTEMA_ID=1
EMPRESA_SW_PORTAL_ID=1
UPDATE_DATABASE=true
UPDATE_FRAMEWORK=false
BLOQUEAR_CRIACAO_ADM_FORA_DEBUG=true
PROGRAM_ID=PORTALPROPMVC_

# Configurações de E-mail (Produção)
ENVIAR_EMAIL_APENAS_PARA_DESTINATARIOS_PERMITIDOS=false
DESTINATARIO_EMAIL_PERMITIDO=

# Configurações de SMS (Produção)
ENVIAR_SMS_APENAS_PARA_NUMERO_PERMITIDO=false
NUMERO_SMS_PERMITIDO=

# Configurações de Fila
SEND_OPERATIONS_TO_LOG_QUEUE=true
SAVE_LOG_FROM_QUEUE=true
SEND_EMAIL_FROM_QUEUE=true
AUTOMATIC_EMAIL_ENABLED=true

# Tempos de espera (em minutos/segundos)
WAIT_SEND_LOG_QUEUE=1
WAIT_SAVE_LOG_QUEUE=1
WAIT_SEND_EMAIL_QUEUE=1
WAIT_SEARCH_PIX=30
WAIT_FINALIZE_CARTAO=30

# Audit Log
AUDIT_LOG_CONSUMER_CONCURRENCY=5
AUDIT_LOG_RETRY_ATTEMPTS=3
AUDIT_LOG_RETRY_DELAY_SECONDS=5

# Integração
INTEGRADO_COM=eSolution
CONTROLE_USUARIO_SFE=false
CONTROLE_USUARIO_ACCESS_CENTER=true

# Tags
TAG_GERAL_ID=1
TAG_TROPICAL_ID=2
TAG_HOMES_ID=3

# Broker
USE_BROKER_TYPE=NovaXSBrokerProduction
PODE_INFORMAR_PIX_SCP=true
TIME_SHARING_ATIVADO=false
MULTIPROPRIEDADE_ATIVADA=true
```

### Atualizar `.csproj`

Certifique-se de que o arquivo `.csproj` está configurado para copiar o `.env`:

```xml
<!-- SW_PortalCliente_BeachPark.API.csproj -->
<ItemGroup>
  <None Update=".env">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
  <None Update=".env.production">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
  <None Update="appsettings.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
  <None Update="appsettings.Production.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

---

## ??? 2. Build e Publicação

### Script de Build (`scripts/build-and-publish.sh`)

Crie o arquivo `scripts/build-and-publish.sh`:

```bash
#!/bin/bash

# build-and-publish.sh
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
cp .env.production $OUTPUT_PATH/.env

echo "? Build e publicação concluídos com sucesso!"
echo "?? Arquivos publicados em: $OUTPUT_PATH"
```

### Executar Build

```bash
chmod +x scripts/build-and-publish.sh
./scripts/build-and-publish.sh
```

---

## ?? 3. Configurar Systemd Service

### Criar diretório de instalação

```bash
sudo mkdir -p /var/www/swportal
sudo chown -R www-data:www-data /var/www/swportal
```

### Criar arquivo de serviço

```bash
sudo nano /etc/systemd/system/swportal-api.service
```

Conteúdo do arquivo:

```ini
[Unit]
Description=SW Portal Proprietario API
After=network.target postgresql.service redis.service rabbitmq-server.service

[Service]
Type=notify
WorkingDirectory=/var/www/swportal
ExecStart=/usr/local/bin/dotnet /var/www/swportal/SW_PortalCliente_BeachPark.API.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=swportal-api
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
Environment=ASPNETCORE_URLS=http://localhost:5000

# Limites de recursos
LimitNOFILE=65536
LimitNPROC=4096

# Segurança
NoNewPrivileges=true
PrivateTmp=true

[Install]
WantedBy=multi-user.target
```

### Habilitar e iniciar o serviço

```bash
# Recarregar configurações do systemd
sudo systemctl daemon-reload

# Habilitar para iniciar com o sistema
sudo systemctl enable swportal-api.service

# Iniciar o serviço
sudo systemctl start swportal-api.service

# Verificar status
sudo systemctl status swportal-api.service

# Ver logs
sudo journalctl -u swportal-api.service -f
```

---

## ?? 4. Configurar Nginx como Reverse Proxy

### Criar configuração do Nginx

```bash
sudo nano /etc/nginx/sites-available/swportal-api
```

Conteúdo do arquivo:

```nginx
# /etc/nginx/sites-available/swportal-api

upstream swportal_api {
    server 127.0.0.1:5000;
    keepalive 32;
}

# Redirecionar HTTP para HTTPS
server {
    listen 80;
    listen [::]:80;
    server_name api.seudominio.com;
    
    # Para Let's Encrypt
    location /.well-known/acme-challenge/ {
        root /var/www/certbot;
    }
    
    # Redirecionar para HTTPS
    location / {
        return 301 https://$server_name$request_uri;
    }
}

server {
    listen 443 ssl http2;
    listen [::]:443 ssl http2;
    server_name api.seudominio.com;

    # SSL Configuration (Let's Encrypt)
    ssl_certificate /etc/letsencrypt/live/api.seudominio.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/api.seudominio.com/privkey.pem;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;
    ssl_prefer_server_ciphers on;

    # Security Headers
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header Referrer-Policy "no-referrer-when-downgrade" always;

    # Client body size (ajustar conforme necessário)
    client_max_body_size 100M;
    client_body_timeout 300s;

    # Logging
    access_log /var/log/nginx/swportal-api-access.log;
    error_log /var/log/nginx/swportal-api-error.log warn;

    # Proxy settings
    location / {
        proxy_pass http://swportal_api;
        proxy_http_version 1.1;
        
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header X-Forwarded-Host $host;
        proxy_set_header X-Forwarded-Port $server_port;
        
        proxy_cache_bypass $http_upgrade;
        proxy_buffering off;
        proxy_read_timeout 300s;
        proxy_connect_timeout 75s;
    }

    # Arquivos estáticos
    location ~ ^/(images|documents|pdfs|certidoes)/ {
        root /var/www/swportal/wwwroot;
        expires 30d;
        add_header Cache-Control "public, immutable";
    }

    # Health check endpoint
    location /health {
        access_log off;
        proxy_pass http://swportal_api;
    }
}
```

### Habilitar site e testar

```bash
# Criar link simbólico
sudo ln -s /etc/nginx/sites-available/swportal-api /etc/nginx/sites-enabled/

# Remover configuração padrão (opcional)
sudo rm /etc/nginx/sites-enabled/default

# Testar configuração
sudo nginx -t

# Recarregar Nginx
sudo systemctl reload nginx
```

---

## ?? 5. Configurar SSL com Let's Encrypt

```bash
# Instalar Certbot
sudo apt install certbot python3-certbot-nginx -y

# Obter certificado (certifique-se de que o DNS está apontando para o servidor)
sudo certbot --nginx -d api.seudominio.com

# Renovação automática (já configurado pelo certbot)
sudo systemctl status certbot.timer

# Testar renovação
sudo certbot renew --dry-run
```

---

## ??? 6. Configurar PostgreSQL

```bash
# Acessar PostgreSQL
sudo -u postgres psql

# Criar banco e usuário
CREATE DATABASE swportal_prod;
CREATE USER swportal_user WITH ENCRYPTED PASSWORD 'SenhaSegura123!';
GRANT ALL PRIVILEGES ON DATABASE swportal_prod TO swportal_user;

# Instalar extensão unaccent (necessária)
\c swportal_prod
CREATE EXTENSION IF NOT EXISTS unaccent;
\q
```

### Configurar acesso remoto (opcional)

```bash
# Editar postgresql.conf
sudo nano /etc/postgresql/*/main/postgresql.conf
# Adicionar ou modificar:
# listen_addresses = '*'

# Editar pg_hba.conf
sudo nano /etc/postgresql/*/main/pg_hba.conf
# Adicionar:
# host all all 0.0.0.0/0 md5

# Reiniciar PostgreSQL
sudo systemctl restart postgresql
```

### Backup do PostgreSQL

```bash
# Backup manual
pg_dump -U swportal_user -h localhost swportal_prod | gzip > backup_$(date +%Y%m%d).sql.gz

# Restore
gunzip -c backup_20240101.sql.gz | psql -U swportal_user -h localhost swportal_prod
```

---

## ?? 7. Configurar Redis

```bash
# Configurar senha do Redis
sudo nano /etc/redis/redis.conf

# Procurar e descomentar/modificar:
# requirepass SenhaRedis123!

# Reiniciar Redis
sudo systemctl restart redis-server

# Testar conexão
redis-cli
# No prompt do Redis:
# AUTH SenhaRedis123!
# PING
# (deve retornar PONG)
```

---

## ?? 8. Configurar RabbitMQ

```bash
# Habilitar painel de gerenciamento
sudo rabbitmq-plugins enable rabbitmq_management

# Criar usuário admin
sudo rabbitmqctl add_user admin SenhaRabbitMQ123!
sudo rabbitmqctl set_user_tags admin administrator
sudo rabbitmqctl set_permissions -p / admin ".*" ".*" ".*"

# Acessar painel: http://seu-servidor:15672
# Usuário: admin / Senha: SenhaRabbitMQ123!

# Listar filas
sudo rabbitmqctl list_queues

# Verificar status
sudo rabbitmqctl status
```

---

## ?? 9. Script de Deploy Automatizado

### Criar script de deploy (`scripts/deploy.sh`)

```bash
#!/bin/bash
# deploy.sh

set -e

echo "?? Iniciando deploy da API..."

# Variáveis
REMOTE_USER="seu_usuario"
REMOTE_HOST="seu_servidor.com"
REMOTE_PATH="/var/www/swportal"
LOCAL_PUBLISH_PATH="./publish"

# Build local
echo "?? Executando build..."
./scripts/build-and-publish.sh

# Upload para servidor
echo "?? Enviando arquivos para servidor..."
rsync -avz --delete \
  --exclude='appsettings.Development.json' \
  --exclude='.env.development' \
  $LOCAL_PUBLISH_PATH/ \
  $REMOTE_USER@$REMOTE_HOST:$REMOTE_PATH/

# Comandos remotos
echo "?? Reiniciando serviço no servidor..."
ssh $REMOTE_USER@$REMOTE_HOST << 'EOF'
  # Parar o serviço
  sudo systemctl stop swportal-api.service
  
  # Criar diretórios necessários
  sudo mkdir -p /var/www/swportal/wwwroot/{images/grupos,documents,pdfs/{comunicacoes,boletos},certidoes/{modelos,pdf,contratos}}
  
  # Ajustar permissões
  sudo chown -R www-data:www-data /var/www/swportal
  sudo chmod -R 755 /var/www/swportal
  
  # Iniciar o serviço
  sudo systemctl start swportal-api.service
  
  # Verificar status
  sudo systemctl status swportal-api.service --no-pager
  
  echo "? Serviço reiniciado com sucesso!"
EOF

echo "? Deploy concluído com sucesso!"
echo "?? API disponível em: https://api.seudominio.com"
```

### Tornar executável

```bash
chmod +x scripts/deploy.sh
```

### Executar deploy

```bash
./scripts/deploy.sh
```

---

## ?? 10. Monitoramento e Logs

### Visualizar logs da aplicação

```bash
# Logs em tempo real
sudo journalctl -u swportal-api.service -f

# Logs das últimas 100 linhas
sudo journalctl -u swportal-api.service -n 100

# Logs de hoje
sudo journalctl -u swportal-api.service --since today

# Logs de ontem
sudo journalctl -u swportal-api.service --since yesterday

# Logs com filtro de erro
sudo journalctl -u swportal-api.service | grep -i error

# Logs com filtro de warning
sudo journalctl -u swportal-api.service | grep -i warning
```

### Logs do Nginx

```bash
# Access logs
sudo tail -f /var/log/nginx/swportal-api-access.log

# Error logs
sudo tail -f /var/log/nginx/swportal-api-error.log

# Últimas 100 linhas de erro
sudo tail -n 100 /var/log/nginx/swportal-api-error.log
```

### Script de monitoramento (`scripts/monitor.sh`)

```bash
#!/bin/bash
# monitor.sh

echo "?? Status dos Serviços"
echo "===================="
echo ""

echo "?? API:"
sudo systemctl status swportal-api.service --no-pager | head -n 5

echo ""
echo "?? Nginx:"
sudo systemctl status nginx --no-pager | head -n 5

echo ""
echo "?? PostgreSQL:"
sudo systemctl status postgresql --no-pager | head -n 5

echo ""
echo "?? Redis:"
sudo systemctl status redis-server --no-pager | head -n 5

echo ""
echo "?? RabbitMQ:"
sudo systemctl status rabbitmq-server --no-pager | head -n 5

echo ""
echo "?? Uso de Recursos:"
echo "===================="
echo "Memória:"
free -h

echo ""
echo "Disco:"
df -h /var/www/swportal

echo ""
echo "CPU:"
top -bn1 | grep "Cpu(s)" | sed "s/.*, *\([0-9.]*\)%* id.*/\1/" | awk '{print "Uso: " 100 - $1"%"}'

echo ""
echo "?? Conectividade:"
echo "===================="
curl -s -o /dev/null -w "Status API: %{http_code}\n" https://api.seudominio.com/health
```

### Tornar executável e executar

```bash
chmod +x scripts/monitor.sh
./scripts/monitor.sh
```

---

## ?? 11. Backup e Restore

### Script de Backup (`scripts/backup.sh`)

```bash
#!/bin/bash
# backup.sh

set -e

BACKUP_DIR="/backup/swportal"
DATE=$(date +%Y%m%d_%H%M%S)

# Criar diretório de backup
mkdir -p $BACKUP_DIR

echo "?? Iniciando backup..."

# Backup do banco de dados PostgreSQL
echo "?? Backup do banco de dados..."
pg_dump -U swportal_user -h localhost swportal_prod | gzip > $BACKUP_DIR/db_$DATE.sql.gz

# Backup dos arquivos (wwwroot, documents, pdfs, certidoes)
echo "?? Backup dos arquivos..."
tar -czf $BACKUP_DIR/files_$DATE.tar.gz \
  /var/www/swportal/wwwroot \
  /var/www/swportal/documents \
  /var/www/swportal/pdfs \
  /var/www/swportal/certidoes

# Backup das configurações
echo "?? Backup das configurações..."
tar -czf $BACKUP_DIR/config_$DATE.tar.gz \
  /var/www/swportal/.env \
  /var/www/swportal/appsettings.json \
  /var/www/swportal/appsettings.Production.json \
  /etc/nginx/sites-available/swportal-api \
  /etc/systemd/system/swportal-api.service

# Manter apenas últimos 7 dias
echo "?? Limpando backups antigos..."
find $BACKUP_DIR -name "*.gz" -mtime +7 -delete

echo "? Backup concluído: $BACKUP_DIR"
echo "?? Tamanho total:"
du -sh $BACKUP_DIR
```

### Script de Restore (`scripts/restore.sh`)

```bash
#!/bin/bash
# restore.sh

set -e

if [ -z "$1" ]; then
    echo "? Uso: ./restore.sh <data_backup>"
    echo "Exemplo: ./restore.sh 20240101_120000"
    exit 1
fi

BACKUP_DIR="/backup/swportal"
BACKUP_DATE=$1

echo "?? Iniciando restore do backup: $BACKUP_DATE"

# Parar serviço
echo "??  Parando serviço..."
sudo systemctl stop swportal-api.service

# Restore do banco de dados
echo "?? Restore do banco de dados..."
gunzip -c $BACKUP_DIR/db_$BACKUP_DATE.sql.gz | psql -U swportal_user -h localhost swportal_prod

# Restore dos arquivos
echo "?? Restore dos arquivos..."
tar -xzf $BACKUP_DIR/files_$BACKUP_DATE.tar.gz -C /

# Ajustar permissões
echo "?? Ajustando permissões..."
sudo chown -R www-data:www-data /var/www/swportal
sudo chmod -R 755 /var/www/swportal

# Iniciar serviço
echo "??  Iniciando serviço..."
sudo systemctl start swportal-api.service

echo "? Restore concluído com sucesso!"
sudo systemctl status swportal-api.service --no-pager
```

### Agendar backup automático

```bash
# Editar crontab
sudo crontab -e

# Adicionar linha para backup diário às 2h da manhã
0 2 * * * /path/to/scripts/backup.sh >> /var/log/swportal-backup.log 2>&1
```

---

## ?? 12. Troubleshooting

### Problemas Comuns

#### 1. Permissões de arquivo

```bash
# Corrigir permissões
sudo chown -R www-data:www-data /var/www/swportal
sudo chmod -R 755 /var/www/swportal

# Verificar permissões
ls -la /var/www/swportal
```

#### 2. Porta já em uso

```bash
# Verificar o que está usando a porta
sudo lsof -i :5000

# Matar processo
sudo kill -9 <PID>
```

#### 3. Problemas de conectividade do banco

```bash
# Testar conexão
psql -h localhost -U swportal_user -d swportal_prod

# Ver conexões ativas
sudo -u postgres psql -c "SELECT * FROM pg_stat_activity WHERE datname='swportal_prod';"

# Reiniciar PostgreSQL
sudo systemctl restart postgresql
```

#### 4. Testar Redis

```bash
# Conectar ao Redis
redis-cli

# Testar autenticação e conexão
AUTH SenhaRedis123!
PING

# Ver chaves
KEYS *

# Ver informações
INFO
```

#### 5. Verificar RabbitMQ

```bash
# Status
sudo rabbitmqctl status

# Listar filas
sudo rabbitmqctl list_queues

# Listar conexões
sudo rabbitmqctl list_connections

# Limpar fila (se necessário)
sudo rabbitmqctl purge_queue nome_da_fila
```

#### 6. API não responde

```bash
# Verificar se a aplicação está rodando
sudo systemctl status swportal-api.service

# Ver logs
sudo journalctl -u swportal-api.service -n 100

# Reiniciar serviço
sudo systemctl restart swportal-api.service

# Testar endpoint
curl -I http://localhost:5000/health
```

#### 7. Nginx retorna 502 Bad Gateway

```bash
# Verificar logs do Nginx
sudo tail -f /var/log/nginx/swportal-api-error.log

# Verificar se a API está rodando
sudo systemctl status swportal-api.service

# Testar conexão direta
curl -I http://localhost:5000

# Reiniciar Nginx
sudo systemctl restart nginx
```

#### 8. Erro de certificado SSL

```bash
# Renovar certificado
sudo certbot renew

# Reiniciar Nginx
sudo systemctl reload nginx

# Verificar validade do certificado
sudo certbot certificates
```

### Comandos Úteis

```bash
# Ver uso de recursos
htop

# Ver processos .NET
ps aux | grep dotnet

# Ver portas em uso
sudo netstat -tulpn | grep LISTEN

# Reiniciar todos os serviços
sudo systemctl restart swportal-api.service nginx postgresql redis-server rabbitmq-server

# Limpar logs antigos
sudo journalctl --vacuum-time=7d
```

---

## ? Checklist Final de Deploy

- [ ] Servidor Linux configurado (Ubuntu/Debian)
- [ ] .NET 8 Runtime instalado e funcionando
- [ ] Nginx instalado e configurado
- [ ] PostgreSQL instalado, configurado e rodando
- [ ] Redis instalado, configurado e rodando
- [ ] RabbitMQ instalado, configurado e rodando
- [ ] Certificado SSL configurado (Let's Encrypt)
- [ ] Arquivo `.env.production` criado e configurado
- [ ] Build e publicação realizados com sucesso
- [ ] Serviço systemd criado e habilitado
- [ ] Diretórios de arquivos criados com permissões corretas
- [ ] Nginx configurado como reverse proxy
- [ ] Testes de conectividade realizados
- [ ] Backup automático configurado
- [ ] Monitoramento em funcionamento
- [ ] Documentação atualizada
- [ ] Equipe treinada

---

## ?? Suporte

**Desenvolvido por:** SW Soluções Integradas Ltda  
**Email:** contato@swsolucoes.inf.br  
**Website:** https://www.swsolucoes.inf.br

**GitHub:** https://github.com/glebersonsm/SW_PortalProprietario_BeachPark.API

---

## ?? Referências

- [ASP.NET Core Deployment](https://learn.microsoft.com/pt-br/aspnet/core/host-and-deploy/)
- [Nginx Documentation](https://nginx.org/en/docs/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Redis Documentation](https://redis.io/documentation)
- [RabbitMQ Documentation](https://www.rabbitmq.com/documentation.html)
- [Let's Encrypt](https://letsencrypt.org/docs/)
- [Systemd Documentation](https://www.freedesktop.org/software/systemd/man/)

---

**Última atualização:** Janeiro 2024  
**Versão do documento:** 1.0
