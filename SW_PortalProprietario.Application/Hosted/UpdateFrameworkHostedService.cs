using Microsoft.Extensions.Hosting;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Services.Core.Interfaces;

namespace SW_PortalProprietario.Application.Hosted
{
    public class UpdateFrameworkHostedService : BackgroundService, IBackGroundProcessUpdateFramework
    {
        static bool _stopped = false;

        private readonly IFrameworkInitialService _frameworkInitialService;
        public UpdateFrameworkHostedService(IFrameworkInitialService frameworkInitialService)
        {
            _frameworkInitialService = frameworkInitialService;
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
                await UpdateExecute();
                Stopped = true;
            }
        }

        private async Task UpdateExecute()
        {

            await _frameworkInitialService.UpdateFramework();

        }
    }
}
