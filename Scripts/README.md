# Scripts de Deploy e Manutenção

Este diretório contém scripts úteis para build, deploy, monitoramento e manutenção da API SW Portal Proprietário.

## ?? Lista de Scripts

### 1. build-and-publish.sh
**Finalidade:** Compila e publica a aplicação para produção.

**Uso:**
```bash
chmod +x scripts/build-and-publish.sh
./scripts/build-and-publish.sh
```

**O que faz:**
- Limpa builds anteriores
- Restaura dependências
- Compila o projeto em modo Release
- Publica a aplicação
- Copia o arquivo `.env.production` para `.env`

---

### 2. deploy.sh
**Finalidade:** Automatiza o deploy completo no servidor Linux.

**Configuração necessária:**
Edite as variáveis no início do script:
```bash
REMOTE_USER="seu_usuario"
REMOTE_HOST="seu_servidor.com"
REMOTE_PATH="/var/www/swportal"
```

**Uso:**
```bash
chmod +x scripts/deploy.sh
./scripts/deploy.sh
```

**O que faz:**
- Executa o build local
- Envia arquivos via rsync
- Para o serviço no servidor
- Cria diretórios necessários
- Ajusta permissões
- Reinicia o serviço
- Mostra o status

---

### 3. monitor.sh
**Finalidade:** Monitora o status de todos os serviços e recursos do sistema.

**Uso:**
```bash
chmod +x scripts/monitor.sh
./scripts/monitor.sh
```

**O que mostra:**
- Status de todos os serviços (API, Nginx, PostgreSQL, Redis, RabbitMQ)
- Uso de memória, disco e CPU
- Conectividade da API
- Portas em uso
- Logs recentes

---

### 4. backup.sh
**Finalidade:** Realiza backup completo do sistema.

**Configuração necessária:**
Configure as variáveis de conexão do banco:
```bash
DB_USER="swportal_user"
DB_NAME="swportal_prod"
DB_HOST="localhost"
```

**Uso:**
```bash
chmod +x scripts/backup.sh
./scripts/backup.sh
```

**O que faz backup:**
- Banco de dados PostgreSQL (compactado)
- Arquivos (wwwroot, documents, pdfs, certidoes)
- Configurações (.env, appsettings, nginx, systemd)
- Limpa backups antigos (>7 dias)

**Agendar backup automático:**
```bash
sudo crontab -e
# Adicionar linha para backup diário às 2h
0 2 * * * /caminho/para/scripts/backup.sh >> /var/log/swportal-backup.log 2>&1
```

---

### 5. restore.sh
**Finalidade:** Restaura um backup do sistema.

**Uso:**
```bash
chmod +x scripts/restore.sh

# Listar backups disponíveis
./scripts/restore.sh

# Restaurar um backup específico
./scripts/restore.sh 20240101_120000
```

**O que faz:**
- Cria backup de segurança antes do restore
- Para o serviço da API
- Restaura banco de dados
- Restaura arquivos (se disponível)
- Restaura configurações (se disponível)
- Ajusta permissões
- Reinicia o serviço

**?? Atenção:** Este script sobrescreve os dados atuais!

---

## ?? Preparação Inicial

Antes de usar os scripts, torne-os executáveis:

```bash
chmod +x scripts/*.sh
```

## ?? Variáveis de Ambiente

### Para backup.sh e restore.sh

Se necessário, configure a senha do banco de dados:

```bash
export DB_PASSWORD="sua_senha_do_banco"
```

Ou adicione ao script diretamente (não recomendado em produção).

### Para deploy.sh

**Obrigatório:** Configure acesso SSH sem senha (usando chaves SSH):

```bash
# Gerar chave SSH (se não tiver)
ssh-keygen -t rsa -b 4096 -C "seu_email@exemplo.com"

# Copiar chave pública para o servidor
ssh-copy-id seu_usuario@seu_servidor.com

# Testar conexão
ssh seu_usuario@seu_servidor.com
```

## ?? Monitoramento Contínuo

Para monitoramento em tempo real, use:

```bash
# Monitorar logs da API
sudo journalctl -u swportal-api.service -f

# Monitorar todos os serviços
watch -n 5 './scripts/monitor.sh'
```

## ?? Workflow Recomendado

### Deploy em Produção

1. **Desenvolvimento local:**
   ```bash
   # Fazer alterações e testar
   dotnet test
   ```

2. **Build e teste:**
   ```bash
   ./scripts/build-and-publish.sh
   ```

3. **Deploy:**
   ```bash
   ./scripts/deploy.sh
   ```

4. **Monitorar:**
   ```bash
   ./scripts/monitor.sh
   ```

### Backup Regular

1. **Configurar cron para backup automático:**
   ```bash
   sudo crontab -e
   # Adicionar:
   0 2 * * * /var/www/swportal/scripts/backup.sh >> /var/log/swportal-backup.log 2>&1
   ```

2. **Backup manual (quando necessário):**
   ```bash
   ./scripts/backup.sh
   ```

### Restore em Caso de Problema

1. **Listar backups:**
   ```bash
   ./scripts/restore.sh
   ```

2. **Restaurar backup:**
   ```bash
   ./scripts/restore.sh 20240101_120000
   ```

## ?? Troubleshooting

### Problema: "Permission denied"

**Solução:**
```bash
chmod +x scripts/*.sh
```

### Problema: "rsync: command not found"

**Solução:**
```bash
sudo apt install rsync
```

### Problema: "pg_dump: command not found"

**Solução:**
```bash
sudo apt install postgresql-client
```

### Problema: Deploy falha na conexão SSH

**Solução:**
```bash
# Verificar conectividade
ssh seu_usuario@seu_servidor.com

# Configurar chaves SSH
ssh-copy-id seu_usuario@seu_servidor.com
```

## ?? Documentação Adicional

Para documentação completa sobre deploy, consulte:
- [DEPLOY_LINUX.md](../docs/DEPLOY_LINUX.md) - Guia completo de deploy

## ?? Suporte

Em caso de problemas ou dúvidas:
- **Email:** contato@swsolucoes.inf.br
- **GitHub:** https://github.com/glebersonsm/SW_PortalProprietario_BeachPark.API

---

**Última atualização:** Janeiro 2024
