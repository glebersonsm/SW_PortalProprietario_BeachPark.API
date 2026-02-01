using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SW_PortalProprietario.Application.Interfaces.ProgramacaoParalela.Communication.Email;

namespace SW_PortalProprietario.Application.Hosted.ProgramacaoParalela.Communication.Email
{
    public class EmailSenderFromProcessingQueueConsumer : BackgroundService, IBackGroundSenderEmailFromProcessingQueue
    {
        static bool _stopped = false;

        private readonly IConfiguration _configuration;
        private readonly IEmailSenderFromQueueConsumer _sendEmailFromQueue;
        public EmailSenderFromProcessingQueueConsumer(IConfiguration configuration,
            IEmailSenderFromQueueConsumer senderEmailFromQueue)
        {
            _configuration = configuration;
            _sendEmailFromQueue = senderEmailFromQueue;
        }

        public bool Stopped
        {
            get
            {
                return _stopped;
            }
            set { _stopped = value; }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!Stopped)
            {
                await Execute();
            }
        }

        private async Task Execute()
        {
            await Task.Delay(_configuration.GetValue("TimeWaitInMinutesSandEmailFromPorcessingQueueConsumer", 1) * 60000);
            
            await _sendEmailFromQueue.RegisterAndSendEmailFromQueue();
        }
    }
}
