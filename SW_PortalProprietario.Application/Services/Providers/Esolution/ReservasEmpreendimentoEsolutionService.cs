using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models.TimeSharing;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;

namespace SW_PortalProprietario.Application.Services.Providers.Esolution
{
    public class ReservasEmpreendimentoEsolutionService : IReservasEmpreendimentoProviderService
    {
        private readonly ILogger<ReservasEmpreendimentoEsolutionService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IRepositoryNHAccessCenter _repositoryNHAccessCenter;
        public ReservasEmpreendimentoEsolutionService(ILogger<ReservasEmpreendimentoEsolutionService> logger,
            IFinanceiroProviderService financeiroProviderService,
            IConfiguration configuration,
            IRepositoryNHAccessCenter repositoryNHAccessCenter)
        {
            _logger = logger;
            _configuration = configuration;
            _repositoryNHAccessCenter = repositoryNHAccessCenter;
        }

        public async Task<(int pageNumber, int lastPageNumber, List<ReservaGeralTsModel> reservas)?> GetMinhasReservasGeral(SearchMinhasReservasGeralModel searchModel)
        {
            throw new NotImplementedException();
        }

        public async Task<(int pageNumber, int lastPageNumber, List<ReservaGeralTsModel> reservas)?> GetReservasGeral(SearchReservasGeralModel searchModel)
        {
            throw new NotImplementedException();
        }
    }
}
