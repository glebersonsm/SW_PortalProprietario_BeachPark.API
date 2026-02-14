using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models.FrameworkModels;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Geral;

namespace SW_PortalProprietario.Application.Services.Core
{
    public class VhfConfigService : IVhfConfigService
    {
        private readonly IFrameworkService _frameworkService;
        private readonly IRepositoryNH _repository;

        public VhfConfigService(IFrameworkService frameworkService, IRepositoryNH repository)
        {
            _frameworkService = frameworkService;
            _repository = repository;
        }

        public async Task<VhfConfigOpcoesModel> GetOpcoesAsync()
        {
            var result = new VhfConfigOpcoesModel();

            // 1. Tipo de utilização (fixo)
            result.TipoUtilizacao = new List<VhfConfigOpcaoItem>
            {
                new() { Value = "Uso próprio", Label = "Uso próprio" },
                new() { Value = "Uso convidado", Label = "Uso convidado" }
            };

            // 2. Hotéis (CM): unidades do cadastro de Empresas
            try
            {
                var empresas = await _frameworkService.SearchCompany(new EmpresaSearchModel());
                if (empresas != null)
                {
                    result.Hoteis = empresas
                        .Where(e => e.Id.HasValue)
                        .Select(e => new VhfConfigOpcaoItem
                        {
                            Value = e.Id!.Value.ToString(),
                            Label = e.PessoaEmpresa?.Nome ?? e.Codigo ?? e.Id.Value.ToString()
                        })
                        .ToList();
                }
            }
            catch
            {
                result.Hoteis = new List<VhfConfigOpcaoItem>();
            }

            // 3. Tipo de Hóspede (CM): categorias padrão
            result.TiposHospede = new List<VhfConfigOpcaoItem>
            {
                new() { Value = "Lazer", Label = "Lazer" },
                new() { Value = "Corporativo", Label = "Corporativo" },
                new() { Value = "Grupo", Label = "Grupo" },
                new() { Value = "Evento", Label = "Evento" }
            };

            // 4. Origem (CM): canais de venda
            result.Origens = new List<VhfConfigOpcaoItem>
            {
                new() { Value = "Direto", Label = "Direto" },
                new() { Value = "Motor de Reservas", Label = "Motor de Reservas" },
                new() { Value = "Central de Reservas", Label = "Central de Reservas" },
                new() { Value = "Website", Label = "Website" },
                new() { Value = "Agência", Label = "Agência" }
            };

            // 5. Tarifa Hotel (CM): códigos de tarifa base
            result.TarifasHotel = new List<VhfConfigOpcaoItem>
            {
                new() { Value = "BAR", Label = "BAR (Best Available Rate)" },
                new() { Value = "Acordo", Label = "Acordo" },
                new() { Value = "Corporativo", Label = "Corporativo" },
                new() { Value = "Grupo", Label = "Grupo" },
                new() { Value = "Promocional", Label = "Promocional" }
            };

            // 6. Código de Pensão Padrão: regimes de alimentação
            result.CodigosPensao = new List<VhfConfigOpcaoItem>
            {
                new() { Value = "SO", Label = "Sem alimentação (SO)" },
                new() { Value = "BB", Label = "Café da Manhã (BB)" },
                new() { Value = "HB", Label = "Meia Pensão (HB)" },
                new() { Value = "FB", Label = "Pensão Completa (FB)" },
                new() { Value = "MAP", Label = "MAP (Meia Pensão)" },
                new() { Value = "AP", Label = "All Inclusive (AP)" }
            };

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
                    HotelId = model.HotelId,
                    TipoHospede = model.TipoHospede ?? string.Empty,
                    Origem = model.Origem ?? string.Empty,
                    TarifaHotel = model.TarifaHotel ?? string.Empty,
                    CodigoPensao = model.CodigoPensao ?? string.Empty,
                    PermiteIntercambioMultipropriedade = model.PermiteIntercambioMultipropriedade,
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

        public async Task<VhfConfigModel> UpdateAsync(int id, VhfConfigInputModel model)
        {
            ValidateInput(model);
            _repository.BeginTransaction();

            try
            {
                var configs = await _repository.FindByHql<ConfigReservaVhf>(
                    "From ConfigReservaVhf c Where c.Id = :id", session: null, new SW_Utils.Auxiliar.Parameter("id", id));
                var config = configs.FirstOrDefault();
                if (config == null)
                    throw new ArgumentException($"Configuração com ID {id} não encontrada");

                var loggedUser = await _repository.GetLoggedUser();
                var alteracaoUserId = (loggedUser.HasValue && !string.IsNullOrEmpty(loggedUser.Value.userId) && int.TryParse(loggedUser.Value.userId, out var uidAlt))
                    ? (int?)uidAlt : null;

                config.TipoUtilizacao = model.TipoUtilizacao ?? string.Empty;
                config.HotelId = model.HotelId;
                config.TipoHospede = model.TipoHospede ?? string.Empty;
                config.Origem = model.Origem ?? string.Empty;
                config.TarifaHotel = model.TarifaHotel ?? string.Empty;
                config.CodigoPensao = model.CodigoPensao ?? string.Empty;
                config.PermiteIntercambioMultipropriedade = model.PermiteIntercambioMultipropriedade;
                config.DataHoraAlteracao = DateTime.Now;
                config.UsuarioAlteracao = alteracaoUserId;

                await _repository.Update(config);
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

                await _repository.Delete(config);
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
            if (string.IsNullOrWhiteSpace(model.TipoHospede))
                throw new ArgumentException("Tipo de hóspede deve ser informado");
            if (string.IsNullOrWhiteSpace(model.Origem))
                throw new ArgumentException("Origem deve ser informada");
            if (string.IsNullOrWhiteSpace(model.TarifaHotel))
                throw new ArgumentException("Tarifa hotel deve ser informada");
            if (string.IsNullOrWhiteSpace(model.CodigoPensao))
                throw new ArgumentException("Código de pensão deve ser informado");
        }

        private static VhfConfigModel MapToModel(ConfigReservaVhf c, Dictionary<int, string> empresaLookup)
        {
            return new VhfConfigModel
            {
                Id = c.Id,
                TipoUtilizacao = c.TipoUtilizacao,
                HotelId = c.HotelId,
                HotelNome = c.HotelId.HasValue && empresaLookup.TryGetValue(c.HotelId.Value, out var nome) ? nome : null,
                TipoHospede = c.TipoHospede,
                Origem = c.Origem,
                TarifaHotel = c.TarifaHotel,
                CodigoPensao = c.CodigoPensao,
                PermiteIntercambioMultipropriedade = c.PermiteIntercambioMultipropriedade,
                DataHoraCriacao = c.DataHoraCriacao,
                DataHoraAlteracao = c.DataHoraAlteracao,
                UsuarioCriacao = c.UsuarioCriacao,
                UsuarioAlteracao = c.UsuarioAlteracao
            };
        }
    }
}
