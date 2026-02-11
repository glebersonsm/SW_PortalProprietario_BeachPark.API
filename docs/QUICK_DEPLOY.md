# ?? Guia Rápido de Deploy - SW Portal Proprietário

## ? Início Rápido

### 1?? Pré-requisitos no Servidor Linux

```bash
# Instalar tudo de uma vez
curl -fsSL https://raw.githubusercontent.com/glebersonsm/SW_PortalProprietario_BeachPark.API/master/scripts/install-server.sh | bash
```

Ou manualmente:

```bash
# .NET 8
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
sudo chmod +x dotnet-install.sh
sudo ./dotnet-install.sh --channel 8.0 --install-dir /usr/share/dotnet
sudo ln -s /usr/share/dotnet/dotnet /usr/local/bin

# Serviços
sudo apt update && sudo apt install -y nginx postgresql postgresql-contrib redis-server
curl -s https://packagecloud.io/install/repositories/rabbitmq/rabbitmq-server/script.deb.sh | sudo bash
sudo apt install -y rabbitmq-server
```

### 2?? Configurar Arquivo `.env.production`

```bash
# Copiar template
cp .env .env.production

# Editar valores para produção
nano .env.production
```

**Variáveis críticas:**
- `DEFAULT_CONNECTION` - String de conexão do PostgreSQL
- `JWT_KEY` - Chave segura com mínimo 32 caracteres
- `ORIGENS_PERMITIDAS` - Domínios permitidos para CORS
- `REDIS_PASSWORD` - Senha do Redis

### 3?? Build e Deploy

```bash
# Dar permissão aos scripts
chmod +x scripts/*.sh

# Build local
./scripts/build-and-publish.sh

# Configurar variáveis do deploy
nano scripts/deploy.sh
# REMOTE_USER="seu_usuario"
# REMOTE_HOST="seu_servidor.com"

# Deploy
./scripts/deploy.sh
```

---

## ?? Checklist de Deploy

### No Servidor (uma única vez)

- [ ] Servidor Ubuntu/Debian atualizado
- [ ] .NET 8 Runtime instalado
- [ ] Nginx instalado
- [ ] PostgreSQL instalado e rodando
- [ ] Redis instalado e rodando
- [ ] RabbitMQ instalado e rodando
- [ ] Usuário não-root com sudo configurado
- [ ] SSH configurado com chaves (sem senha)
- [ ] Firewall configurado (portas 80, 443, 22)
- [ ] DNS apontando para o servidor

### Banco de Dados

```sql
-- Conectar ao PostgreSQL
sudo -u postgres psql

-- Criar banco e usuário
CREATE DATABASE swportal_prod;
CREATE USER swportal_user WITH ENCRYPTED PASSWORD 'SenhaSegura123!';
GRANT ALL PRIVILEGES ON DATABASE swportal_prod TO swportal_user;

-- Conectar ao banco
\c swportal_prod

-- Criar extensão necessária
CREATE EXTENSION IF NOT EXISTS unaccent;
```

### Nginx

```bash
# Criar configuração
sudo nano /etc/nginx/sites-available/swportal-api
# (copiar configuração do docs/DEPLOY_LINUX.md)

# Habilitar site
sudo ln -s /etc/nginx/sites-available/swportal-api /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

### SSL com Let's Encrypt

```bash
sudo apt install certbot python3-certbot-nginx -y
sudo certbot --nginx -d api.seudominio.com
```

### Systemd Service

```bash
# Criar serviço
sudo nano /etc/systemd/system/swportal-api.service
# (copiar configuração do docs/DEPLOY_LINUX.md)

# Habilitar e iniciar
sudo systemctl daemon-reload
sudo systemctl enable swportal-api.service
sudo systemctl start swportal-api.service
sudo systemctl status swportal-api.service
```

---

## ?? Workflow de Deploy Regular

```bash
# 1. Desenvolvimento local
git pull origin master

# 2. Fazer alterações e testar
dotnet test

# 3. Commit
git add .
git commit -m "Descrição das alterações"
git push origin master

