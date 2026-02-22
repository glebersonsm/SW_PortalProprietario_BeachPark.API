using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Interfaces.Esol;
using SW_PortalProprietario.Application.Models.Esol;
using SW_Utils.Auxiliar;
using System.Text;

namespace SW_PortalProprietario.Application.Services.Esol
{
    /// <summary>
    /// Serviço de consultas gerais - migrado do SwReservaApiMain (Access Center).
    /// </summary>
    public class GeralAccessCenterEsolService : IGeralAccessCenterEsolService
    {
        private readonly IRepositoryNHAccessCenter _repository;
        private readonly ILogger<GeralAccessCenterEsolService> _logger;
        private readonly IConfiguration _configuration;

        public GeralAccessCenterEsolService(IRepositoryNHAccessCenter repository,
            ILogger<GeralAccessCenterEsolService> logger,
            IConfiguration configuration)
        {
            _repository = repository;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<List<EmpresaEsolModel>> ConsultarEmpresa(ConsultaEmpresaEsolModel model)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(model, nameof(model));

                await _repository.GetLoggedUser();

                var parameters = new List<Parameter>();

                var sb = new StringBuilder(@$"Select 
                e.id as EmpresaId, 
                p.Nome,
                p.NomeExibicao as NomeFantasia,
                p.CNPJ
                From 
                Empresa e 
                inner join Pessoa p on p.Id = e.Pessoa 
                Where 1 = 1 ");

                if (model.EmpresaId.GetValueOrDefault(0) > 0)
                {
                    sb.AppendLine(" and e.Id = @empresaId ");
                    parameters.Add(new Parameter("empresaId", model.EmpresaId.GetValueOrDefault()));
                }

                if (!string.IsNullOrEmpty(model.Nome))
                {
                    sb.AppendLine($" and Lower(p.Nome) like '{model.Nome.ToLower()}%' ");
                }

                var empresas = (await _repository.FindBySql<EmpresaEsolModel>(sb.ToString(), parameters.ToArray())).ToList();

                return empresas;
            }
            catch (Exception err)
            {
                _logger.LogError(err, err.Message);
                throw;
            }
        }
    }
}
