using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Interfaces.ProgramacaoParalela.LogsBackGround;
using SW_Utils.Models;

namespace SW_PortalProprietario.Application.Hosted.ProgramacaoParalela.LogsBackGround
{
    public class SenderLogOperationsSystemToProcessingQueue : BackgroundService, IBackGroundSenderLogToProcessingQueue
    {
        static bool _stopped = false;

        private readonly ICacheStore _cache;
        private readonly IConfiguration _configuration;
        private readonly ILogMessageToQueueProducer _logMessageToQueue;
        public SenderLogOperationsSystemToProcessingQueue(ICacheStore cache,
            IConfiguration configuration,
            ILogMessageToQueueProducer logMessageToQueue)
        {
            _cache = cache;
            _configuration = configuration;
            _logMessageToQueue = logMessageToQueue;
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
            await Task.Delay(_configuration.GetValue("TimeWaitInMinutesSendOperationsToPorcessingLogQueue", 1) * 60000);
            var keysToSend = await _cache.GetKeysAsync<List<string>>("operationSystemExecuted", default);
            if (keysToSend.Any())
            {
                var itensToSend = await _cache.GetListAsync<OperationSystemLogModelEvent>(keysToSend, default);
                foreach (var item in itensToSend)
                {
                    await _logMessageToQueue.AddLogMessage(item.Value);
                    await _cache.DeleteByKey(item.Key, default);
                }
            }
        }
    }
}
