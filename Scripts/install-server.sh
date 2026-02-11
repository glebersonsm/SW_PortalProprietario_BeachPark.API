#!/bin/bash
# install-server.sh
# Script para instalação automatizada dos componentes necessários no servidor Linux

set -e

echo "?? Instalação Automatizada - SW Portal Proprietário"
echo "===================================================="
echo ""

# Verificar se está rodando como root
if [[ $EUID -ne 0 ]]; then
   echo "? Este script deve ser executado como root (use sudo)"
   exit 1
fi

# Atualizar sistema
echo "?? Atualizando sistema..."
apt update && apt upgrade -y

# Instalar dependências básicas
echo "?? Instalando dependências básicas..."
apt install -y wget curl apt-transport-https software-properties-common gnupg2

# Instalar .NET 8 Runtime
echo "?? Instalando .NET 8 Runtime..."
if ! command -v dotnet &> /dev/null; then
    wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
    chmod +x dotnet-install.sh
    ./dotnet-install.sh --channel 8.0 --install-dir /usr/share/dotnet
    ln -sf /usr/share/dotnet/dotnet /usr/local/bin/dotnet
    rm dotnet-install.sh
    echo "? .NET 8 instalado: $(dotnet --version)"
else
    echo "? .NET já instalado: $(dotnet --version)"
fi

# Instalar Nginx
echo "?? Instalando Nginx..."
if ! command -v nginx &> /dev/null; then
    apt install -y nginx
    systemctl enable nginx
    systemctl start nginx
    echo "? Nginx instalado"
else
    echo "? Nginx já instalado"
fi

# Instalar PostgreSQL
echo "?? Instalando PostgreSQL..."
if ! command -v psql &> /dev/null; then
    apt install -y postgresql postgresql-contrib
    systemctl enable postgresql
    systemctl start postgresql
    echo "? PostgreSQL instalado"
else
    echo "? PostgreSQL já instalado"
fi

# Instalar Redis
echo "?? Instalando Redis..."
if ! command -v redis-cli &> /dev/null; then
    apt install -y redis-server
    systemctl enable redis-server
    systemctl start redis-server
    echo "? Redis instalado"
else
    echo "? Redis já instalado"
fi

# Instalar RabbitMQ
echo "?? Instalando RabbitMQ..."
if ! command -v rabbitmqctl &> /dev/null; then
    curl -s https://packagecloud.io/install/repositories/rabbitmq/rabbitmq-server/script.deb.sh | bash
    apt install -y rabbitmq-server
    systemctl enable rabbitmq-server
    systemctl start rabbitmq-server
    rabbitmq-plugins enable rabbitmq_management
    echo "? RabbitMQ instalado"
else
    echo "? RabbitMQ já instalado"
fi

# Instalar Certbot (Let's Encrypt)
echo "?? Instalando Certbot..."
if ! command -v certbot &> /dev/null; then
    apt install -y certbot python3-certbot-nginx
    echo "? Certbot instalado"
else
    echo "? Certbot já instalado"
fi

# Criar diretórios necessários
echo "?? Criando diretórios..."
mkdir -p /var/www/swportal
mkdir -p /var/www/swportal/wwwroot/{images/grupos,documents,pdfs/{comunicacoes,boletos},certidoes/{modelos,pdf,contratos}}
mkdir -p /backup/swportal

# Configurar permissões
echo "?? Configurando permissões..."
chown -R www-data:www-data /var/www/swportal
chmod -R 755 /var/www/swportal
chown -R www-data:www-data /backup/swportal
chmod -R 755 /backup/swportal

# Configurar firewall (ufw)
echo "?? Configurando firewall..."
if command -v ufw &> /dev/null; then
    ufw --force enable
    ufw allow 22/tcp    # SSH
    ufw allow 80/tcp    # HTTP
    ufw allow 443/tcp   # HTTPS
    echo "? Firewall configurado"
fi

echo ""
echo "? Instalação concluída com sucesso!"
echo ""
echo "?? Status dos Serviços:"
echo "======================="
systemctl is-active --quiet nginx && echo "? Nginx: Ativo" || echo "? Nginx: Inativo"
systemctl is-active --quiet postgresql && echo "? PostgreSQL: Ativo" || echo "? PostgreSQL: Inativo"
systemctl is-active --quiet redis-server && echo "? Redis: Ativo" || echo "? Redis: Inativo"
systemctl is-active --quiet rabbitmq-server && echo "? RabbitMQ: Ativo" || echo "? RabbitMQ: Inativo"

echo ""
echo "?? Próximos Passos:"
echo "==================="
echo ""
echo "1. Configurar PostgreSQL:"
echo "   sudo -u postgres psql"
echo "   CREATE DATABASE swportal_prod;"
echo "   CREATE USER swportal_user WITH ENCRYPTED PASSWORD 'SenhaSegura123!';"
echo "   GRANT ALL PRIVILEGES ON DATABASE swportal_prod TO swportal_user;"
echo "   \\c swportal_prod"
echo "   CREATE EXTENSION IF NOT EXISTS unaccent;"
echo "   \\q"
echo ""
echo "2. Configurar Redis (opcional - senha):"
echo "   sudo nano /etc/redis/redis.conf"
echo "   # Descomentar e definir: requirepass SenhaRedis123!"
echo "   sudo systemctl restart redis-server"
echo ""
echo "3. Configurar RabbitMQ (opcional - usuário admin):"
echo "   sudo rabbitmqctl add_user admin SenhaRabbitMQ123!"
echo "   sudo rabbitmqctl set_user_tags admin administrator"
echo "   sudo rabbitmqctl set_permissions -p / admin '.*' '.*' '.*'"
echo "   # Acessar painel: http://seu-servidor:15672"
echo ""
echo "4. Fazer deploy da aplicação:"
echo "   # No seu computador local:"
echo "   ./scripts/deploy.sh"
echo ""
echo "5. Configurar SSL:"
echo "   sudo certbot --nginx -d api.seudominio.com"
echo ""
echo "?? Documentação completa:"
echo "   https://github.com/glebersonsm/SW_PortalProprietario_BeachPark.API/blob/master/docs/DEPLOY_LINUX.md"
