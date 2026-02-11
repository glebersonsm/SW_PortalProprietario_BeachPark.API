#!/bin/bash
# monitor.sh
# Script para monitorar o status dos serviços

echo "?? Status dos Serviços SW Portal Proprietário"
echo "=============================================="
echo ""

# Função para verificar se um serviço está rodando
check_service() {
    local service_name=$1
    local display_name=$2
    
    echo "?? $display_name:"
    if systemctl is-active --quiet $service_name; then
        echo "   ? Ativo"
        sudo systemctl status $service_name --no-pager | grep "Active:" | sed 's/^/   /'
    else
        echo "   ? Inativo"
        sudo systemctl status $service_name --no-pager | grep "Active:" | sed 's/^/   /'
    fi
    echo ""
}

# Verificar serviços
check_service "swportal-api.service" "API SW Portal"
check_service "nginx" "Nginx"
check_service "postgresql" "PostgreSQL"
check_service "redis-server" "Redis"
check_service "rabbitmq-server" "RabbitMQ"

echo "?? Uso de Recursos"
echo "===================="
echo ""

# Memória
echo "?? Memória:"
free -h | grep -E "^Mem:" | awk '{print "   Total: "$2"  |  Usado: "$3"  |  Livre: "$4"  |  Disponível: "$7}'
echo ""

# Disco
echo "?? Disco (/var/www/swportal):"
df -h /var/www/swportal 2>/dev/null | tail -n 1 | awk '{print "   Total: "$2"  |  Usado: "$3" ("$5")  |  Livre: "$4"}' || echo "   ??  Diretório não encontrado"
echo ""

# CPU
echo "???  CPU:"
top -bn1 | grep "Cpu(s)" | sed "s/.*, *\([0-9.]*\)%* id.*/\1/" | awk '{print "   Uso: " 100 - $1"%"}'
echo ""

# Processos .NET
echo "??  Processos .NET:"
ps aux | grep "[d]otnet" | wc -l | awk '{print "   Processos rodando: "$1}'
echo ""

echo "?? Conectividade"
echo "===================="
echo ""

# Verificar se a API está respondendo localmente
echo "?? API (localhost:5000):"
if curl -s -o /dev/null -w "%{http_code}" http://localhost:5000/health 2>/dev/null | grep -q "200"; then
    echo "   ? Respondendo (Status: 200)"
else
    HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:5000/health 2>/dev/null || echo "000")
    echo "   ? Não respondendo (Status: $HTTP_CODE)"
fi
echo ""

# Verificar portas em uso
echo "?? Portas em Uso:"
echo "   API: $(sudo lsof -i :5000 -sTCP:LISTEN -t | wc -l) processo(s) na porta 5000"
echo "   Nginx: $(sudo lsof -i :80 -sTCP:LISTEN -t | wc -l) processo(s) na porta 80"
echo "   Nginx HTTPS: $(sudo lsof -i :443 -sTCP:LISTEN -t | wc -l) processo(s) na porta 443"
echo "   PostgreSQL: $(sudo lsof -i :5432 -sTCP:LISTEN -t | wc -l) processo(s) na porta 5432"
echo "   Redis: $(sudo lsof -i :6379 -sTCP:LISTEN -t | wc -l) processo(s) na porta 6379"
echo "   RabbitMQ: $(sudo lsof -i :5672 -sTCP:LISTEN -t | wc -l) processo(s) na porta 5672"
echo ""

echo "?? Logs Recentes (API)"
echo "===================="
sudo journalctl -u swportal-api.service -n 5 --no-pager | tail -n 5
echo ""

echo "? Monitoramento concluído!"
echo ""
echo "?? Dicas:"
echo "   - Ver logs em tempo real: sudo journalctl -u swportal-api.service -f"
echo "   - Reiniciar API: sudo systemctl restart swportal-api.service"
echo "   - Ver logs do Nginx: sudo tail -f /var/log/nginx/swportal-api-error.log"
