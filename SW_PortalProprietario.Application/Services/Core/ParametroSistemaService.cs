using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;
using SW_PortalProprietario.Domain.Entities.Core.Framework;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;
using SW_Utils.Auxiliar;
using System.Diagnostics;
using System.Text;

namespace SW_PortalProprietario.Application.Services.Core
{
    public class ParametroSistemaService : IParametroSistemaService
    {
        private readonly IRepositoryNH _repository;
        private readonly ILogger<ParametroSistemaService> _logger;
        private readonly IProjectObjectMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IServiceBase _serviceBase;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ParametroSistemaService(IRepositoryNH repository,
            ILogger<ParametroSistemaService> logger,
            IProjectObjectMapper mapper,
            IConfiguration configuration,
            IServiceBase serviceBase,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _configuration = configuration;
            _serviceBase = serviceBase;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ParametroSistemaViewModel?> SaveParameters(ParametroSistemaInputUpdateModel model)
        {
            try
            {

                _repository.BeginTransaction();
                var empresas = (await _repository.FindByHql<Empresa>("From Empresa e Inner Join Fetch e.Pessoa p")).AsList();
                if (empresas.Count() > 1 || empresas.Count() == 0)
                    throw new ArgumentException($"Não foi possível salvar os parâmetros do sistema empCount = {empresas.Count()}");

                var empFirst = empresas.First();

                var httpContext = _httpContextAccessor?.HttpContext?.Request;
                if (httpContext is null)
                    throw new Exception("Não foi possível identificar a URL do servidor");

                var loggedUser = await _repository.GetLoggedUser();
                if (loggedUser == null)
                    throw new Exception("Não foi possível identificar o usuário logado");

                ParametroSistema psBd = (await _repository.FindByHql<ParametroSistema>($"From ParametroSistema ps Inner Join Fetch ps.Empresa emp Where emp.Id = {empFirst.Id}")).FirstOrDefault();

                if (string.IsNullOrEmpty(model.SiteParaReserva) || model.SiteParaReserva.Equals("string", StringComparison.InvariantCultureIgnoreCase))
                {
                    model.SiteParaReserva = psBd != null ? psBd.SiteParaReserva : null;
                }

                if (!model.QtdeMaximaDiasContasAVencer.HasValue || model.QtdeMaximaDiasContasAVencer == 0)
                {
                    model.QtdeMaximaDiasContasAVencer = psBd != null ? psBd.QtdeMaximaDiasContasAVencer : null;
                }

                if (!model.PermitirUsuarioAlterarSeuDoc.HasValue)
                {
                    model.PermitirUsuarioAlterarSeuDoc = psBd != null ? psBd.PermitirUsuarioAlterarSeuDoc : Domain.Enumns.EnumSimNao.Não;
                }

                if (!model.PermitirUsuarioAlterarSeuEmail.HasValue)
                {
                    model.PermitirUsuarioAlterarSeuEmail = psBd != null ? psBd.PermitirUsuarioAlterarSeuEmail : Domain.Enumns.EnumSimNao.Não;
                }

                if (model.AgruparCertidaoPorCliente.GetValueOrDefault(Domain.Enumns.EnumSimNao.Não) == Domain.Enumns.EnumSimNao.Não && model.EmitirCertidaoPorUnidCliente.GetValueOrDefault(Domain.Enumns.EnumSimNao.Não) == Domain.Enumns.EnumSimNao.Não)
                    model.AgruparCertidaoPorCliente = Domain.Enumns.EnumSimNao.Sim;

                if (model.IntegradoComMultiPropriedade.GetValueOrDefault(Domain.Enumns.EnumSimNao.Não) == Domain.Enumns.EnumSimNao.Não && model.IntegradoComTimeSharing.GetValueOrDefault(Domain.Enumns.EnumSimNao.Não) == Domain.Enumns.EnumSimNao.Não)
                {
                    if (psBd != null && (psBd.IntegradoComTimeSharing.GetValueOrDefault(Domain.Enumns.EnumSimNao.Não) != Domain.Enumns.EnumSimNao.Não || psBd.IntegradoComMultiPropriedade.GetValueOrDefault(Domain.Enumns.EnumSimNao.Não) != Domain.Enumns.EnumSimNao.Não))
                    {
                        model.IntegradoComMultiPropriedade = psBd.IntegradoComMultiPropriedade;
                        model.IntegradoComTimeSharing = psBd.IntegradoComTimeSharing;
                    }
                    else
                    {
                        model.IntegradoComMultiPropriedade = Domain.Enumns.EnumSimNao.Sim;
                        model.IntegradoComTimeSharing = Domain.Enumns.EnumSimNao.Não;
                    }
                }

                if (string.IsNullOrEmpty(model.NomeCondominio) || model.NomeCondominio.Equals("string", StringComparison.InvariantCultureIgnoreCase))
                {
                    model.NomeCondominio = psBd != null ? psBd.NomeCondominio : null;
                }

                if (string.IsNullOrEmpty(model.CnpjCondominio) || model.CnpjCondominio.Equals("string", StringComparison.InvariantCultureIgnoreCase))
                {
                    model.CnpjCondominio = psBd != null ? psBd.CnpjCondominio : null;
                }

                if (string.IsNullOrEmpty(model.EnderecoCondominio) || model.EnderecoCondominio.Equals("string", StringComparison.InvariantCultureIgnoreCase))
                {
                    model.EnderecoCondominio = psBd != null ? psBd.EnderecoCondominio : null;
                }

                if (string.IsNullOrEmpty(model.NomeAdministradoraCondominio) || model.NomeAdministradoraCondominio.Equals("string", StringComparison.InvariantCultureIgnoreCase))
                {
                    model.NomeAdministradoraCondominio = psBd != null ? psBd.NomeAdministradoraCondominio : null;
                }

                if (string.IsNullOrEmpty(model.CnpjAdministradoraCondominio) || model.CnpjAdministradoraCondominio.Equals("string", StringComparison.InvariantCultureIgnoreCase))
                {
                    model.CnpjAdministradoraCondominio = psBd != null ? psBd.CnpjAdministradoraCondominio : null;
                }

                if (string.IsNullOrEmpty(model.EnderecoAdministradoraCondominio) || model.EnderecoAdministradoraCondominio.Equals("string", StringComparison.InvariantCultureIgnoreCase))
                {
                    model.EnderecoAdministradoraCondominio = psBd != null ? psBd.EnderecoAdministradoraCondominio : null;
                }

                if (string.IsNullOrEmpty(model.ExibirFinanceirosDasEmpresaIds) || model.ExibirFinanceirosDasEmpresaIds.Equals("string", StringComparison.InvariantCultureIgnoreCase))
                {
                    model.ExibirFinanceirosDasEmpresaIds = psBd != null ? psBd.ExibirFinanceirosDasEmpresaIds : null;
                }

                if (!model.PontosRci.HasValue || model.PontosRci == 0)
                {
                    model.PontosRci = psBd != null ? psBd.PontosRci : 5629;
                }

                if (!model.PermiteReservaRciApenasClientesComContratoRci.HasValue)
                {
                    model.PermiteReservaRciApenasClientesComContratoRci = psBd != null ? psBd.PermiteReservaRciApenasClientesComContratoRci : Domain.Enumns.EnumSimNao.Não;
                }

                if (!model.Habilitar2FAPorEmail.HasValue)
                {
                    model.Habilitar2FAPorEmail = psBd != null ? psBd.Habilitar2FAPorEmail : Domain.Enumns.EnumSimNao.Não;
                }
                if (!model.Habilitar2FAPorSms.HasValue)
                {
                    model.Habilitar2FAPorSms = psBd != null ? psBd.Habilitar2FAPorSms : Domain.Enumns.EnumSimNao.Não;
                }
                if (!model.Habilitar2FAParaCliente.HasValue)
                {
                    model.Habilitar2FAParaCliente = psBd != null ? psBd.Habilitar2FAParaCliente : Domain.Enumns.EnumSimNao.Não;
                }
                if (!model.Habilitar2FAParaAdministrador.HasValue)
                {
                    model.Habilitar2FAParaAdministrador = psBd != null ? psBd.Habilitar2FAParaAdministrador : Domain.Enumns.EnumSimNao.Não;
                }

                if (string.IsNullOrEmpty(model.EndpointEnvioSms2FA) || model.EndpointEnvioSms2FA.Equals("string", StringComparison.InvariantCultureIgnoreCase))
                {
                    model.EndpointEnvioSms2FA = psBd != null ? psBd.EndpointEnvioSms2FA : null;
                }

                if (string.IsNullOrEmpty(model.SmtpHost) || model.SmtpHost.Equals("string", StringComparison.InvariantCultureIgnoreCase))
                    model.SmtpHost = psBd != null ? psBd.SmtpHost : null;
                if (!model.SmtpPort.HasValue || model.SmtpPort == 0)
                    model.SmtpPort = psBd != null ? psBd.SmtpPort : null;
                if (!model.SmtpUseSsl.HasValue)
                    model.SmtpUseSsl = psBd != null ? psBd.SmtpUseSsl : Domain.Enumns.EnumSimNao.Não;
                if (string.IsNullOrEmpty(model.SmtpUser) || model.SmtpUser.Equals("string", StringComparison.InvariantCultureIgnoreCase))
                    model.SmtpUser = psBd != null ? psBd.SmtpUser : null;
                if (string.IsNullOrEmpty(model.SmtpPass) || model.SmtpPass.Equals("string", StringComparison.InvariantCultureIgnoreCase))
                    model.SmtpPass = psBd != null ? psBd.SmtpPass : null;
                if (string.IsNullOrEmpty(model.SmtpFromName) || model.SmtpFromName.Equals("string", StringComparison.InvariantCultureIgnoreCase))
                    model.SmtpFromName = psBd != null ? psBd.SmtpFromName : null;
                if (!model.TipoEnvioEmail.HasValue)
                    model.TipoEnvioEmail = psBd != null ? psBd.TipoEnvioEmail : null;
                if (string.IsNullOrEmpty(model.EmailTrackingBaseUrl) || model.EmailTrackingBaseUrl.Equals("string", StringComparison.InvariantCultureIgnoreCase))
                    model.EmailTrackingBaseUrl = psBd != null ? psBd.EmailTrackingBaseUrl : null;

                var outroParametroMesmaEmpresa = (await _repository.FindByHql<ParametroSistema>($"From ParametroSistema ps Inner Join Fetch ps.Empresa emp Where emp.Id = {empFirst.Id}")).FirstOrDefault();
                if (psBd?.Id == 0 && (outroParametroMesmaEmpresa != null && outroParametroMesmaEmpresa.Id > 0))
                    psBd.Id = outroParametroMesmaEmpresa.Id;
                else if (psBd?.Id > 0 && outroParametroMesmaEmpresa != null && outroParametroMesmaEmpresa.Id != psBd?.Id)
                    throw new Exception($"Só pode existir um parâmetro na empresa: {empFirst.Id}");

                var wwwrootconfigpath = _configuration.GetValue<string>("WwwRootImagePath", "C:\\inetpub\\wwwroot\\Imagens");
                var virtualdirectoryname = _configuration.GetValue<string>("NomeDiretorioVirtualImagens", "portalproprietario");

                ParametroSistema? parametroSistema = psBd != null ? _mapper.Map(model, psBd) : _mapper.Map<ParametroSistema>(model);
                parametroSistema.Empresa = new Empresa() { Id = empFirst.Id };

                #region Imagens Empreendimento I
                if (model.Imagem1 != null && model.Imagem1.Any())
                {
                    var pathFull = string.Concat(Path.Combine(wwwrootconfigpath, $"{Guid.NewGuid()}"), Path.GetExtension(model.Imagem1.First().FileName)).Replace(" ", "_");

                    using (var stream = new FileStream(pathFull, FileMode.Create))
                    {
                        await model.Imagem1.First().CopyToAsync(stream);
                    }
                    parametroSistema.ImagemHomeUrl1 = string.Concat("/", virtualdirectoryname, "/", pathFull.Replace($"{wwwrootconfigpath}", string.Empty).Replace("\\", "/").TrimStart('/'));
                }

                if (model.Imagem2 != null && model.Imagem2.Any())
                {
                    var pathFull = string.Concat(Path.Combine(wwwrootconfigpath, $"{Guid.NewGuid()}"), Path.GetExtension(model.Imagem2.First().FileName)).Replace(" ", "_");

                    using (var stream = new FileStream(pathFull, FileMode.Create))
                    {
                        await model.Imagem2.First().CopyToAsync(stream);
                    }
                    parametroSistema.ImagemHomeUrl2 = string.Concat("/", virtualdirectoryname, "/", pathFull.Replace($"{wwwrootconfigpath}", string.Empty).Replace("\\", "/").TrimStart('/'));
                }

                if (model.Imagem3 != null && model.Imagem3.Any())
                {
                    var pathFull = string.Concat(Path.Combine(wwwrootconfigpath, $"{Guid.NewGuid()}"), Path.GetExtension(model.Imagem3.First().FileName)).Replace(" ", "_");

                    using (var stream = new FileStream(pathFull, FileMode.Create))
                    {
                        await model.Imagem3.First().CopyToAsync(stream);
                    }
                    parametroSistema.ImagemHomeUrl3 = string.Concat("/", virtualdirectoryname, "/", pathFull.Replace($"{wwwrootconfigpath}", string.Empty).Replace("\\", "/").TrimStart('/'));
                }

                if (model.Imagem4 != null && model.Imagem4.Any())
                {
                    var pathFull = string.Concat(Path.Combine(wwwrootconfigpath, $"{Guid.NewGuid()}"), Path.GetExtension(model.Imagem4.First().FileName)).Replace(" ", "_");

                    using (var stream = new FileStream(pathFull, FileMode.Create))
                    {
                        await model.Imagem4.First().CopyToAsync(stream);
                    }
                    parametroSistema.ImagemHomeUrl4 = string.Concat("/", virtualdirectoryname, "/", pathFull.Replace($"{wwwrootconfigpath}", string.Empty).Replace("\\", "/").TrimStart('/'));
                }

                if (model.Imagem5 != null && model.Imagem5.Any())
                {
                    var pathFull = string.Concat(Path.Combine(wwwrootconfigpath, $"{Guid.NewGuid()}"), Path.GetExtension(model.Imagem5.First().FileName)).Replace(" ", "_");

                    using (var stream = new FileStream(pathFull, FileMode.Create))
                    {
                        await model.Imagem5.First().CopyToAsync(stream);
                    }
                    parametroSistema.ImagemHomeUrl5 = string.Concat("/", virtualdirectoryname, "/", pathFull.Replace($"{wwwrootconfigpath}", string.Empty).Replace("\\", "/").TrimStart('/'));
                }

                if (model.Imagem6 != null && model.Imagem6.Any())
                {
                    var pathFull = string.Concat(Path.Combine(wwwrootconfigpath, $"{Guid.NewGuid()}"), Path.GetExtension(model.Imagem6.First().FileName)).Replace(" ", "_");

                    using (var stream = new FileStream(pathFull, FileMode.Create))
                    {
                        await model.Imagem6.First().CopyToAsync(stream);
                    }
                    parametroSistema.ImagemHomeUrl6 = string.Concat("/", virtualdirectoryname, "/", pathFull.Replace($"{wwwrootconfigpath}", string.Empty).Replace("\\", "/").TrimStart('/'));
                }

                if (model.Imagem7 != null && model.Imagem7.Any())
                {
                    var pathFull = string.Concat(Path.Combine(wwwrootconfigpath, $"{Guid.NewGuid()}"), Path.GetExtension(model.Imagem7.First().FileName)).Replace(" ", "_");

                    using (var stream = new FileStream(pathFull, FileMode.Create))
                    {
                        await model.Imagem7.First().CopyToAsync(stream);
                    }
                    parametroSistema.ImagemHomeUrl7 = string.Concat("/", virtualdirectoryname, "/", pathFull.Replace($"{wwwrootconfigpath}", string.Empty).Replace("\\", "/").TrimStart('/'));
                }

                if (model.Imagem8 != null && model.Imagem8.Any())
                {
                    var pathFull = string.Concat(Path.Combine(wwwrootconfigpath, $"{Guid.NewGuid()}"), Path.GetExtension(model.Imagem8.First().FileName)).Replace(" ", "_");

                    using (var stream = new FileStream(pathFull, FileMode.Create))
                    {
                        await model.Imagem8.First().CopyToAsync(stream);
                    }
                    parametroSistema.ImagemHomeUrl8 = string.Concat("/", virtualdirectoryname, "/", pathFull.Replace($"{wwwrootconfigpath}", string.Empty).Replace("\\", "/").TrimStart('/'));
                }

                if (model.Imagem9 != null && model.Imagem9.Any())
                {
                    var pathFull = string.Concat(Path.Combine(wwwrootconfigpath, $"{Guid.NewGuid()}"), Path.GetExtension(model.Imagem9.First().FileName)).Replace(" ", "_");

                    using (var stream = new FileStream(pathFull, FileMode.Create))
                    {
                        await model.Imagem9.First().CopyToAsync(stream);
                    }
                    parametroSistema.ImagemHomeUrl9 = string.Concat("/", virtualdirectoryname, "/", pathFull.Replace($"{wwwrootconfigpath}", string.Empty).Replace("\\", "/").TrimStart('/'));
                }

                if (model.Imagem10 != null && model.Imagem10.Any())
                {
                    var pathFull = string.Concat(Path.Combine(wwwrootconfigpath, $"{Guid.NewGuid()}"), Path.GetExtension(model.Imagem10.First().FileName)).Replace(" ", "_");

                    using (var stream = new FileStream(pathFull, FileMode.Create))
                    {
                        await model.Imagem10.First().CopyToAsync(stream);
                    }
                    parametroSistema.ImagemHomeUrl10 = string.Concat("/", virtualdirectoryname, "/", pathFull.Replace($"{wwwrootconfigpath}", string.Empty).Replace("\\", "/").TrimStart('/'));
                }
                #endregion

                #region Imagens Empreendimento II
                if (model.Imagem11 != null && model.Imagem11.Any())
                {
                    var pathFull = string.Concat(Path.Combine(wwwrootconfigpath, $"{Guid.NewGuid()}"), Path.GetExtension(model.Imagem11.First().FileName)).Replace(" ", "_");

                    using (var stream = new FileStream(pathFull, FileMode.Create))
                    {
                        await model.Imagem11.First().CopyToAsync(stream);
                    }
                    parametroSistema.ImagemHomeUrl11 = string.Concat("/", virtualdirectoryname, "/", pathFull.Replace($"{wwwrootconfigpath}", string.Empty).Replace("\\", "/").TrimStart('/'));
                }

                if (model.Imagem12 != null && model.Imagem12.Any())
                {
                    var pathFull = string.Concat(Path.Combine(wwwrootconfigpath, $"{Guid.NewGuid()}"), Path.GetExtension(model.Imagem12.First().FileName)).Replace(" ", "_");

                    using (var stream = new FileStream(pathFull, FileMode.Create))
                    {
                        await model.Imagem12.First().CopyToAsync(stream);
                    }
                    parametroSistema.ImagemHomeUrl12 = string.Concat("/", virtualdirectoryname, "/", pathFull.Replace($"{wwwrootconfigpath}", string.Empty).Replace("\\", "/").TrimStart('/'));
                }

                if (model.Imagem13 != null && model.Imagem13.Any())
                {
                    var pathFull = string.Concat(Path.Combine(wwwrootconfigpath, $"{Guid.NewGuid()}"), Path.GetExtension(model.Imagem13.First().FileName)).Replace(" ", "_");

                    using (var stream = new FileStream(pathFull, FileMode.Create))
                    {
                        await model.Imagem13.First().CopyToAsync(stream);
                    }
                    parametroSistema.ImagemHomeUrl13 = string.Concat("/", virtualdirectoryname, "/", pathFull.Replace($"{wwwrootconfigpath}", string.Empty).Replace("\\", "/").TrimStart('/'));
                }

                if (model.Imagem14 != null && model.Imagem14.Any())
                {
                    var pathFull = string.Concat(Path.Combine(wwwrootconfigpath, $"{Guid.NewGuid()}"), Path.GetExtension(model.Imagem14.First().FileName)).Replace(" ", "_");

                    using (var stream = new FileStream(pathFull, FileMode.Create))
                    {
                        await model.Imagem14.First().CopyToAsync(stream);
                    }
                    parametroSistema.ImagemHomeUrl14 = string.Concat("/", virtualdirectoryname, "/", pathFull.Replace($"{wwwrootconfigpath}", string.Empty).Replace("\\", "/").TrimStart('/'));
                }

                if (model.Imagem15 != null && model.Imagem15.Any())
                {
                    var pathFull = string.Concat(Path.Combine(wwwrootconfigpath, $"{Guid.NewGuid()}"), Path.GetExtension(model.Imagem15.First().FileName)).Replace(" ", "_");

                    using (var stream = new FileStream(pathFull, FileMode.Create))
                    {
                        await model.Imagem15.First().CopyToAsync(stream);
                    }
                    parametroSistema.ImagemHomeUrl15 = string.Concat("/", virtualdirectoryname, "/", pathFull.Replace($"{wwwrootconfigpath}", string.Empty).Replace("\\", "/").TrimStart('/'));
                }

                if (model.Imagem16 != null && model.Imagem16.Any())
                {
                    var pathFull = string.Concat(Path.Combine(wwwrootconfigpath, $"{Guid.NewGuid()}"), Path.GetExtension(model.Imagem16.First().FileName)).Replace(" ", "_");

                    using (var stream = new FileStream(pathFull, FileMode.Create))
                    {
                        await model.Imagem16.First().CopyToAsync(stream);
                    }
                    parametroSistema.ImagemHomeUrl16 = string.Concat("/", virtualdirectoryname, "/", pathFull.Replace($"{wwwrootconfigpath}", string.Empty).Replace("\\", "/").TrimStart('/'));
                }

                if (model.Imagem17 != null && model.Imagem17.Any())
                {
                    var pathFull = string.Concat(Path.Combine(wwwrootconfigpath, $"{Guid.NewGuid()}"), Path.GetExtension(model.Imagem17.First().FileName)).Replace(" ", "_");

                    using (var stream = new FileStream(pathFull, FileMode.Create))
                    {
                        await model.Imagem17.First().CopyToAsync(stream);
                    }
                    parametroSistema.ImagemHomeUrl17 = string.Concat("/", virtualdirectoryname, "/", pathFull.Replace($"{wwwrootconfigpath}", string.Empty).Replace("\\", "/").TrimStart('/'));
                }

                if (model.Imagem18 != null && model.Imagem18.Any())
                {
                    var pathFull = string.Concat(Path.Combine(wwwrootconfigpath, $"{Guid.NewGuid()}"), Path.GetExtension(model.Imagem18.First().FileName)).Replace(" ", "_");

                    using (var stream = new FileStream(pathFull, FileMode.Create))
                    {
                        await model.Imagem18.First().CopyToAsync(stream);
                    }
                    parametroSistema.ImagemHomeUrl18 = string.Concat("/", virtualdirectoryname, "/", pathFull.Replace($"{wwwrootconfigpath}", string.Empty).Replace("\\", "/").TrimStart('/'));
                }

                if (model.Imagem19 != null && model.Imagem19.Any())
                {
                    var pathFull = string.Concat(Path.Combine(wwwrootconfigpath, $"{Guid.NewGuid()}"), Path.GetExtension(model.Imagem19.First().FileName)).Replace(" ", "_");

                    using (var stream = new FileStream(pathFull, FileMode.Create))
                    {
                        await model.Imagem19.First().CopyToAsync(stream);
                    }
                    parametroSistema.ImagemHomeUrl19 = string.Concat("/", virtualdirectoryname, "/", pathFull.Replace($"{wwwrootconfigpath}", string.Empty).Replace("\\", "/").TrimStart('/'));
                }

                if (model.Imagem20 != null && model.Imagem20.Any())
                {
                    var pathFull = string.Concat(Path.Combine(wwwrootconfigpath, $"{Guid.NewGuid()}"), Path.GetExtension(model.Imagem20.First().FileName)).Replace(" ", "_");

                    using (var stream = new FileStream(pathFull, FileMode.Create))
                    {
                        await model.Imagem20.First().CopyToAsync(stream);
                    }
                    parametroSistema.ImagemHomeUrl20 = string.Concat("/", virtualdirectoryname, "/", pathFull.Replace($"{wwwrootconfigpath}", string.Empty).Replace("\\", "/").TrimStart('/'));
                }
                #endregion

                parametroSistema.HabilitarPagamentosOnLine =
                (parametroSistema.HabilitarPagamentoEmPix.GetValueOrDefault(Domain.Enumns.EnumSimNao.Não) == Domain.Enumns.EnumSimNao.Sim ||
                parametroSistema.HabilitarPagamentoEmCartao.GetValueOrDefault(Domain.Enumns.EnumSimNao.Não) == Domain.Enumns.EnumSimNao.Sim) ? Domain.Enumns.EnumSimNao.Sim : Domain.Enumns.EnumSimNao.Não;

                await _repository.Save(parametroSistema);

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"Parâmetro de sistema: ({parametroSistema.Id} - {parametroSistema.Empresa.Id}) salvo com sucesso!");

                    var searchResult = (await GetParameters());
                    return searchResult;

                }
                throw exception ?? new Exception($"Não foi possível salvar o parâmetro");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível salvar o parâmetro");
                _repository.Rollback();
                throw;
            }
        }

