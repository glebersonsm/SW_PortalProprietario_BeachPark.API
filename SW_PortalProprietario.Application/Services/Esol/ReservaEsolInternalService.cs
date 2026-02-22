using EsolutionPortalDomain.Portal;
using EsolutionPortalDomain.ReservasApiModels.Hotel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Interfaces.Esol;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_Utils.Auxiliar;
using System.Net;
using System.Text;

namespace SW_PortalProprietario.Application.Services.Esol
{
    /// <summary>
    /// Serviço interno que executa operações de reserva/agendamento diretamente no banco eSolution Portal.
    /// Substitui todas as chamadas HTTP à SwReserva/Esolution API.
    /// </summary>
    public class ReservaEsolInternalService : IReservaEsolInternalService
    {
        private readonly IRepositoryNHEsolPortal _repository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ReservaEsolInternalService> _logger;

        public ReservaEsolInternalService(
            IRepositoryNHEsolPortal repository,
            IConfiguration configuration,
            ILogger<ReservaEsolInternalService> logger)
        {
            _repository = repository;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ResultWithPaginationModel<List<SemanaModel>>?> GetAgendamentosGerais(ReservasMultiPropriedadeSearchModel model)
        {
            var parameters = new List<Parameter>();
            var empresaCondominios = _configuration.GetValue<string>("Esolution:EmpresaCondominios") ?? "1,15,16";

            var sb = new StringBuilder($@"Select
                                        pcd.Id, 
                                        pcd.DataInicial, 
                                        pcd.DataFinal, 
                                        ct.Id as CotaId, 
                                        ct.Nome as CotaNome, 
                                        uc.Id as UhCondominioId, 
                                        uc.Numero as UhCondominioNumero, 
                                        case
                                        when Lower(i.Nome) like '%interc%' then 'I'
                                        when Coalesce(i.Pool,'N') = 'S' then 'P'
                                        when Coalesce(pcd.TipoDisponibilizacao,r.TipoUtilizacao) = 'I' then 'I'
                                        when Coalesce(pcd.TipoDisponibilizacao,r.TipoUtilizacao) = 'C' then 'C'
                                        else 'U' end as TipoDisponibilizacao,
                                        case
                                        when Lower(i.Nome) like '%interc%' then 'Intercambiadora'
                                        when Coalesce(i.Pool,'N') = 'S' then 'Pool'
                                        when Coalesce(pcd.TipoDisponibilizacao,r.TipoUtilizacao) = 'I' then 'Intercambiadora'
                                        when Coalesce(pcd.TipoDisponibilizacao,r.TipoUtilizacao) = 'C' then 'Uso convidado'
                                        else 'Uso proprietário' end as TipoDisponibilizacaoNome,
                                        pes.Nome as NomeProprietario,
                                        pes.Cpf as DocumentoProprietario,
                                        pcd.Id as PeriodoCotaDisponibilidadeId,
                                        Cast(Year(pcd.DataInicial) as varchar) as Ano,
                                        pcd.Inventario as InventarioId,
                                        Coalesce(i.Pool,'N') as InventarioPool,
                                        Case when Coalesce(i.Pool,'N') = 'S' then 1 else 0 end as PodeRetirarDoPool, 
                                        Case when Coalesce(i.Pool,'S') = 'N' then 1 else 0 end as PodeLiberarParaPool,
                                        1 as PodeForcarAlteracao,
                                        'Sem reserva' as ReservasVinculadas,
                                        (Select ts.Nome From TipoSemana ts
                                         Inner Join Semana s on s.TipoSemana = ts.Id
                                         Inner Join SemanaDisponibilidade sd on sd.Semana = s.Id
                                         Where sd.DataHoraExclusao is null and sd.UsuarioExclusao is null
                                           and s.DataHoraExclusao is null and s.UsuarioExclusao is null
                                           and sd.PeriodoCotaDisponibilidade = pcd.Id) as TipoSemana,
                                        tuh.Capacidade
                                        From PeriodoCotaDisponibilidade pcd 
                                        Inner Join Cota ct on ct.Id = pcd.Cota
                                        Inner Join GrupoCotas gc on ct.GrupoCotas = gc.Id and gc.Empresa in ({empresaCondominios})
                                        Inner Join UhCondominio uc on uc.Id = pcd.UhCondominio
                                        Inner Join CotaProprietario cp on cp.UhCondominio = pcd.UhCondominio and cp.Cota = pcd.Cota and cp.UsuarioExclusao is null and cp.DataHoraExclusao is null
                                        Inner Join Proprietario pro on cp.Id = pro.CotaProprietario and pro.UsuarioExclusao is null and pro.DataHoraExclusao is null
                                        Inner Join Cliente cli on cli.Id = pro.Cliente and pro.DataHoraExclusao is null
                                        Inner Join Pessoa pes on pes.Id = cli.Pessoa 
                                        Left Join Inventario i on pcd.Inventario = i.Id
                                        Left Join Uh u on u.UhCondominio = uc.Id
                                        Left Join TipoUh tuh on u.TipoUh = tuh.Id
                                        Left Join Reserva r on r.PeriodoCotaDisponibilidade = pcd.Id and r.Status <> 'CL'
                                        Where pcd.DataHoraExclusao is null ");

            if (model.CotaProprietarioId > 0)
                sb.AppendLine($" and cp.Id = {model.CotaProprietarioId} ");

            if (model.Ano > 0)
                sb.AppendLine($" and year(pcd.datainicial) = {model.Ano} ");

            if (!string.IsNullOrEmpty(model.NomeProprietario))
            {
                var nomeParam = model.NomeProprietario.TrimEnd().ToLower().Replace("'", "''");
                sb.AppendLine($" and (Lower(pes.Nome) like '%{nomeParam}%' or Exists(Select r2.PeriodoCotaDisponibilidade From Reserva r2 Inner Join ReservaCliente rc on r2.Id = rc.Reserva Inner Join Cliente c on rc.Cliente = c.Id Inner Join Pessoa cp on c.Pessoa = cp.Id Where r2.PeriodoCotaDisponibilidade = pcd.Id and r2.Status != 'CL' and Lower(cp.Nome) like '%{nomeParam}%'))");
            }

            if (!string.IsNullOrEmpty(model.DocumentoProprietario))
            {
                var docParam = model.DocumentoProprietario.Replace(".", "").Replace("/", "").Replace("-", "").Replace("'", "''");
                sb.AppendLine($" AND REPLACE(REPLACE(REPLACE(pes.CPF, '-', ''), '.', ''), '/', '') = '{docParam}' ");
            }

            if (!string.IsNullOrEmpty(model.NumeroApartamento))
            {
                var numParam = model.NumeroApartamento.TrimEnd().ToLower().Replace("'", "''");
                sb.AppendLine($" and Lower(uc.Numero) like '%{numParam}%' ");
            }

            if (model.NumeroApartamentos != null && model.NumeroApartamentos.Any())
            {
                var str = string.Join(",", model.NumeroApartamentos.Where(a => !string.IsNullOrEmpty(a)).Select(a => $"'{a.Replace("'", "''")}'").Distinct());
                if (!string.IsNullOrEmpty(str))
                    sb.AppendLine($" and uc.Numero in ({str}) ");
            }

            if (model.NomeCotas != null && model.NomeCotas.Any())
            {
                var str = string.Join(",", model.NomeCotas.Where(a => !string.IsNullOrEmpty(a)).Select(a => $"'{a.Replace("'", "''")}'").Distinct());
                if (!string.IsNullOrEmpty(str))
                    sb.AppendLine($" and ct.Nome in ({str}) ");
            }

            if (model.CheckinInicial.HasValue)
            {
                sb.AppendLine(" and pcd.DataInicial >= @checkinInicial ");
                parameters.Add(new Parameter("checkinInicial", model.CheckinInicial.Value.Date));
            }
            if (model.CheckinFinal.HasValue)
            {
                sb.AppendLine(" and pcd.DataInicial <= @checkinFinal ");
                parameters.Add(new Parameter("checkinFinal", model.CheckinFinal.Value.Date.AddDays(1).AddMilliseconds(-1)));
            }
            if (model.CheckoutInicial.HasValue)
            {
                sb.AppendLine(" and pcd.DataFinal >= @checkoutInicial ");
                parameters.Add(new Parameter("checkoutInicial", model.CheckoutInicial.Value.Date));
            }
            if (model.CheckoutFinal.HasValue)
            {
                sb.AppendLine(" and pcd.DataFinal <= @checkoutFinal ");
                parameters.Add(new Parameter("checkoutFinal", model.CheckoutFinal.Value.Date.AddDays(1).AddMilliseconds(-1)));
            }
            if (model.DataUtilizacaoInicial.HasValue)
            {
                sb.AppendLine(" and pcd.DataInicial >= @utilizacaoInicial ");
                parameters.Add(new Parameter("utilizacaoInicial", model.DataUtilizacaoInicial.Value.Date));
            }
            if (model.DataUtilizacaoFinal.HasValue)
            {
                sb.AppendLine(" and pcd.DataFinal <= @utilizacaoFinal ");
                parameters.Add(new Parameter("utilizacaoFinal", model.DataUtilizacaoFinal.Value.Date.AddDays(1).AddMilliseconds(-1)));
            }
            if (model.Reserva.GetValueOrDefault(0) > 0)
                sb.AppendLine($" And Exists(Select r2.Id From Reserva r2 Where r2.PeriodoCotaDisponibilidade = pcd.Id and r2.Id = {model.Reserva})");

            if (!string.IsNullOrEmpty(model.ComReservas))
            {
                if (model.ComReservas.StartsWith("S", StringComparison.OrdinalIgnoreCase))
                    sb.AppendLine(" And Exists(Select r2.Id From Reserva r2 Where r2.PeriodoCotaDisponibilidade = pcd.Id and r2.Status != 'CL')");
                else if (model.ComReservas.StartsWith("N", StringComparison.OrdinalIgnoreCase))
                    sb.AppendLine(" And not Exists(Select r2.Id From Reserva r2 Where r2.PeriodoCotaDisponibilidade = pcd.Id and r2.Status != 'CL')");
            }

            if (model.PeriodoCotaDisponibilidadeId.GetValueOrDefault(0) > 0)
                sb.AppendLine($" and pcd.Id = {model.PeriodoCotaDisponibilidadeId} ");

            if (model.DataAquisicaoContrato.HasValue)
            {
                sb.AppendLine(" and pcd.DataHoraInclusao >= @dataAquisicaoContrato");
                parameters.Add(new Parameter("dataAquisicaoContrato", model.DataAquisicaoContrato.Value));
            }

            var sql = sb.ToString();
            var pageSize = model.QuantidadeRegistrosRetornar.GetValueOrDefault(0);
            var pageNumber = model.NumeroDaPagina.GetValueOrDefault(1);

            if (pageSize == 0) pageSize = 20;
            if (pageNumber == 0) pageNumber = 1;

            int totalRegistros = 0;
            if (pageSize > 0)
            {
                try
                {
                    totalRegistros = Convert.ToInt32(await _repository.CountTotalEntry(sql, session: null, parameters.ToArray()));
                }
                catch (Exception err)
                {
                    _logger.LogError(err, "Erro ao contar agendamentos");
                    throw;
                }
            }

            long totalPage = pageSize > 0 ? SW_Utils.Functions.Helper.TotalPaginas(pageSize, totalRegistros) : 1;
            if (totalPage > 0 && pageNumber > totalPage)
                pageNumber = (int)totalPage;

            sb.Append(" Order by pcd.Id ");

            IList<SemanaModel> semanas;
            try
            {
                semanas = pageSize > 0
                    ? await _repository.FindBySql<SemanaModel>(sb.ToString(), pageSize, pageNumber, parameters.ToArray())
                    : (await _repository.FindBySql<SemanaModel>(sb.ToString(), parameters.ToArray())).ToList();
            }
            catch (Exception err)
            {
                _logger.LogError(err, "Erro ao consultar agendamentos");
                return new ResultWithPaginationModel<List<SemanaModel>>
                {
                    Data = new List<SemanaModel>(),
                    Success = false,
                    Status = 500,
                    Errors = new List<string> { err.Message }
                };
            }

            if (semanas.Any())
            {
                foreach (var item in semanas)
                {
                    if (item.DataInicial.GetValueOrDefault().DayOfWeek == DayOfWeek.Saturday &&
                        item.DataFinal.GetValueOrDefault().DayOfWeek == DayOfWeek.Friday)
                        item.DataFinal = item.DataFinal.GetValueOrDefault().Date.AddDays(1);

                    if (!string.IsNullOrEmpty(item.DocumentoProprietario))
                    {
                        var doc = SW_Utils.Functions.Helper.ApenasNumeros(item.DocumentoProprietario);
                        if (doc.Length == 11)
                            item.PessoaTitular1CPF = item.DocumentoProprietario;
                        else if (doc.Length == 14)
                            item.PessoaTitualar1CNPJ = item.DocumentoProprietario;
                    }
                }
            }

            return new ResultWithPaginationModel<List<SemanaModel>>(semanas.ToList())
            {
                Success = true,
                Status = 200,
                PageNumber = pageNumber,
                LastPageNumber = (int)Math.Max(1, totalPage),
                NumberRecords = semanas.Count
            };
        }

        public Task<ResultWithPaginationModel<List<SemanaModel>>?> GetConsultarMeusAgendamentos(PeriodoCotaDisponibilidadeUsuarioSearchModel model)
        {
            var searchModel = new ReservasMultiPropriedadeSearchModel
            {
                Ano = int.TryParse(model.Ano, out var ano) ? ano : DateTime.Today.Year,
                QuantidadeRegistrosRetornar = model.QuantidadeRegistrosRetornar,
                NumeroDaPagina = model.NumeroDaPagina,
                NomeCotas = !string.IsNullOrEmpty(model.CotaNome) ? new List<string> { model.CotaNome } : null,
                NumeroApartamentos = !string.IsNullOrEmpty(model.ImovelNumero) ? new List<string> { model.ImovelNumero } : null,
                DataAquisicaoContrato = model.DataAquisicaoContrato,
                PeriodoCotaDisponibilidadeId = !string.IsNullOrEmpty(model.AgendamentoId) && int.TryParse(model.AgendamentoId, out var pcdId) ? pcdId : null
            };
            return GetAgendamentosGerais(searchModel);
        }

        public async Task<ResultModel<List<ReservaModel>>?> ConsultarReservaByAgendamentoId(string agendamentoId)
        {
            if (string.IsNullOrEmpty(agendamentoId) || !int.TryParse(agendamentoId, out var id) || id <= 0)
                return new ResultModel<List<ReservaModel>> { Success = false, Errors = new List<string> { "AgendamentoId inválido" } };

            try
            {
                var sql = $@"Select r.Id, r.DataHora as DataReserva, r.Checkin, r.Checkout,
                    Case when r.Status in ('CO','CP') then 'CO - Checkout' when r.Status = 'CI' then 'CI - Checkin' when r.Status = 'CL' then 'CL - Cancelada'
                    when r.Status = 'AC' then 'AC - À confirmar' when r.Status = 'CF' then 'CF - Confirmada' when r.Status = 'NS' then 'NS - NoShow' end as Status,
                    r.TipoPensao, th.Nome as TipoHospede, tuh.Nome as TipoUhNome, r.QuantidadeAdulto as Adultos, r.QuantidadeCrianca1 as Criancas1, r.QuantidadeCrianca2 as Criancas2,
                    h.Nome as HotelNome, pc.Nome as NomeHospede, r.ProprietarioUh as ProprietarioId, r.UhProprietario as UhCondominioId, r.PeriodoCotaDisponibilidade as PeriodoCotaDisponibilidadeId,
                    procp.Nome as ProprietarioNome, procp.Cpf as ProprietarioCpfCnpj,
                    (Select Max(ts.Nome) From TipoSemana ts Inner Join Semana s on s.TipoSemana = ts.Id Inner Join SemanaDisponibilidade sd on sd.Semana = s.Id
                    Where sd.DataHoraExclusao is null and sd.UsuarioExclusao is null and s.DataHoraExclusao is null and s.UsuarioExclusao is null and sd.PeriodoCotaDisponibilidade = pcd.Id) as TipoSemana,
                    Case When r.TipoUtilizacao = 'U' or pcd.TipoDisponibilizacao = 'U' then 'UP' When r.TipoUtilizacao = 'C' or pcd.TipoDisponibilizacao = 'C' then 'UC' When r.TipoUtilizacao = 'I' or pcd.TipoDisponibilizacao = 'I' then 'I' end as TipoUtilizacao
                    From Reserva r Inner Join PeriodoCotaDisponibilidade pcd on r.PeriodoCotaDisponibilidade = pcd.Id
                    Left Outer Join ReservaCliente rc on rc.Reserva = r.Id and rc.Principal = 'S'
                    Left Outer Join Cliente rcc on rc.Cliente = rcc.Id Left Outer Join Pessoa pc on rcc.Pessoa = pc.Id
                    Left Outer Join TipoUh tuh on r.TipoUh = tuh.Id Left Outer Join Hotel h on r.Hotel = h.Id
                    Inner Join CotaProprietario cp on cp.UhCondominio = pcd.UhCondominio and cp.Cota = pcd.Cota and cp.UsuarioExclusao is null and cp.DataHoraExclusao is null
                    Inner Join Proprietario pro on pro.CotaProprietario = cp.Id and pro.DataHoraExclusao is null
                    Inner Join Cliente proca on pro.Cliente = proca.Id Inner Join Pessoa procp on proca.Pessoa = procp.Id
                    Left Outer Join TipoHospede th on r.TipoHospede = th.Id
                    Where pro.Id = (Select Max(pro1.Id) From Proprietario pro1 Where pro1.CotaProprietario = cp.Id and pro1.Cliente = proca.Id and pro1.DataHoraExclusao is null)
                    and pcd.Id = {id} and r.Status <> 'CL'";
                var reservas = (await _repository.FindBySql<ReservaModel>(sql)).ToList();
                return new ResultModel<List<ReservaModel>>(reservas) { Success = true, Status = (int)HttpStatusCode.OK };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar reservas por agendamento");
                return new ResultModel<List<ReservaModel>> { Success = false, Status = 500, Errors = new List<string> { ex.Message } };
            }
        }

        public Task<ResultModel<List<ReservaModel>>?> ConsultarMinhasReservaByAgendamentoId(string agendamentoId)
            => ConsultarReservaByAgendamentoId(agendamentoId);

        public async Task<ResultModel<int>?> SalvarReservaEmAgendamento(CriacaoReservaAgendamentoInputModel modelReserva)
        {
            throw new NotImplementedException("SalvarReservaEmAgendamento requer portação completa do InventarioService.EfetuarOuAlterarReservasAgendamento. Use a API SwReserva temporariamente ou implemente a lógica de criação de Reserva/ReservaCliente/ReservaDiaria.");
        }

        public async Task<ResultModel<bool>?> CancelarReservaAgendamento(CancelamentoReservaAgendamentoModel model)
        {
            try
            {
                _repository.BeginTransaction();
                var usuario = await _repository.GetLoggedUser();
                var usuarioId = usuario?.userId != null ? int.Parse(usuario.Value.userId) : 56;
                await _repository.ExecuteSqlCommand($"UPDATE Reserva SET Status = 'CL', DataHoraCancelamento = GETDATE(), UsuarioCancelamento = {usuarioId}, ObservacaoCancelamento = 'Reserva cancelada via Portal do Cliente' WHERE Id = {model.ReservaId}");
                var (executed, ex) = await _repository.CommitAsync();
                if (executed) return new ResultModel<bool>(true) { Success = true, Status = 200 };
                throw ex ?? new Exception("Falha ao cancelar");
            }
            catch (Exception ex)
            {
                _repository.Rollback();
                _logger.LogError(ex, "Erro ao cancelar reserva");
                return new ResultModel<bool> { Success = false, Status = 500, Errors = new List<string> { ex.Message } };
            }
        }

        public Task<ResultModel<bool>?> CancelarMinhaReservaAgendamento(CancelamentoReservaAgendamentoModel model)
            => CancelarReservaAgendamento(model);

        public async Task<ResultModel<ReservaForEditModel>?> EditarReserva(int id)
        {
            var empresaCondominios = _configuration.GetValue<string>("Esolution:EmpresaCondominios") ?? "1,15,16";
            try
            {
                var sql = $@"Select r.Id, r.DataHora as DataReserva, r.Checkin, r.Checkout,
                    Case when r.Status in ('CO','CP') then 'CO - Checkout' when r.Status = 'CI' then 'CI - Checkin' when r.Status = 'CL' then 'CL - Cancelada'
                    when r.Status = 'AC' then 'AC - À confirmar' when r.Status = 'CF' then 'CF - Confirmada' when r.Status = 'NS' then 'NS - NoShow' end as Status,
                    r.TipoPensao, th.Nome as TipoHospede, tuh.Nome as TipoUhNome, r.QuantidadeAdulto as Adultos, r.QuantidadeCrianca1 as Criancas1, r.QuantidadeCrianca2 as Criancas2,
                    h.Nome as HotelNome, pc.Nome as NomeHospede, r.ProprietarioUh as ProprietarioId, r.UhProprietario as UhCondominioId, r.PeriodoCotaDisponibilidade as PeriodoCotaDisponibilidadeId,
                    procp.Nome as ProprietarioNome, procp.Cpf as ProprietarioCpfCnpj, tuh.Capacidade,
                    Case When r.TipoUtilizacao = 'U' or pcd.TipoDisponibilizacao = 'U' then 'UP' When r.TipoUtilizacao = 'C' or pcd.TipoDisponibilizacao = 'C' then 'UC' When r.TipoUtilizacao = 'I' or pcd.TipoDisponibilizacao = 'I' then 'I' end as TipoUtilizacao
                    From Reserva r Inner Join ReservaCliente rc on rc.Reserva = r.Id and rc.Principal = 'S'
                    Inner Join Cliente rcc on rc.Cliente = rcc.Id Inner Join Pessoa pc on rcc.Pessoa = pc.Id
                    Inner Join TipoUh tuh on r.TipoUh = tuh.Id Inner Join Hotel h on r.Hotel = h.Id and h.Condominio in ({empresaCondominios})
                    Inner Join PeriodoCotaDisponibilidade pcd on r.PeriodoCotaDisponibilidade = pcd.Id
                    Inner Join CotaProprietario cp on cp.UhCondominio = pcd.UhCondominio and cp.Cota = pcd.Cota
                    Inner Join Proprietario pro on pro.CotaProprietario = cp.Id and pro.DataHoraExclusao is null
                    Inner Join Cliente proca on pro.Cliente = proca.Id Inner Join Pessoa procp on proca.Pessoa = procp.Id
                    Left Outer Join TipoHospede th on r.TipoHospede = th.Id
                    Where pro.Id = (Select Max(pro1.Id) From Proprietario pro1 Where pro1.CotaProprietario = cp.Id and pro1.Cliente = proca.Id and pro1.DataHoraExclusao is null)
                    and r.Status <> 'CL' and r.Id = {id}";
                var result = (await _repository.FindBySql<ReservaForEditModel>(sql)).FirstOrDefault();
                return new ResultModel<ReservaForEditModel>(result) { Success = result != null, Status = result != null ? 200 : 404 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao editar reserva");
                return new ResultModel<ReservaForEditModel> { Success = false, Status = 500, Errors = new List<string> { ex.Message } };
            }
        }

        public Task<ResultModel<ReservaForEditModel>?> EditarMinhaReserva(int id)
            => EditarReserva(id);

        public async Task<ResultModel<List<InventarioModel>>?> ConsultarInventarios(InventarioSearchModel searchModel)
        {
            if (searchModel.Agendamentoid.GetValueOrDefault(0) <= 0)
                return new ResultModel<List<InventarioModel>> { Success = false, Errors = new List<string> { "AgendamentoId deve ser informado" } };
            var empresaCondominios = _configuration.GetValue<string>("Esolution:EmpresaCondominios") ?? "1,15,16";
            var where = "";
            if (!string.IsNullOrEmpty(searchModel.NoPool) && searchModel.NoPool.StartsWith("s", StringComparison.OrdinalIgnoreCase)) where = " And inv.Pool = 'S'";
            else if (!string.IsNullOrEmpty(searchModel.NoPool) && searchModel.NoPool.StartsWith("n", StringComparison.OrdinalIgnoreCase)) where = " And inv.Pool = 'N'";
            try
            {
                var sql = $@"Select inv.Id, inv.Id as InventarioId, inv.Codigo, inv.Nome, inv.NomeExibicao, inv.Pool,
                    inv.DiasMinimoInicioAgendamentoAdicionar, inv.DiasMinimoInicioAgendamentoRemover, inv.DiasVencidoConsideradoInadimplente,
                    inv.ValidarSituacaoFinanceiraEfetuarReserva, inv.CriarReservaStatusConfirmada, inv.SegmentoMercadoReserva, inv.OrigemReserva, inv.MeioComunicacaoReserva,
                    inv.TipoHospedeProprietarioReserva, inv.TipoHospedeConvidadoReserva, inv.PermitirEfetuarReservaPortalProprietario,
                    inv.PemitirCriarSegundaReservaAgendamento, inv.PemitirReservaFracionada, inv.PermitirUsoConvidado, inv.GerarReservaAutomaticamente
                    From Inventario inv Inner Join Hotel h on h.Id = inv.Hotel Inner Join Empresa emp on emp.Id = h.Condominio and emp.Id in ({empresaCondominios})
                    Inner Join UhCondominio uc on uc.Empresa = emp.Id Inner Join PeriodoCotaDisponibilidade pcd on pcd.UhCondominio = uc.Id
                    Where pcd.Id = {searchModel.Agendamentoid} {where}";
                var inventarios = (await _repository.FindBySql<InventarioModel>(sql)).ToList();
                return new ResultModel<List<InventarioModel>>(inventarios) { Success = true, Status = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar inventários");
                return new ResultModel<List<InventarioModel>> { Success = false, Status = 500, Errors = new List<string> { ex.Message } };
            }
        }

        public async Task<ResultModel<bool>?> RetirarSemanaPool(AgendamentoInventarioModel modelAgendamentoPool)
        {
            var agendamentoId = modelAgendamentoPool.AgendamentoId.GetValueOrDefault(0);
            var inventarioId = modelAgendamentoPool.InventarioId.GetValueOrDefault(0);
            if (agendamentoId <= 0 || inventarioId <= 0)
                return new ResultModel<bool> { Success = false, Status = 400, Errors = new List<string> { "AgendamentoId e InventarioId devem ser informados" } };
            try
            {
                _repository.BeginTransaction();
                await _repository.ExecuteSqlCommand($"UPDATE PeriodoCotaDisponibilidade SET Inventario = {inventarioId}, TipoDisponibilizacao = 'U' WHERE Id = {agendamentoId}");
                var (executed, ex) = await _repository.CommitAsync();
                if (executed) return new ResultModel<bool>(true) { Success = true, Status = 200 };
                throw ex ?? new Exception("Falha ao retirar do pool");
            }
            catch (Exception ex)
            {
                _repository.Rollback();
                _logger.LogError(ex, "Erro ao retirar semana do pool");
                return new ResultModel<bool> { Success = false, Status = 500, Errors = new List<string> { ex.Message } };
            }
        }

        public async Task<ResultModel<bool>?> LiberarSemanaPool(LiberacaoAgendamentoInputModel modelAgendamentoPool)
        {
            var agendamentoId = modelAgendamentoPool.AgendamentoId.GetValueOrDefault(0);
            var inventarioId = modelAgendamentoPool.InventarioId.GetValueOrDefault(0);
            if (agendamentoId <= 0 || inventarioId <= 0)
                return new ResultModel<bool> { Success = false, Status = 400, Errors = new List<string> { "AgendamentoId e InventarioId devem ser informados" } };
            try
            {
                _repository.BeginTransaction();
                await _repository.ExecuteSqlCommand($"UPDATE Reserva SET Status = 'CL', DataHoraCancelamento = GETDATE() WHERE PeriodoCotaDisponibilidade = {agendamentoId}");
                await _repository.ExecuteSqlCommand($"UPDATE PeriodoCotaDisponibilidade SET Inventario = {inventarioId}, TipoDisponibilizacao = 'P' WHERE Id = {agendamentoId}");
                var (executed, ex) = await _repository.CommitAsync();
                if (executed) return new ResultModel<bool>(true) { Success = true, Status = 200 };
                throw ex ?? new Exception("Falha ao liberar para pool");
            }
            catch (Exception ex)
            {
                _repository.Rollback();
                _logger.LogError(ex, "Erro ao liberar semana para pool");
                return new ResultModel<bool> { Success = false, Status = 500, Errors = new List<string> { ex.Message } };
            }
        }

        public async Task<ResultModel<int>?> TrocarSemana(TrocaSemanaInputModel model)
        {
            throw new NotImplementedException("TrocarSemana requer portação completa do InventarioService. Use a API SwReserva temporariamente.");
        }

        public async Task<ResultModel<int>?> IncluirSemana(IncluirSemanaInputModel model)
        {
            throw new NotImplementedException("IncluirSemana requer portação completa do InventarioService. Use a API SwReserva temporariamente.");
        }

        public async Task<DadosImpressaoVoucherResultModel?> GetDadosImpressaoVoucher(string agendamentoId)
        {
            if (string.IsNullOrEmpty(agendamentoId) || !int.TryParse(agendamentoId, out var id) || id <= 0)
                return null;
            try
            {
                var sql = $@"Select Cast(r.Id as varchar) as NumeroReserva, r.PeriodoCotaDisponibilidade as AgendamentoId, procp.Nome as Cliente, pc.Nome as HospedePrincipal,
                    pcd.TipoDisponibilizacao, Case when pcd.TipoDisponibilizacao = 'U' then 'Uso Próprio' when pcd.TipoDisponibilizacao = 'C' then 'Uso Convidado' when pcd.TipoDisponibilizacao = 'P' then 'Pool' when pcd.TipoDisponibilizacao = 'I' then 'Uso/Intercambiadora' else pcd.TipoDisponibilizacao end as TipoUso,
                    Format(Coalesce(r.Checkin,r.CheckinPrevisao),'dd/MM/yyyy') as DataChegada, Format(Coalesce(r.Checkout,r.CheckoutPrevisao),'dd/MM/yyyy') as DataPartida,
                    (Coalesce(r.QuantidadeAdulto,0)+Coalesce(r.QuantidadeCrianca1,0)+Coalesce(r.QuantidadeCrianca2,0)) as QuantidadePaxPorFaixaEtaria,
                    tuh.Nome as TipoApartamento, pcd.UhCondominio as UhCondominioId, cp.Cota as CotaPortalId, h.Nome as NomeHotel,
                    Coalesce(r.QuantidadeAdulto,0) as QuantidadeAdulto, Coalesce(r.QuantidadeCrianca1,0) as QuantidadeCrianca1, Coalesce(r.QuantidadeCrianca2,0) as QuantidadeCrianca2,
                    ct.Nome as CotaNome, uc.Numero as UhCondominioNumero
                    From Reserva r Inner Join PeriodoCotaDisponibilidade pcd on r.PeriodoCotaDisponibilidade = pcd.Id
                    Left Outer Join ReservaCliente rc on rc.Reserva = r.Id and rc.Principal = 'S'
                    Left Outer Join Cliente rcc on rc.Cliente = rcc.Id Left Outer Join Pessoa pc on rcc.Pessoa = pc.Id
                    Left Outer Join TipoUh tuh on r.TipoUh = tuh.Id Left Outer Join Hotel h on r.Hotel = h.Id
                    Inner Join CotaProprietario cp on cp.UhCondominio = pcd.UhCondominio and cp.Cota = pcd.Cota and cp.UsuarioExclusao is null and cp.DataHoraExclusao is null
                    Inner Join Proprietario pro on pro.CotaProprietario = cp.Id and pro.DataHoraExclusao is null and pro.UsuarioExclusao is null
                    Inner Join Cliente proca on pro.Cliente = proca.Id Inner Join Pessoa procp on proca.Pessoa = procp.Id
                    Inner Join UhCondominio uc on uc.Id = cp.UhCondominio Inner Join Cota ct on ct.Id = cp.Cota
                    Where pro.Id = (Select Max(pro1.Id) From Proprietario pro1 Where pro1.CotaProprietario = cp.Id and pro1.Cliente = proca.Id and pro1.DataHoraExclusao is null and pro1.UsuarioExclusao is null)
                    and pcd.Id = {id} and r.Status <> 'CL'";
                return (await _repository.FindBySql<DadosImpressaoVoucherResultModel>(sql)).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter dados impressão voucher");
                return null;
            }
        }
    }
}
