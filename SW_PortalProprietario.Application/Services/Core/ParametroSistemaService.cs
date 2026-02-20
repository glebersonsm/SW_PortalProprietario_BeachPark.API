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
                    throw new ArgumentException($"NÃ£o foi possÃ­vel salvar os parÃ¢metros do sistema empCount = {empresas.Count()}");

                var empFirst = empresas.First();

                var httpContext = _httpContextAccessor?.HttpContext?.Request;
                if (httpContext is null)
                    throw new Exception("NÃ£o foi possÃ­vel identificar a URL do servidor");

                var loggedUser = await _repository.GetLoggedUser();
                if (loggedUser == null)
                    throw new Exception("NÃ£o foi possÃ­vel identificar o usuÃ¡rio logado");

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
                    model.PermitirUsuarioAlterarSeuDoc = psBd != null ? psBd.PermitirUsuarioAlterarSeuDoc : Domain.Enumns.EnumSimNao.Nao;
                }

                if (!model.PermitirUsuarioAlterarSeuEmail.HasValue)
                {
                    model.PermitirUsuarioAlterarSeuEmail = psBd != null ? psBd.PermitirUsuarioAlterarSeuEmail : Domain.Enumns.EnumSimNao.Nao;
                }

                if (model.AgruparCertidaoPorCliente.GetValueOrDefault(Domain.Enumns.EnumSimNao.Nao) == Domain.Enumns.EnumSimNao.Nao && model.EmitirCertidaoPorUnidCliente.GetValueOrDefault(Domain.Enumns.EnumSimNao.Nao) == Domain.Enumns.EnumSimNao.Nao)
                    model.AgruparCertidaoPorCliente = Domain.Enumns.EnumSimNao.Sim;

                if (model.IntegradoComMultiPropriedade.GetValueOrDefault(Domain.Enumns.EnumSimNao.Nao) == Domain.Enumns.EnumSimNao.Nao && model.IntegradoComTimeSharing.GetValueOrDefault(Domain.Enumns.EnumSimNao.Nao) == Domain.Enumns.EnumSimNao.Nao)
                {
                    if (psBd != null && (psBd.IntegradoComTimeSharing.GetValueOrDefault(Domain.Enumns.EnumSimNao.Nao) != Domain.Enumns.EnumSimNao.Nao || psBd.IntegradoComMultiPropriedade.GetValueOrDefault(Domain.Enumns.EnumSimNao.Nao) != Domain.Enumns.EnumSimNao.Nao))
                    {
                        model.IntegradoComMultiPropriedade = psBd.IntegradoComMultiPropriedade;
                        model.IntegradoComTimeSharing = psBd.IntegradoComTimeSharing;
                    }
                    else
                    {
                        model.IntegradoComMultiPropriedade = Domain.Enumns.EnumSimNao.Sim;
                        model.IntegradoComTimeSharing = Domain.Enumns.EnumSimNao.Nao;
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
                    model.PermiteReservaRciApenasClientesComContratoRci = psBd != null ? psBd.PermiteReservaRciApenasClientesComContratoRci : Domain.Enumns.EnumSimNao.Nao;
                }

                if (!model.Habilitar2FAPorEmail.HasValue)
                {
                    model.Habilitar2FAPorEmail = psBd != null ? psBd.Habilitar2FAPorEmail : Domain.Enumns.EnumSimNao.Nao;
                }
                if (!model.Habilitar2FAPorSms.HasValue)
                {
                    model.Habilitar2FAPorSms = psBd != null ? psBd.Habilitar2FAPorSms : Domain.Enumns.EnumSimNao.Nao;
                }
                if (!model.Habilitar2FAParaCliente.HasValue)
                {
                    model.Habilitar2FAParaCliente = psBd != null ? psBd.Habilitar2FAParaCliente : Domain.Enumns.EnumSimNao.Nao;
                }
                if (!model.Habilitar2FAParaAdministrador.HasValue)
                {
                    model.Habilitar2FAParaAdministrador = psBd != null ? psBd.Habilitar2FAParaAdministrador : Domain.Enumns.EnumSimNao.Nao;
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
                    model.SmtpUseSsl = psBd != null ? psBd.SmtpUseSsl : Domain.Enumns.EnumSimNao.Nao;
                if (string.IsNullOrEmpty(model.SmtpIamUser) || model.SmtpIamUser.Equals("string", StringComparison.InvariantCultureIgnoreCase))
                    model.SmtpIamUser = psBd != null ? psBd.SmtpIamUser : null;
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

                if (!model.CriarUsuariosLegado.HasValue)
                    model.CriarUsuariosLegado = psBd != null ? psBd.CriarUsuariosLegado : Domain.Enumns.EnumSimNao.Nao;
                if (!model.CriarUsuariosClientesLegado.HasValue)
                    model.CriarUsuariosClientesLegado = psBd != null ? psBd.CriarUsuariosClientesLegado : Domain.Enumns.EnumSimNao.Nao;

                var outroParametroMesmaEmpresa = (await _repository.FindByHql<ParametroSistema>($"From ParametroSistema ps Inner Join Fetch ps.Empresa emp Where emp.Id = {empFirst.Id}")).FirstOrDefault();
                if (psBd?.Id == 0 && (outroParametroMesmaEmpresa != null && outroParametroMesmaEmpresa.Id > 0))
                    psBd.Id = outroParametroMesmaEmpresa.Id;
                else if (psBd?.Id > 0 && outroParametroMesmaEmpresa != null && outroParametroMesmaEmpresa.Id != psBd?.Id)
                    throw new Exception($"SÃ³ pode existir um parÃ¢metro na empresa: {empFirst.Id}");

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
                (parametroSistema.HabilitarPagamentoEmPix.GetValueOrDefault(Domain.Enumns.EnumSimNao.Nao) == Domain.Enumns.EnumSimNao.Sim ||
                parametroSistema.HabilitarPagamentoEmCartao.GetValueOrDefault(Domain.Enumns.EnumSimNao.Nao) == Domain.Enumns.EnumSimNao.Sim) ? Domain.Enumns.EnumSimNao.Sim : Domain.Enumns.EnumSimNao.Nao;

                await _repository.Save(parametroSistema);

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"ParÃ¢metro de sistema: ({parametroSistema.Id} - {parametroSistema.Empresa.Id}) salvo com sucesso!");

                    var searchResult = (await GetParameters());
                    return searchResult;

                }
                throw exception ?? new Exception($"NÃ£o foi possÃ­vel salvar o parÃ¢metro");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"NÃ£o foi possÃ­vel salvar o parÃ¢metro");
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
                    throw new ArgumentException($"NÃ£o foi encontrada a configuraÃ§Ã£o de direcionamento de path com a key: '{item}'");

                paths.Add(pathConfigAtual.path);
            }

            pathReturn = Path.Combine(paths.ToArray());

            return await Task.FromResult(pathReturn);

        }

        public async Task<ParametroSistemaViewModel?> GetParameters()
        {
            var httpContext = _httpContextAccessor?.HttpContext?.Request;
            if (httpContext is null)
                throw new Exception("NÃ£o foi possÃ­vel identificar a URL do servidor");

            var complemento = _configuration.GetValue<string>("ComplementoUrlApi", string.Empty);
            var complementoUrlApiComplementoParaHttps = _configuration.GetValue<string>("ComplementoUrlApiComplementoParaHttps", string.Empty);

            var loggedUser = await _repository.GetLoggedUser();
            if (loggedUser == null)
                throw new Exception("NÃ£o foi possÃ­vel identificar o usuÃ¡rio logado");

            var empresas = (await _repository.FindByHql<Empresa>("From Empresa e Inner Join Fetch e.Pessoa p")).AsList();
            if (empresas.Count() > 1 || empresas.Count() == 0)
                throw new ArgumentException($"NÃ£o foi possÃ­vel salvar os parÃ¢metros do sistema empCount = {empresas.Count()}");

            var empFirst = empresas.First();

            ParametroSistemaViewModel? parametroSistema = await _serviceBase.GetParametroSistema();

            if (parametroSistema == null)
            {
                throw new ArgumentException($"NÃ£o foi possÃ­vel encontrar o parÃ¢metro do sistema para a empresa: {empFirst.Id}");
            }

            parametroSistema.HabilitarPagamentosOnLine =
                (parametroSistema.HabilitarPagamentoEmPix.GetValueOrDefault(Domain.Enumns.EnumSimNao.Nao) == Domain.Enumns.EnumSimNao.Sim ||
                parametroSistema.HabilitarPagamentoEmCartao.GetValueOrDefault(Domain.Enumns.EnumSimNao.Nao) == Domain.Enumns.EnumSimNao.Sim) ? Domain.Enumns.EnumSimNao.Sim : Domain.Enumns.EnumSimNao.Nao;

            

            var integradoWith = _configuration.GetValue<string>("IntegradoCom","eSolution");
            if (!string.IsNullOrEmpty(integradoWith))
            {
                parametroSistema.IntegradoComMultiPropriedade = integradoWith.Contains("eSolution", StringComparison.InvariantCultureIgnoreCase) ? Domain.Enumns.EnumSimNao.Sim : Domain.Enumns.EnumSimNao.Nao;
                parametroSistema.IntegradoComTimeSharing = !integradoWith.Contains("eSolution", StringComparison.InvariantCultureIgnoreCase) ? Domain.Enumns.EnumSimNao.Sim : Domain.Enumns.EnumSimNao.Nao;
            }


            parametroSistema.ServerAddress = $"{httpContext.Scheme}{complementoUrlApiComplementoParaHttps}://{httpContext.Host}{(complemento)}";


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
                    _logger.LogWarning("UpdateTipoEnvioEmailOnly: nÃ£o foi possÃ­vel identificar a empresa (count={Count}).", empresas.Count);
                    return;
                }
                var empFirst = empresas.First();
                var psList = await _repository.FindByHql<ParametroSistema>($"From ParametroSistema ps Inner Join Fetch ps.Empresa emp Where emp.Id = {empFirst.Id}");
                var psBd = psList.FirstOrDefault();
                if (psBd == null)
                {
                    _repository.Rollback();
                    _logger.LogWarning("UpdateTipoEnvioEmailOnly: ParametroSistema nÃ£o encontrado para a empresa Id={EmpresaId}.", empFirst.Id);
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
                _logger.LogError(ex, "UpdateTipoEnvioEmailOnly: exceÃ§Ã£o ao atualizar TipoEnvioEmail.");
            }
        }
    }
}