        private async Task<string> GetPathToSaveImage(string? configPath, string wwwrootconfigpath, int imageGroupId, int empresaId)
        {
            var wwrootpath = wwwrootconfigpath;
            List<(string key, string path)> listItensPathTranslate = new List<(string key, string path)>()
            {
                new ("[wwwroot]",$"{wwrootpath}"),
                new ("[empresaId]",$"EmpId_{empresaId}"),
                new ("[imageGroupId]",$"ImageGroupId_{imageGroupId}"),

             };


            if (string.IsNullOrEmpty(configPath))
                configPath = "[wwwroot]|[empresaId]|[imageGroupId]";


            var itensToTranslate = configPath.Split('|');

            var pathReturn = string.Empty;
            List<string> paths = new List<string>();

            foreach (var item in itensToTranslate)
            {
                var pathConfigAtual = listItensPathTranslate.FirstOrDefault(b => b.key.Equals($"{item}", StringComparison.CurrentCultureIgnoreCase));
                if (string.IsNullOrEmpty(pathConfigAtual.path))
                    throw new ArgumentException($"Não foi encontrada a configuração de direcionamento de path com a key: '{item}'");

                paths.Add(pathConfigAtual.path);
            }

            pathReturn = Path.Combine(paths.ToArray());

            return await Task.FromResult(pathReturn);

        }