# 4. Deploy
./scripts/deploy.sh

# 5. Monitorar
./scripts/monitor.sh
sudo journalctl -u swportal-api.service -f
```

---

## ??? Comandos Úteis

### Ver Logs

```bash
# API
sudo journalctl -u swportal-api.service -f

# Nginx
sudo tail -f /var/log/nginx/swportal-api-error.log

# PostgreSQL
sudo tail -f /var/log/postgresql/postgresql-*-main.log

# Redis
sudo tail -f /var/log/redis/redis-server.log
```

### Reiniciar Serviços

```bash
# API
sudo systemctl restart swportal-api.service

# Nginx
sudo systemctl reload nginx

# PostgreSQL
sudo systemctl restart postgresql

# Redis
sudo systemctl restart redis-server

# RabbitMQ
sudo systemctl restart rabbitmq-server

# Todos de uma vez
sudo systemctl restart swportal-api nginx postgresql redis-server rabbitmq-server
```

### Monitoramento

```bash
# Status de todos os serviços
./scripts/monitor.sh

# Uso de recursos
htop

# Processos .NET
ps aux | grep dotnet

# Portas em uso
sudo netstat -tulpn | grep LISTEN

# Espaço em disco
df -h

# Memória
free -h
```

---

## ?? Backup e Restore

### Backup Manual

```bash
./scripts/backup.sh
```

### Backup Automático

```bash
# Editar crontab
sudo crontab -e

# Adicionar (diário às 2h)
0 2 * * * /var/www/swportal/scripts/backup.sh >> /var/log/swportal-backup.log 2>&1
```

### Restore

```bash
# Listar backups
./scripts/restore.sh

# Restaurar backup específico
./scripts/restore.sh 20240101_120000
```

---

## ?? Troubleshooting Rápido

### API não inicia

```bash
# Ver logs
sudo journalctl -u swportal-api.service -n 50

# Testar manualmente
cd /var/www/swportal
dotnet SW_PortalCliente_BeachPark.API.dll

# Verificar .env
cat /var/www/swportal/.env | grep -v "PASSWORD\|KEY"
```

### Erro 502 Bad Gateway

```bash
# Verificar se API está rodando
curl http://localhost:5000/health

# Reiniciar API
sudo systemctl restart swportal-api.service

# Ver logs do Nginx
sudo tail -f /var/log/nginx/swportal-api-error.log
```

### Banco de dados não conecta

```bash
# Testar conexão
psql -h localhost -U swportal_user -d swportal_prod

# Verificar se está rodando
sudo systemctl status postgresql

# Ver logs
sudo tail -f /var/log/postgresql/postgresql-*-main.log
```

### Redis não conecta

```bash
# Testar conexão
redis-cli
AUTH SenhaRedis123!
PING

# Verificar se está rodando
sudo systemctl status redis-server

# Ver logs
sudo tail -f /var/log/redis/redis-server.log
```

---

## ?? Suporte

**Documentação Completa:** [docs/DEPLOY_LINUX.md](./DEPLOY_LINUX.md)  
**Scripts:** [scripts/README.md](../scripts/README.md)  
**Email:** contato@swsolucoes.inf.br  
**GitHub:** https://github.com/glebersonsm/SW_PortalProprietario_BeachPark.API

---

## ?? Próximos Passos Após Deploy

1. **Configurar monitoramento:**
   - [ ] Adicionar Prometheus/Grafana (opcional)
   - [ ] Configurar alertas de email
   - [ ] Configurar healthchecks

2. **Configurar backups:**
   - [ ] Backup automático diário
   - [ ] Backup remoto (S3, Azure, etc.)
   - [ ] Testar restore

3. **Segurança:**
   - [ ] Configurar firewall
   - [ ] Atualizar senhas padrão
   - [ ] Configurar fail2ban
   - [ ] Atualizar certificado SSL regularmente

4. **Documentação:**
   - [ ] Documentar configurações específicas
   - [ ] Criar runbook de operações
   - [ ] Treinar equipe

---

**Última atualização:** Janeiro 2024  
**Versão:** 1.0
