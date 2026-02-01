using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Interfaces.ProgramacaoParalela.Communication.Email;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces.Communication;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Enumns;
using SW_Utils.Models;
using System.Text;

namespace SW_PortalProprietario.Infra.Data.RabbitMQ.Consumers
{
    public class RabbitMQEmailSenderFromQueueConsumer : IEmailSenderFromQueueConsumer, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly IEmailSenderHostedService _emailSenderService;
        private readonly ILogger<RabbitMQEmailSenderFromQueueConsumer> _logger;
        private readonly IRepositoryHosted _repository;

        private IConnection? _connection;
        private IChannel? _channel;
        private static bool _isRunning = false;
        private static readonly object _lock = new();

        public RabbitMQEmailSenderFromQueueConsumer(
            IConfiguration configuration,
            IEmailSenderHostedService emailSenderService,
            ILogger<RabbitMQEmailSenderFromQueueConsumer> logger,
            IRepositoryHosted repository)
        {
            _configuration = configuration;
            _emailSenderService = emailSenderService;
            _logger = logger;
            _repository = repository;

        }

        public async Task RegisterAndSendEmailFromQueue()
        {
            try
            {
                lock (_lock)
                {
                    if (_isRunning)
                    {
                        _logger.LogInformation("Consumer de email já está em execução");
                        return;
                    }
                    _isRunning = true;
                }

                var factory = new ConnectionFactory
                {
                    HostName = _configuration.GetValue<string>("RabbitMqConnectionHost"),
                    Password = _configuration.GetValue<string>("RabbitMqConnectionPass"),
                    UserName = _configuration.GetValue<string>("RabbitMqConnectionUser"),
                    Port = _configuration.GetValue<int>("RabbitMqConnectionPort"),
                    ClientProvidedName = _configuration.GetValue<string>("RabbitMqFilaDeEmailNome", "ProcessamentoFilaEmail"),
                    ConsumerDispatchConcurrency = 1,
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
                };

                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                // Configurar QoS para processar uma mensagem por vez
                await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

                var queueName = $"{_configuration.GetValue<string>("ProgramId", "PORTALPROP_")}{_configuration.GetValue<string>("RabbitMqFilaDeEmailNome", "ProcessamentoFilaEmail")}";
                queueName = queueName.Replace(" ", "");
                
                await _channel.ExchangeDeclareAsync(
                    exchange: queueName,
                    type: ExchangeType.Direct,
                    durable: true,
                    autoDelete: false,
                    arguments: null);

                await _channel.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                await _channel.QueueBindAsync(queue: queueName, exchange: queueName, routingKey: queueName);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var tag = ea.DeliveryTag;
                    var message = Encoding.UTF8.GetString(body);

                    try
                    {
                        _logger.LogInformation($"Processando email da fila. DeliveryTag: {tag}");

                        var emailModel = System.Text.Json.JsonSerializer.Deserialize<EmailModel>(message);
                        if (emailModel != null)
                        {
                            // Carregar anexos do banco de dados antes de enviar
                            await CarregarAnexosDoEmail(emailModel);
                            
                            await _emailSenderService.Send(emailModel);
                            await _channel.BasicAckAsync(deliveryTag: tag, multiple: false);
                            await MarcarComoEnviado(emailModel.Id.GetValueOrDefault());
                            
                            _logger.LogInformation($"Email {emailModel.Id} enviado e marcado como enviado com sucesso");
                        }
                        else
                        {
                            _logger.LogWarning($"Não foi possível deserializar o email. DeliveryTag: {tag}");
                            await _channel.BasicNackAsync(deliveryTag: tag, multiple: false, requeue: false);
                        }
                    }
                    catch (Exception err)
                    {
                        _logger.LogError(err, $"Erro ao processar email da fila. DeliveryTag: {tag}");
                        // Não reenviar para a fila (requeue: false) para evitar loop infinito
                        await _channel.BasicNackAsync(deliveryTag: tag, multiple: false, requeue: false);
                    }
                };
                
                await _channel.BasicConsumeAsync(
                    queue: queueName,
                    autoAck: false,
                    consumer: consumer);

                _logger.LogInformation($"Consumer de email registrado na fila: {queueName}");

                await Task.CompletedTask;
            }
            catch (Exception err)
            {
                _logger.LogError(err, "Erro ao registrar consumer de email");
                lock (_lock)
                {
                    _isRunning = false;
                }
                throw;
            }
        }

        private async Task MarcarComoEnviado(int id)
        {
            using (var session = _repository.CreateSession())
            {
                try
                {
                    _repository.BeginTransaction(session);

                    var usuarioDefault = _configuration.GetValue<int>("UsuarioSistemaId", 1);

                    Email email = await _repository.FindById<Email>(id, session);
                    if (email == null)
                    {
                        _logger.LogWarning($"Email com id: {id} não encontrado no banco de dados");
                        return;
                    }

                    if (email.Enviado == EnumSimNao.Sim)
                    {
                        _logger.LogInformation($"O email id: {id} já foi enviado anteriormente em: {email.DataHoraEnvio.GetValueOrDefault():dd/MM/yyyy HH:mm:ss}");
                        return;
                    }

                    email.Enviado = EnumSimNao.Sim;
                    email.NaFila = EnumSimNao.Não;
                    email.DataHoraEnvio = DateTime.Now;
                    email.UsuarioAlteracao = email.UsuarioCriacao.GetValueOrDefault(usuarioDefault);

                    var result = await _repository.ForcedSave(email, session);

                    await _repository.CommitAsync(session);
                    
                    _logger.LogInformation($"Email: ({result.Id}) marcado como enviado com sucesso!");
                }
                catch (Exception err)
                {
                    _logger.LogError(err, $"Erro ao marcar email {id} como enviado");
                    _repository.Rollback(session);
                }
            }
        }

        private async Task CarregarAnexosDoEmail(EmailModel emailModel)
        {
            if (emailModel?.Id == null)
                return;

            using (var session = _repository.CreateSession())
            {
                try
                {
                    var anexos = await _repository.FindBySql<EmailAnexoModel>(
                        @"SELECT 
                            ea.Id,
                            ea.NomeArquivo,
                            ea.TipoMime,
                            ea.Arquivo
                          FROM EmailAnexo ea
                          WHERE ea.Email = :emailId",
                        session,
                        new SW_Utils.Auxiliar.Parameter("emailId", emailModel.Id.Value));

                    if (anexos != null && anexos.Any())
                    {
                        emailModel.Anexos = anexos.ToList();
                        _logger.LogInformation($"Carregados {anexos.Count()} anexo(s) para o email {emailModel.Id}");
                    }
                }
                catch (Exception err)
                {
                    _logger.LogError(err, $"Erro ao carregar anexos do email {emailModel.Id}");
                    // Não lançar exceção para não impedir o envio do email
                }
            }
        }

        public void Dispose()
        {
            try
            {
                _channel?.CloseAsync().GetAwaiter().GetResult();
                _channel?.Dispose();
                _connection?.CloseAsync().GetAwaiter().GetResult();
                _connection?.Dispose();
                
                lock (_lock)
                {
                    _isRunning = false;
                }
                
                _logger.LogInformation("Consumer de email finalizado e recursos liberados");
            }
            catch (Exception err)
            {
                _logger.LogError(err, "Erro ao finalizar consumer de email");
            }
        }
    }
}