        public async Task<ParametroSistemaViewModel?> GetParameters()
        {
            var httpContext = _httpContextAccessor?.HttpContext?.Request;
            if (httpContext is null)
                throw new Exception("Não foi possível identificar a URL do servidor");

            var complemento = _configuration.GetValue<string>("ComplementoUrlApi", string.Empty);
            var complementoUrlApiComplementoParaHttps = _configuration.GetValue<string>("ComplementoUrlApiComplementoParaHttps", string.Empty);

            var loggedUser = await _repository.GetLoggedUser();
            if (loggedUser == null)
                throw new Exception("Não foi possível identificar o usuário logado");

            var empresas = (await _repository.FindByHql<Empresa>("From Empresa e Inner Join Fetch e.Pessoa p")).AsList();
            if (empresas.Count() > 1 || empresas.Count() == 0)
                throw new ArgumentException($"Não foi possível salvar os parâmetros do sistema empCount = {empresas.Count()}");

            var empFirst = empresas.First();

            ParametroSistemaViewModel? parametroSistema = await _serviceBase.GetParametroSistema();

            if (parametroSistema == null)
            {
                throw new ArgumentException($"Não foi possível encontrar o parâmetro do sistema para a empresa: {empFirst.Id}");
            }

            parametroSistema.HabilitarPagamentosOnLine =
                (parametroSistema.HabilitarPagamentoEmPix.GetValueOrDefault(Domain.Enumns.EnumSimNao.Não) == Domain.Enumns.EnumSimNao.Sim ||
                parametroSistema.HabilitarPagamentoEmCartao.GetValueOrDefault(Domain.Enumns.EnumSimNao.Não) == Domain.Enumns.EnumSimNao.Sim) ? Domain.Enumns.EnumSimNao.Sim : Domain.Enumns.EnumSimNao.Não;

            

            var integradoWith = _configuration.GetValue<string>("IntegradoCom","eSolution");
            if (!string.IsNullOrEmpty(integradoWith))
            {
                parametroSistema.IntegradoComMultiPropriedade = integradoWith.Contains("eSolution", StringComparison.InvariantCultureIgnoreCase) ? Domain.Enumns.EnumSimNao.Sim : Domain.Enumns.EnumSimNao.Não;
                parametroSistema.IntegradoComTimeSharing = !integradoWith.Contains("eSolution", StringComparison.InvariantCultureIgnoreCase) ? Domain.Enumns.EnumSimNao.Sim : Domain.Enumns.EnumSimNao.Não;
            }


            parametroSistema.ServerAddress = $"{httpContext.Scheme}{complementoUrlApiComplementoParaHttps}://{httpContext.Host}{(complemento)}";

            if (!string.IsNullOrEmpty(parametroSistema.ImagemHomeUrl1) && !parametroSistema.ImagemHomeUrl1.StartsWith(parametroSistema.ServerAddress, StringComparison.CurrentCultureIgnoreCase))
            {
                parametroSistema.ImagemHomeUrl1 = $"{parametroSistema.ServerAddress}{parametroSistema.ImagemHomeUrl1}";
            }

            if (!string.IsNullOrEmpty(parametroSistema.ImagemHomeUrl2) && !parametroSistema.ImagemHomeUrl2.StartsWith(parametroSistema.ServerAddress, StringComparison.CurrentCultureIgnoreCase))
            {
                parametroSistema.ImagemHomeUrl2 = $"{parametroSistema.ServerAddress}{parametroSistema.ImagemHomeUrl2}";
            }

            if (!string.IsNullOrEmpty(parametroSistema.ImagemHomeUrl3) && !parametroSistema.ImagemHomeUrl3.StartsWith(parametroSistema.ServerAddress, StringComparison.CurrentCultureIgnoreCase))
            {
                parametroSistema.ImagemHomeUrl3 = $"{parametroSistema.ServerAddress}{parametroSistema.ImagemHomeUrl3}";
            }

            if (!string.IsNullOrEmpty(parametroSistema.ImagemHomeUrl4) && !parametroSistema.ImagemHomeUrl4.StartsWith(parametroSistema.ServerAddress, StringComparison.CurrentCultureIgnoreCase))
            {
                parametroSistema.ImagemHomeUrl4 = $"{parametroSistema.ServerAddress}{parametroSistema.ImagemHomeUrl4}";
            }

            if (!string.IsNullOrEmpty(parametroSistema.ImagemHomeUrl5) && !parametroSistema.ImagemHomeUrl5.StartsWith(parametroSistema.ServerAddress, StringComparison.CurrentCultureIgnoreCase))
            {
                parametroSistema.ImagemHomeUrl5 = $"{parametroSistema.ServerAddress}{parametroSistema.ImagemHomeUrl5}";
            }

            if (!string.IsNullOrEmpty(parametroSistema.ImagemHomeUrl6) && !parametroSistema.ImagemHomeUrl6.StartsWith(parametroSistema.ServerAddress, StringComparison.CurrentCultureIgnoreCase))
            {
                parametroSistema.ImagemHomeUrl6 = $"{parametroSistema.ServerAddress}{parametroSistema.ImagemHomeUrl6}";
            }

            if (!string.IsNullOrEmpty(parametroSistema.ImagemHomeUrl7) && !parametroSistema.ImagemHomeUrl7.StartsWith(parametroSistema.ServerAddress, StringComparison.CurrentCultureIgnoreCase))
            {
                parametroSistema.ImagemHomeUrl7 = $"{parametroSistema.ServerAddress}{parametroSistema.ImagemHomeUrl7}";
            }

            if (!string.IsNullOrEmpty(parametroSistema.ImagemHomeUrl8) && !parametroSistema.ImagemHomeUrl8.StartsWith(parametroSistema.ServerAddress, StringComparison.CurrentCultureIgnoreCase))
            {
                parametroSistema.ImagemHomeUrl8 = $"{parametroSistema.ServerAddress}{parametroSistema.ImagemHomeUrl8}";
            }

            if (!string.IsNullOrEmpty(parametroSistema.ImagemHomeUrl9) && !parametroSistema.ImagemHomeUrl9.StartsWith(parametroSistema.ServerAddress, StringComparison.CurrentCultureIgnoreCase))
            {
                parametroSistema.ImagemHomeUrl9 = $"{parametroSistema.ServerAddress}{parametroSistema.ImagemHomeUrl9}";
            }

            if (!string.IsNullOrEmpty(parametroSistema.ImagemHomeUrl10) && !parametroSistema.ImagemHomeUrl10.StartsWith(parametroSistema.ServerAddress, StringComparison.CurrentCultureIgnoreCase))
            {
                parametroSistema.ImagemHomeUrl10 = $"{parametroSistema.ServerAddress}{parametroSistema.ImagemHomeUrl10}";
            }

            if (!string.IsNullOrEmpty(parametroSistema.ImagemHomeUrl11) && !parametroSistema.ImagemHomeUrl11.StartsWith(parametroSistema.ServerAddress, StringComparison.CurrentCultureIgnoreCase))
            {
                parametroSistema.ImagemHomeUrl11 = $"{parametroSistema.ServerAddress}{parametroSistema.ImagemHomeUrl11}";
            }

            if (!string.IsNullOrEmpty(parametroSistema.ImagemHomeUrl12) && !parametroSistema.ImagemHomeUrl12.StartsWith(parametroSistema.ServerAddress, StringComparison.CurrentCultureIgnoreCase))
            {
                parametroSistema.ImagemHomeUrl12 = $"{parametroSistema.ServerAddress}{parametroSistema.ImagemHomeUrl12}";
            }

            if (!string.IsNullOrEmpty(parametroSistema.ImagemHomeUrl13) && !parametroSistema.ImagemHomeUrl13.StartsWith(parametroSistema.ServerAddress, StringComparison.CurrentCultureIgnoreCase))
            {
                parametroSistema.ImagemHomeUrl13 = $"{parametroSistema.ServerAddress}{parametroSistema.ImagemHomeUrl13}";
            }

            if (!string.IsNullOrEmpty(parametroSistema.ImagemHomeUrl14) && !parametroSistema.ImagemHomeUrl14.StartsWith(parametroSistema.ServerAddress, StringComparison.CurrentCultureIgnoreCase))
            {
                parametroSistema.ImagemHomeUrl14 = $"{parametroSistema.ServerAddress}{parametroSistema.ImagemHomeUrl14}";
            }

            if (!string.IsNullOrEmpty(parametroSistema.ImagemHomeUrl15) && !parametroSistema.ImagemHomeUrl15.StartsWith(parametroSistema.ServerAddress, StringComparison.CurrentCultureIgnoreCase))
            {
                parametroSistema.ImagemHomeUrl15 = $"{parametroSistema.ServerAddress}{parametroSistema.ImagemHomeUrl15}";
            }

            if (!string.IsNullOrEmpty(parametroSistema.ImagemHomeUrl16) && !parametroSistema.ImagemHomeUrl16.StartsWith(parametroSistema.ServerAddress, StringComparison.CurrentCultureIgnoreCase))
            {
                parametroSistema.ImagemHomeUrl16 = $"{parametroSistema.ServerAddress}{parametroSistema.ImagemHomeUrl16}";
            }

            if (!string.IsNullOrEmpty(parametroSistema.ImagemHomeUrl17) && !parametroSistema.ImagemHomeUrl17.StartsWith(parametroSistema.ServerAddress, StringComparison.CurrentCultureIgnoreCase))
            {
                parametroSistema.ImagemHomeUrl17 = $"{parametroSistema.ServerAddress}{parametroSistema.ImagemHomeUrl17}";
            }

            if (!string.IsNullOrEmpty(parametroSistema.ImagemHomeUrl18) && !parametroSistema.ImagemHomeUrl18.StartsWith(parametroSistema.ServerAddress, StringComparison.CurrentCultureIgnoreCase))
            {
                parametroSistema.ImagemHomeUrl18 = $"{parametroSistema.ServerAddress}{parametroSistema.ImagemHomeUrl18}";
            }

            if (!string.IsNullOrEmpty(parametroSistema.ImagemHomeUrl19) && !parametroSistema.ImagemHomeUrl19.StartsWith(parametroSistema.ServerAddress, StringComparison.CurrentCultureIgnoreCase))
            {
                parametroSistema.ImagemHomeUrl19 = $"{parametroSistema.ServerAddress}{parametroSistema.ImagemHomeUrl19}";
            }

            if (!string.IsNullOrEmpty(parametroSistema.ImagemHomeUrl20) && !parametroSistema.ImagemHomeUrl20.StartsWith(parametroSistema.ServerAddress, StringComparison.CurrentCultureIgnoreCase))
            {
                parametroSistema.ImagemHomeUrl20 = $"{parametroSistema.ServerAddress}{parametroSistema.ImagemHomeUrl20}";
            }


            return parametroSistema;
        }

