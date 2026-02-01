using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SW_PortalProprietario.Application.Interfaces.ProgramacaoParalela.LogsBackGround;

namespace SW_PortalProprietario.Application.Hosted.ProgramacaoParalela.LogsBackGround
{
    public class SaveLogOperationsSystemFromProcessingQueueConsumer : BackgroundService, IBackGroundSaveLogFromProcessingQueue
    {
        static bool _stopped = false;

        private readonly IConfiguration _configuration;
        private readonly ILogMessageFromQueueConsumer _logMessageFromQueue;
        public SaveLogOperationsSystemFromProcessingQueueConsumer(IConfiguration configuration,
            ILogMessageFromQueueConsumer logMessageFromQueue)
        {
            _configuration = configuration;
            _logMessageFromQueue = logMessageFromQueue;
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
            await Task.Delay(_configuration.GetValue("TimeWaitInMinutesSaveOperationsFromPorcessingLogQueueConsumer", 1) * 60000);
            await _logMessageFromQueue.RegisterConsumerAndSaveLogFromQueue();
        }
    }
}
