using Dapper;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models.FrameworkModels;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Models.TimeSharing;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Geral;

namespace SW_PortalProprietario.Application.Services.Core
{
    public class VhfConfigService : IVhfConfigService
    {
        private readonly IFrameworkService _frameworkService;
        private readonly IRepositoryNH _repository;
        private readonly IRepositoryNHCm _repositoryCM;
        private readonly ICacheStore _cacheStore;

        public VhfConfigService(IFrameworkService frameworkService, IRepositoryNH repository, IRepositoryNHCm repositoryCM, ICacheStore cacheStore)
        {
            _frameworkService = frameworkService;
            _repository = repository;
            _repositoryCM = repositoryCM;
            _cacheStore = cacheStore;
        }

        public async Task<VhfConfigOpcoesModel> GetOpcoesAsync()
        {
            var itemCache = await _cacheStore.GetAsync<VhfConfigOpcoesModel>("Opcoes_Configuracoes_", 2, _repository.CancellationToken);
            if (itemCache != null)
                return itemCache;
                        
            
            var result = new VhfConfigOpcoesModel();

            // 1. Tipo de utilização (fixo). "Todos os negócios" = configuração vale para todos os tipos.
            result.TipoNegocio = new List<VhfConfigOpcaoItem>
            {
                new() { Value = "Todos os negócios", Label = "Todos os negócios" },
                new() { Value = "Timesharing", Label = "Timesharing" },
                new() { Value = "Multipropriedade", Label = "Multipropriedade" }
            };

            // 1. Tipo de utilização (fixo). "Todas" = configuração vale para todos os tipos.
            result.TipoUtilizacao = new List<VhfConfigOpcaoItem>
            {
                new() { Value = "Todos", Label = "Todos" },
                new() { Value = "Uso próprio", Label = "Uso próprio" },
                new() { Value = "Uso convidado", Label = "Uso convidado" }
            };

            // 2. Hotéis (CM): unidades do cadastro de Empresas
            try
            {
                result.Hoteis = (await _repositoryCM.FindBySql<HotelModel>(@$"Select 
                    h.IdHotel as Id,
                    h.IdHotel,
                    p.Nome as HotelNome
                    From 
                    Hotel h 
                    Inner Join Pessoa p on h.IdPessoa = p.IdPessoa
                    Where 
                    1 = 1
                    AND exists( SELECT 
			                     b.IdHotel
                                FROM
                                 (
			                    SELECT 
		                         Count(1) AS Qtde,
			                     uh.IdHotel 
		                        FROM 
		                         uh
		                        WHERE 
		                         1 = 1
		                         GROUP BY uh.IDHOTEL 
		                         HAVING Count(1) > 10
		                        ) b WHERE b.IDHOTEL = h.IDHOTEL)")).AsList();
            }
            catch
            {
                result.Hoteis = new List<HotelModel>();
            }

            // 3. Tipo de Hóspede (CM): categorias padrão
            try
            {
                result.TiposHospede = (await _repositoryCM.FindBySql<TipoHospedeModel>(@$"Select 
                th.IdTipoHospede as Id,
                th.CodReduzido as CodReduzido,
                th.Descricao as Nome,
                th.IdHotel as IdHotel
                From 
                TipoHospede th
                Where 
                1 = 1")).AsList();
            }
            catch
            {
                result.TiposHospede = new List<TipoHospedeModel>();
            }


            // 4. Origem (CM): canais de venda
            try
            {
                result.Origens = (await _repositoryCM.FindBySql<OrigemReservaModel>(@$"SELECT ori.IdOrigem AS Id,
                ori.CodReduzido,
                ori.Descricao AS Nome ,
                ori.IdOrigem
                FROM 
                origemreserva ori
                WHERE lower(ori.descricao) NOT LIKE '%falta%' and ori.FlgAtivo = 'S'")).AsList();
            }
            catch
            {
                result.Origens = new List<OrigemReservaModel>();
            }

            // 5. Tarifa Hotel (CM): códigos de tarifa base
            try
            {
                result.TarifasHotel = (await _repositoryCM.FindBySql<TarifaHotelModel>(@$"SELECT 
                    th.IdTarifa AS Id,
                    th.IdHotel,
                    th.CodCategoria AS Categoria,
                    th.Descricao AS Nome,
                    th.IdOrigem,
                    th.CodSegmento
                    FROM 
                    TarifaHotel th
                    WHERE 1 = 1")).AsList();
            }
            catch
            {
                result.TarifasHotel = new List<TarifaHotelModel>();
            }

            // 6. Segmento (CM): códigos de segmento base
            try
            {
                result.SegmentoReserva = (await _repositoryCM.FindBySql<SegmentoReservaModel>(@$"SELECT
                    seg.IdHotel,
                    seg.CodSegmento AS Codigo,
                    seg.Descricao AS Nome
                    FROM 
                    Segmento seg
                    WHERE
                    seg.ANALITICASINTETIC = 'A' AND 
                    seg.FLGATIVO = 'S'")).AsList();
            }
            catch
            {
                result.SegmentoReserva = new List<SegmentoReservaModel>();
            }

            // 7. Código de Pensão Padrão: regimes de alimentação
            result.CodigosPensao = new List<VhfConfigOpcaoItem>
            {
                new() { Value = "N", Label = "Sem alimentação (N)" },
                new() { Value = "C", Label = "Café da Manhã (C)" },
                new() { Value = "M", Label = "Meia pensão - Café e Almoço (M)" },
                new() { Value = "J", Label = "Meia pensão - Café e Jantar (J)" }
            };

            await _cacheStore.AddAsync("Opcoes_Configuracoes_", result, DateTimeOffset.Now.AddMinutes(10), 2, _repository.CancellationToken);

            return result;
        }

        public async Task<List<VhfConfigModel>> GetAllAsync()
        {
            var configs = await _repository.FindByHql<ConfigReservaVhf>(
                "From ConfigReservaVhf c Order by c.Id");
            var empresas = await _frameworkService.SearchCompany(new EmpresaSearchModel());
            var empresaLookup = empresas?.ToDictionary(e => e.Id ?? 0, e => e.PessoaEmpresa?.Nome ?? e.Codigo ?? e.Id?.ToString() ?? "") ?? new Dictionary<int, string>();

            return configs.Select(c => MapToModel(c, empresaLookup)).ToList();
        }

        public async Task<VhfConfigModel?> GetByIdAsync(int id)
        {
            var configs = await _repository.FindByHql<ConfigReservaVhf>(
                "From ConfigReservaVhf c Where c.Id = :id", session: null, new SW_Utils.Auxiliar.Parameter("id", id));
            var config = configs.FirstOrDefault();
            if (config == null) return null;

            var empresas = await _frameworkService.SearchCompany(new EmpresaSearchModel());
            var empresaLookup = empresas?.ToDictionary(e => e.Id ?? 0, e => e.PessoaEmpresa?.Nome ?? e.Codigo ?? e.Id?.ToString() ?? "") ?? new Dictionary<int, string>();
            return MapToModel(config, empresaLookup);
        }

        public async Task<VhfConfigModel> CreateAsync(VhfConfigInputModel model)
        {
            ValidateInput(model);
            _repository.BeginTransaction();

            try
            {
                var loggedUser = await _repository.GetLoggedUser();
                var userId = (loggedUser.HasValue && !string.IsNullOrEmpty(loggedUser.Value.userId) && int.TryParse(loggedUser.Value.userId, out var uid))
                    ? (int?)uid : null;
                var config = new ConfigReservaVhf
                {
                    TipoUtilizacao = model.TipoUtilizacao ?? string.Empty,
                    TipoNegocio = model.TipoNegocio,
                    HotelId = model.HotelId,
                    TipoHospede = model.TipoHospede ?? string.Empty,
                    TipoHospedeCrianca1 = model.TipoHospedeCrianca1,
                    TipoHospedeCrianca2 = model.TipoHospedeCrianca2,
                    Origem = model.Origem ?? string.Empty,
                    TarifaHotel = model.TarifaHotel ?? string.Empty,
                    EncaixarSemanaSeHouver = model.EncaixarSemanaSeHouver,
                    Segmento = model.Segmento,
                    CodigoPensao = model.CodigoPensao ?? string.Empty,
                    PermiteIntercambioMultipropriedade = model.PermiteIntercambioMultipropriedade,
                    OcupacaoMaxRetDispTS = model.OcupacaoMaxRetDispTS,
                    OcupacaoMaxRetDispMP = model.OcupacaoMaxRetDispMP,
                    DataHoraCriacao = DateTime.Now,
                    UsuarioCriacao = userId
                };

                await _repository.Save(config);
                await _repository.CommitAsync();

                var empresas = await _frameworkService.SearchCompany(new EmpresaSearchModel());
                var empresaLookup = empresas?.ToDictionary(e => e.Id ?? 0, e => e.PessoaEmpresa?.Nome ?? e.Codigo ?? e.Id?.ToString() ?? "") ?? new Dictionary<int, string>();
                return MapToModel(config, empresaLookup);
            }
            catch
            {
                _repository.Rollback();
                throw;
            }
        }

        public async Task<VhfConfigModel> UpdateAsync( VhfConfigInputModel model)
        {
            ValidateInput(model);
            _repository.BeginTransaction();

            try
            {
                var configs = await _repository.FindByHql<ConfigReservaVhf>(
                    "From ConfigReservaVhf c Where c.Id = :id", session: null, new SW_Utils.Auxiliar.Parameter("id", model.Id));
                var config = configs.FirstOrDefault();
                if (config == null)
                    throw new ArgumentException($"Configuração com ID {model.Id} não encontrada");

                var loggedUser = await _repository.GetLoggedUser();
                var alteracaoUserId = (loggedUser.HasValue && !string.IsNullOrEmpty(loggedUser.Value.userId) && int.TryParse(loggedUser.Value.userId, out var uidAlt))
                    ? (int?)uidAlt : null;

                config.TipoUtilizacao = model.TipoUtilizacao ?? string.Empty;
                config.TipoNegocio = model.TipoNegocio;
                config.HotelId = model.HotelId;
                config.TipoHospede = model.TipoHospede ?? string.Empty;
                config.TipoHospedeCrianca1 = model.TipoHospedeCrianca1;
                config.TipoHospedeCrianca2 = model.TipoHospedeCrianca2;
                config.Origem = model.Origem ?? string.Empty;
                config.TarifaHotel = model.TarifaHotel ?? string.Empty;
                config.EncaixarSemanaSeHouver = model.EncaixarSemanaSeHouver;
                config.Segmento = model.Segmento;
                config.CodigoPensao = model.CodigoPensao ?? string.Empty;
                config.PermiteIntercambioMultipropriedade = model.PermiteIntercambioMultipropriedade;
                config.OcupacaoMaxRetDispTS = model.OcupacaoMaxRetDispTS;
                config.OcupacaoMaxRetDispMP = model.OcupacaoMaxRetDispMP;
                config.DataHoraAlteracao = DateTime.Now;
                config.UsuarioAlteracao = alteracaoUserId;

                await _repository.Save(config);
                await _repository.CommitAsync();

                var empresas = await _frameworkService.SearchCompany(new EmpresaSearchModel());
                var empresaLookup = empresas?.ToDictionary(e => e.Id ?? 0, e => e.PessoaEmpresa?.Nome ?? e.Codigo ?? e.Id?.ToString() ?? "") ?? new Dictionary<int, string>();
                return MapToModel(config, empresaLookup);
            }
            catch
            {
                _repository.Rollback();
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _repository.BeginTransaction();
            try
            {
                var configs = await _repository.FindByHql<ConfigReservaVhf>(
                    "From ConfigReservaVhf c Where c.Id = :id", session: null, new SW_Utils.Auxiliar.Parameter("id", id));
                var config = configs.FirstOrDefault();
                if (config == null)
                    throw new ArgumentException($"Configuração com ID {id} não encontrada");

                await _repository.Remove(config);
                await _repository.CommitAsync();
                return true;
            }
            catch
            {
                _repository.Rollback();
                throw;
            }
        }

        private static void ValidateInput(VhfConfigInputModel model)
        {
            if (string.IsNullOrWhiteSpace(model.TipoUtilizacao))
                throw new ArgumentException("Tipo de utilização deve ser informado");
            if (string.IsNullOrWhiteSpace(model.TipoNegocio))
                throw new ArgumentException("Tipo de negócio deve ser informado");
            if (string.IsNullOrWhiteSpace(model.TipoHospede))
                throw new ArgumentException("Tipo de hóspede deve ser informado");
            if (string.IsNullOrWhiteSpace(model.Origem))
                throw new ArgumentException("Origem deve ser informada");
            if (string.IsNullOrWhiteSpace(model.TarifaHotel))
                throw new ArgumentException("Tarifa hotel deve ser informada");
            if (string.IsNullOrWhiteSpace(model.Segmento))
                throw new ArgumentException("Segmento deve ser informado");
            if (string.IsNullOrWhiteSpace(model.CodigoPensao))
                throw new ArgumentException("Código de pensão deve ser informado");
        }

        private static VhfConfigModel MapToModel(ConfigReservaVhf c, Dictionary<int, string> empresaLookup)
        {
            return new VhfConfigModel
            {
                Id = c.Id,
                TipoUtilizacao = c.TipoUtilizacao,
                TipoNegocio = c.TipoNegocio,
                HotelId = c.HotelId,
                HotelNome = c.HotelId.HasValue && empresaLookup.TryGetValue(c.HotelId.Value, out var nome) ? nome : null,
                TipoHospedeAdulto = c.TipoHospede,
                TipoHospedeCrianca1 = c.TipoHospedeCrianca1,
                TipoHospedeCrianca2 = c.TipoHospedeCrianca2,
                Origem = c.Origem,
                TarifaHotel = c.TarifaHotel,
                EncaixarSemanaSeHouver = c.EncaixarSemanaSeHouver,
                Segmento = c.Segmento,
                CodigoPensao = c.CodigoPensao,
                PermiteIntercambioMultipropriedade = c.PermiteIntercambioMultipropriedade,
                OcupacaoMaxRetDispTS = c.OcupacaoMaxRetDispTS,
                OcupacaoMaxRetDispMP = c.OcupacaoMaxRetDispMP,
                DataHoraCriacao = c.DataHoraCriacao,
                DataHoraAlteracao = c.DataHoraAlteracao,
                UsuarioCriacao = c.UsuarioCriacao,
                UsuarioAlteracao = c.UsuarioAlteracao
            };
        }
    }
}