        /// <inheritdoc />
        public async Task UpdateTipoEnvioEmailOnlyAsync(Domain.Enumns.EnumTipoEnvioEmail tipoEnvioEmail, CancellationToken cancellationToken = default)
        {
            try
            {
                _repository.BeginTransaction();
                var empresas = (await _repository.FindByHql<Empresa>("From Empresa e Inner Join Fetch e.Pessoa p")).AsList();
                if (empresas.Count == 0 || empresas.Count > 1)
                {
                    _repository.Rollback();
                    _logger.LogWarning("UpdateTipoEnvioEmailOnly: não foi possível identificar a empresa (count={Count}).", empresas.Count);
                    return;
                }
                var empFirst = empresas.First();
                var psList = await _repository.FindByHql<ParametroSistema>($"From ParametroSistema ps Inner Join Fetch ps.Empresa emp Where emp.Id = {empFirst.Id}");
                var psBd = psList.FirstOrDefault();
                if (psBd == null)
                {
                    _repository.Rollback();
                    _logger.LogWarning("UpdateTipoEnvioEmailOnly: ParametroSistema não encontrado para a empresa Id={EmpresaId}.", empFirst.Id);
                    return;
                }
                psBd.TipoEnvioEmail = tipoEnvioEmail;
                await _repository.Save(psBd);
                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                    _logger.LogInformation("Tipo de envio de e-mail atualizado para {TipoEnvioEmail} (Id={ParametroId}).", tipoEnvioEmail, psBd.Id);
                else
                {
                    _repository.Rollback();
                    _logger.LogError(exception, "UpdateTipoEnvioEmailOnly: falha ao persistir.");
                }
            }
            catch (Exception ex)
            {
                _repository.Rollback();
                _logger.LogError(ex, "UpdateTipoEnvioEmailOnly: exceção ao atualizar TipoEnvioEmail.");
            }
        }
    }
}
