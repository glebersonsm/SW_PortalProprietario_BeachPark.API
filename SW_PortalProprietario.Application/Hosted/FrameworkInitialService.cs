using AccessCenterDomain.AccessCenter;
using CMDomain.Models.AuthModels;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NHibernate;
using SW_PortalProprietario.Application.Functions;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.AuthModels;
using SW_PortalProprietario.Application.Models.FrameworkModels;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Models.PessoaModels;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;
using SW_PortalProprietario.Domain.Entities.Core.Framework;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;
using SW_PortalProprietario.Domain.Enumns;
using SW_Utils.Functions;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Xml;
using ZXing;
using Pessoa = SW_PortalProprietario.Domain.Entities.Core.DadosPessoa.Pessoa;

namespace SW_PortalProprietario.Application.Hosted
{
    public class FrameworkInitialService : IFrameworkInitialService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<FrameworkInitialService> _logger;
        private readonly IConfiguration _configuration;
        private const string _PATH = "C:\\SW_Solucoes\\Projetos\\SW_PortalProprietario.API\\SW_PortalProprietario.Application";
        public FrameworkInitialService(IServiceScopeFactory serviceScopeFactory,
            ILogger<FrameworkInitialService> logger,
            IConfiguration configuration)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task UpdateFramework()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var _repository = scope.ServiceProvider.GetRequiredService<IRepositoryHosted>();
                var _communicationProvider = scope.ServiceProvider.GetRequiredService<IHybrid_CM_Esolution_Communication>();
                var _mapper = scope.ServiceProvider.GetRequiredService<IProjectObjectMapper>();
                using (var session = _repository.CreateSession())
                {

                    //if (Debugger.IsAttached && DateTime.Today.Date == new DateTime(2025, 8, 14).Date)
                    //{
                    //    await ImportarFaqs(session);
                    //}

                    //if (Debugger.IsAttached && DateTime.Today.Date == new DateTime(2025, 11, 27).Date)
                    //{
                    //    await ImportarDocumentos(session);
                    //    Environment.Exit(0);
                    //}


                    //await UpdatePermissions(_repository, session);
                    //await UpdateAreasSistema(_repository, session);
                    //await UpdateGrupoModulos(_repository, session);
                    //await UpdateModules(_repository, session);

                    if (_configuration.GetValue<bool>("InativarUsuariosSemCota"))
                    {
                        //await InativarUsuariosSemCota();
                        await InativarClienteLegadoSemContratosAtivos(_repository, _communicationProvider, session);
                    }

                    if (_configuration.GetValue<bool>("EfetuarConfiguracoesIniciais"))
                    {
                        await ConfigurarDadosPadroes(_repository, session);
                        if (_configuration.GetValue<bool>("ConfigurarEmpresa"))
                        {
                            await ConfigurarEmpresa(_repository, _communicationProvider, session);
                            await GravarParametroSistema(_repository, session);
                        }
                        if (_configuration.GetValue<bool>("VincularUsuarioEmpresa"))
                            await VincularUsuarioEmpresa(_repository, session);

                    }

                    if (_configuration.GetValue<bool>("CriarPrimeiroUsuario"))
                    {
                        await CriarUsuarioDefault(_repository, _communicationProvider, _mapper, session);
                    }

                    var tipoTelefone = (await _repository.FindBySql<Domain.Entities.Core.DadosPessoa.TipoTelefone>($"Select tt.* From TipoTelefone tt Where Lower(tt.Nome) in ('celular')")).FirstOrDefault();


                    // Buscar par?metros do sistema para verificar configura??es de importa??o legado
                    var parametroSistema = await GetParametroSistema(_repository, session);
                    if (parametroSistema != null)
                    {
                        // Usar configura??o do ParametroSistema, com fallback para configura??o antiga
                        var criarUsuariosLegado = parametroSistema.CriarUsuariosLegado == EnumSimNao.Sim ||
                            (parametroSistema.CriarUsuariosLegado == null && _configuration.GetValue<bool>("CriarUsuariosLegado", false));
                        
                        if (criarUsuariosLegado)
                        {
                            await CriarUsuariosLegado(_repository, _communicationProvider, _mapper, tipoTelefone!, session);
                        }

                        var criarUsuariosClientesLegado = parametroSistema.CriarUsuariosClientesLegado == EnumSimNao.Sim ||
                            (parametroSistema.CriarUsuariosClientesLegado == null && _configuration.GetValue<bool>("CriarUsuariosClientesLegado", false));
                        
                        if (criarUsuariosClientesLegado)
                        {
                            await CriarUsuariosClientesLegado(_repository, _communicationProvider, _mapper, tipoTelefone!, session);
                        }
                    }
                    else
                    {
                        // Fallback para configura??o antiga se ParametroSistema Não existir
                        if (_configuration.GetValue<bool>("CriarUsuariosLegado", false))
                        {
                            await CriarUsuariosLegado(_repository, _communicationProvider, _mapper, tipoTelefone!, session);
                        }

                        if (_configuration.GetValue<bool>("CriarUsuariosClientesLegado", false))
                        {
                            await CriarUsuariosClientesLegado(_repository, _communicationProvider, _mapper, tipoTelefone!, session);
                        }
                    }

                }
            }

        }

        private async Task ImportarFaqs(IRepositoryHosted _repository, IStatelessSession? session)
        {
            // Define o caminho do arquivo
            string caminhoArquivo = "C:\\Users\\glebe\\OneDrive\\Area de Trabalho\\Faq.csv";

            // Verifica se o arquivo existe
            if (!File.Exists(caminhoArquivo))
            {
                Console.WriteLine($"O arquivo '{caminhoArquivo}' Não foi encontrado.");
                return;
            }

            try
            {
                _repository?.BeginTransaction(session);
                string conteudo = File.ReadAllText(caminhoArquivo, Encoding.UTF8);
                List<string> linhas = conteudo.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();
                foreach (string line in linhas)
                {
                    if (line.Contains("GrupoFaqId", StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue; // Pula a linha do cabe?alho
                    }
                    string[] partes = line.Split("[]", StringSplitOptions.RemoveEmptyEntries);
                    if (partes.Length < 2)
                    {
                        Console.WriteLine($"Linha inválida: {line}");
                        continue; // Pula linhas inv?lidas
                    }
                    string pergunta = partes[0].Trim();
                    string resposta = partes[1].Trim();
                    int grupoFaqId = partes.Length > 2 ? int.Parse(partes[2].Trim()) : 0;
                    var faq = new Faq()
                    {
                        Pergunta = pergunta,
                        Resposta = resposta,
                        DataHoraCriacao = DateTime.Now,
                        UsuarioCriacao = 1,
                        Disponivel = EnumSimNao.Sim,
                        GrupoFaq = new GrupoFaq() { Id = 3 }
                    };
                    await _repository.ForcedSave(faq, session);
                }

                var resultCommit = await _repository.CommitAsync(session);
                if (!resultCommit.executed)
                    throw resultCommit.exception ?? new Exception("Erro ao importar o arquivo");

            }
            catch (Exception ex)
            {
                _repository.Rollback(session);
            }
        }

        private async Task ImportarDocumentos(IRepositoryHosted _repository, IStatelessSession? session)
        {
            try
            {
                _repository?.BeginTransaction(session);

                var documentos = (await _repository.FindByHql<Documento>("From Documento ds", session)).AsList();
                if (documentos == null || !documentos.Any())
                {
                    _logger.LogInformation("No documents found to import.");
                    var emptyCommit = await _repository.CommitAsync(session);
                    if (!emptyCommit.executed)
                        throw emptyCommit.exception ?? new Exception("Erro ao processar documentos: commit vazio falhou.");
                    return;
                }

                foreach (var documento in documentos.Where(c=> !string.IsNullOrEmpty(c.Path)))
                {
                    try
                    {
                        if (!File.Exists(documento.Path))
                        {
                            _logger.LogWarning("File not found for Documento Id {DocumentoId}. Expected path: {Path}", documento.Id, documento.Path);
                            continue;
                        }

                        byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(documento.Path);

                        documento.Arquivo = fileBytes;

                        await _repository.ForcedSave(documento, session);
                    }
                    catch (Exception exDoc)
                    {
                        _logger.LogError(exDoc, "Error processing documento Id {DocumentoId}: {Message}", documento?.Id, exDoc.Message);
                    }
                }

                var resultCommit = await _repository.CommitAsync(session);
                if (!resultCommit.executed)
                    throw resultCommit.exception ?? new Exception("Erro ao importar documentos");
            }
            catch (Exception ex)
            {
                try
                {
                    _repository.Rollback(session);
                }
                catch { /* ignore rollback errors */ }

                _logger.LogError(ex, "Erro ao importar documentos: {Message}", ex.Message);
            }
        }

        private async Task InativarUsuariosSemCota(ICommunicationProvider _communicationProvider)
        {
            await _communicationProvider.DesativarUsuariosSemCotaOuContrato();
        }

        private async Task CriarUsuarioDefault(IRepositoryHosted _repository, ICommunicationProvider _communicationProvider, IProjectObjectMapper _mapper, IStatelessSession? session)
        {

            try
            {
                if (_repository == null) return;


                var sb = new StringBuilder(@"select
                    u.Login,
                    p.Nome AS FullName,
                    p.Id AS PessoaId,
                    Coalesce(p.EmailPreferencial,p.EmailAlternativo) as Email,
                    Coalesce(pd.NumeroFormatado,pd.Numero) as CpfCnpj,
                    u.Administrador as Administrator
                    From 
                    usuario u
                    Inner Join Pessoa p on u.Pessoa = p.Id
                    Left Join PessoaDocumento pd on pd.Pessoa = p.Id
                    Left Join TipoDocumentoPessoa tdp on pd.TipoDocumento = tdp.Id
                    Where 1 = 1 
                    and u.DataHoraRemocao is null and Coalesce(u.Removido,0) = 0 ");


                var usuariosSistema =
                    (await _repository.FindBySql<UserRegisterInputModel>(sb.ToString(), session)).AsList();

                var tipoTelefone = (await _repository.FindBySql<Domain.Entities.Core.DadosPessoa.TipoTelefone>($"Select tt.* From TipoTelefone tt Where Lower(tt.Nome) in ('celular')", session)).FirstOrDefault();
                if (tipoTelefone == null) throw new Exception("Tipo de telefone 'Celular' Não encontrado no banco de dados.");


                if (!usuariosSistema.Any(b => !string.IsNullOrEmpty(b.Login) && b.Login.Contains("glebersonsm", StringComparison.InvariantCultureIgnoreCase)))
                {
                    try
                    {
                        _repository.BeginTransaction(session);

                        var item = new UserRegisterInputModel();
                        item.Password = "Gle252626$";
                        item.Login = "Glebersonsm";
                        item.FullName = "Gleberson Sim?o de Moura";
                        item.CpfCnpj = "76473430172";
                        item.Telefone = "64992149095";
                        item.Email = "glebersonsm@gmail.com";
                        item.PasswordConfirmation = item.Password;
                        item.Administrator = EnumSimNao.Sim;
                        await RegistrarUsuarioExecute(_repository, _communicationProvider, _mapper, item, tipoTelefone, session);
                        usuariosSistema.Add(item);

                        var resultCommit = await _repository.CommitAsync(session);
                        if (resultCommit.exception != null)
                            throw resultCommit.exception;
                    }
                    catch (Exception err)
                    {
                        _repository?.Rollback(session);
                        _logger.LogError(err, err.Message);
                    }
                }

            }
            catch (Exception err)
            {
                _logger.LogError(err, err.Message);
            }
        }

        private async Task CriarUsuariosLegado(IRepositoryHosted _repository, ICommunicationProvider _communicationProvider, IProjectObjectMapper _mapper, Domain.Entities.Core.DadosPessoa.TipoTelefone tipoTelefone, IStatelessSession? session)
        {

            try
            {
                if (_repository == null) return;

                var usuariosLegado = new List<UserRegisterInputModel>();

                if (_communicationProvider is IHybrid_CM_Esolution_Communication hybridProvider)
                {
                    _logger.LogInformation("Iniciando importa??o h?brida de usu?rios ativos (CM + Esolution)");

                    var usuariosCm = await hybridProvider.GetUsuariosAtivosSistemaLegado_Cm();
                    if (usuariosCm != null && usuariosCm.Any())
                    {
                        foreach (var usuario in usuariosCm)
                            usuario.ProviderName = "CM";
                        usuariosLegado.AddRange(usuariosCm);
                        _logger.LogInformation($"Importados {usuariosCm.Count} usu?rios ativos do sistema CM");
                    }

                    var usuariosEsol = await hybridProvider.GetUsuariosAtivosSistemaLegado_Esol();
                    if (usuariosEsol != null && usuariosEsol.Any())
                    {
                        foreach (var usuario in usuariosEsol)
                            usuario.ProviderName = "ESOLUTION";
                        usuariosLegado.AddRange(usuariosEsol);
                        _logger.LogInformation($"Importados {usuariosEsol.Count} usu?rios ativos do sistema Esolution");
                    }

                    _logger.LogInformation($"Total de usu?rios ativos para importa??o h?brida: {usuariosLegado.Count}");
                }
                else
                {
                    usuariosLegado = await _communicationProvider.GetUsuariosAtivosSistemaLegado();
                }

                if (usuariosLegado == null || !usuariosLegado.Any()) return;

                 var sb = new StringBuilder(@"select
                    u.Login,
                    p.Nome AS FullName,
                    p.Id AS PessoaId,
                    Coalesce(p.EmailPreferencial,p.EmailAlternativo) as Email,
                    Coalesce(pd.NumeroFormatado,pd.Numero) as CpfCnpj,
                    u.Administrador as Administrator
                    From 
                    usuario u
                    Inner Join Pessoa p on u.Pessoa = p.Id
                    Left Join PessoaDocumento pd on pd.Pessoa = p.Id
                    Left Join TipoDocumentoPessoa tdp on pd.TipoDocumento = tdp.Id
                    Where 1 = 1 
                    and u.DataHoraRemocao is null and Coalesce(u.Removido,0) = 0 ");


                var usuariosSistema =
                    (await _repository.FindBySql<UserRegisterInputModel>(sb.ToString(), session)).AsList();


                foreach (var item in usuariosLegado)
                {
                    try
                    {
                        _repository?.BeginTransaction(session);
                        if (!string.IsNullOrEmpty(item.CpfCnpj) && !SW_Utils.Functions.Helper.IsNumeric(item.CpfCnpj) && 
                            (string.IsNullOrEmpty(item.TipoDocumentoClienteNome) || item.TipoDocumentoClienteNome.ToLower().Contains("passaport",StringComparison.InvariantCultureIgnoreCase)))
                            item.CpfCnpj = "";

                        if (!string.IsNullOrEmpty(item.CpfCnpj) && 
                            (string.IsNullOrEmpty(item.TipoDocumentoClienteNome) || !item.TipoDocumentoClienteNome.ToLower().Contains("passaport", StringComparison.InvariantCultureIgnoreCase)))
                        {
                            if (!SW_Utils.Functions.Helper.IsCpf(item.CpfCnpj) && !SW_Utils.Functions.Helper.IsCnpj(item.CpfCnpj))
                            {
                                _logger.LogWarning($"Não foi poss?vel importar o CPF/CNPJ do usu?rio com login: {item.Login} - Pois o n?mero informado Não ? v?lido: '{item.CpfCnpj}'");
                                item.CpfCnpj = null;
                            }
                        }

                        var usuJaExistente =
                                        usuariosSistema.FirstOrDefault(a => (!string.IsNullOrEmpty(a.CpfCnpj) && !string.IsNullOrEmpty(item.CpfCnpj) &&
                                        Convert.ToInt64(Helper.ApenasNumeros(a.CpfCnpj)) ==
                                    Convert.ToInt64(Helper.ApenasNumeros(item.CpfCnpj))) ||
                                    (!string.IsNullOrEmpty(a.Login) &&
                                    a.Login.Trim(' ').Equals(item.Login?.Trim(' '), StringComparison.CurrentCultureIgnoreCase)));

                        if (usuJaExistente != null)
                        {
                            if (!usuariosSistema.Any(b => !string.IsNullOrEmpty(b.Login) && b.Login.Equals(usuJaExistente.Login?.Trim(' '), StringComparison.CurrentCultureIgnoreCase)))
                                usuariosSistema.Add(usuJaExistente);

                            _repository?.Rollback(session);

                            continue;
                        }


                        var pass = !string.IsNullOrEmpty(item.Password) ? SW_Utils.Functions.Helper.DescriptografarPadraoEsol("", item.Password!) : "";
                        if (string.IsNullOrEmpty(pass))
                            pass = "Abc@123";


                        item.Password = pass;
                        item.PasswordConfirmation = pass;
                        item.Administrator = EnumSimNao.Sim;
                        item.Login = item.Login?.Trim(' ').RemoveAccents();
                        await RegistrarUsuarioExecute(_repository, _communicationProvider, _mapper, item, tipoTelefone, session);
                        usuariosSistema.Add(item);

                        if (_repository != null)
                        {
                            var resultCommit = await _repository.CommitAsync(session);
                            if (resultCommit.exception != null)
                                throw resultCommit.exception;
                        }
                    }
                    catch (Exception err)
                    {
                        _repository?.Rollback(session);
                        _logger.LogError(err, err.Message);
                    }
                }

            }
            catch (Exception err)
            {
                _logger.LogError(err, err.Message);
            }
        }

        private async Task InativarClienteLegadoSemContratosAtivos(IRepositoryHosted _repository, IHybrid_CM_Esolution_Communication _communicationProvider, IStatelessSession? session)
        {
            var usuarioSemContratosAtivosSistemaLegado = new List<UserRegisterInputModel>();

            if (_communicationProvider is IHybrid_CM_Esolution_Communication hybridProvider)
            {
                _logger.LogInformation("Verificando usu?rios sem contratos ativos em ambos os providers (CM + Esolution)");

                var usuariosSemContratosCm = await hybridProvider.GetUsuariosClientesSemCotasAtivoasNoSistemaLegado_Cm();
                if (usuariosSemContratosCm != null && usuariosSemContratosCm.Any())
                {
                    foreach (var usuario in usuariosSemContratosCm)
                        usuario.ProviderName = "CM";
                    usuarioSemContratosAtivosSistemaLegado.AddRange(usuariosSemContratosCm);
                }

                var usuariosSemContratosEsol = await hybridProvider.GetUsuariosClientesSemCotasAtivoasNoSistemaLegado_Esol();
                if (usuariosSemContratosEsol != null && usuariosSemContratosEsol.Any())
                {
                    foreach (var usuario in usuariosSemContratosEsol)
                        usuario.ProviderName = "ESOLUTION";
                    usuarioSemContratosAtivosSistemaLegado.AddRange(usuariosSemContratosEsol);
                }

                _logger.LogInformation($"Total de usu?rios sem contratos ativos (h?brido): {usuarioSemContratosAtivosSistemaLegado.Count}");
            }
            else
            {
                usuarioSemContratosAtivosSistemaLegado = await _communicationProvider.GetUsuariosClientesSemCotasAtivoasNoSistemaLegado();
            }

            var pessoasProvider = await _repository.FindBySql<PessoaSistemaXProvider>(@"Select 
                                                                                                psp.* 
                                                                                             From 
                                                                                                PessoaSistemaXProvider psp 
                                                                                             Where 
                                                                                                psp.NomeProvider IN ('CM', 'ESOLUTION') and 
                                                                                                Exists(Select 
                                                                                                         u.Pessoa From Usuario u 
                                                                                                       Where 
                                                                                                         Cast(u.Pessoa as varchar) = Cast(psp.PessoaSistema as Varchar) and u.Status = 1 and 
                                                                                                         u.DataHoraRemocao is null and Coalesce(u.Removido,0) = 0) ", session);

            foreach (var item in pessoasProvider.GroupBy(a=> a.NomeProvider))
            {
                //var providerName = !string.IsNullOrEmpty(item.Key.ProviderName) ? item.Key.ProviderName : _communicationProvider.CommunicationProviderName;
                //var pessoaProvider = pessoasProvider.FirstOrDefault(a =>
                //    !string.IsNullOrEmpty(a.PessoaProvider) &&
                //    a.PessoaProvider.Equals($"{item.Key.PessoaId}", StringComparison.InvariantCultureIgnoreCase) &&
                //    a.NomeProvider.Equals(providerName, StringComparison.InvariantCultureIgnoreCase));

                //if (pessoaProvider != null)
                //{
                //    var usuario = (await _repository.FindByHql<Usuario>($"From Usuario u Inner Join Fetch u.Pessoa p Where p.Id = {pessoaProvider.PessoaSistema} and u.DataHoraRemocao is null and Coalesce(u.Removido,0) = 0", session)).FirstOrDefault();
                //    if (usuario != null)
                //    {
                //        try
                //        {
                //            _repository.BeginTransaction(session);
                //            usuario.Status = EnumStatus.Inativo;
                //            await _repository.ForcedSave(usuario, session);
                //            var resultCommit = await _repository.CommitAsync(session);
                //            _logger.LogInformation($"Usu?rio: {usuario.Login} desativado, pois Não possui contrato ativo no provider {providerName}.");
                //        }
                //        catch (Exception err)
                //        {
                //            _repository.Rollback(session);
                //        }
                //    }
                //}
            }
        }
        

        private async Task CriarUsuariosClientesLegado(IRepositoryHosted _repository, ICommunicationProvider _communicationProvider, IProjectObjectMapper _mapper, Domain.Entities.Core.DadosPessoa.TipoTelefone tipoTelefone, IStatelessSession? session)
        {
            StringBuilder sbParametros = new(@$"Select 
                                    p.Id, 
                                    p.SiteParaReserva,
                                    p.Empresa as EmpresaId,
                                    p.AgruparCertidaoPorCliente,
                                    p.EmitirCertidaoPorUnidCliente,
                                    p.HabilitarBaixarBoleto,
                                    p.HabilitarPagamentosOnLine,
                                    p.HabilitarPagamentoEmPix,
                                    p.HabilitarPagamentoEmCartao,
                                    p.ExibirContasVencidas,
                                    p.QtdeMaximaDiasContasAVencer,
                                    p.PermitirUsuarioAlterarSeuEmail,
                                    p.PermitirUsuarioAlterarSeuDoc,
                                    Coalesce(p.IntegradoComMultiPropriedade,0) as IntegradoComMultiPropriedade,
                                    Coalesce(p.IntegradoComTimeSharing,0) as IntegradoComTimeSharing,
                                    p.ImagemHomeUrl1,
                                    p.ImagemHomeUrl2,
                                    p.ImagemHomeUrl3,
                                    p.ImagemHomeUrl4,
                                    p.NomeCondominio,
                                    p.CnpjCondominio,
                                    p.EnderecoCondominio,
                                    p.NomeAdministradoraCondominio,
                                    p.CnpjAdministradoraCondominio,
                                    p.EnderecoAdministradoraCondominio,
                                    p.ExibirFinanceirosDasEmpresaIds
                                    From 
                                    ParametroSistema p
                                    Inner Join Empresa e on p.Empresa = e.Id
                                    Where 1 = 1 ");



            var parametroSistema = (await _repository.FindBySql<ParametroSistemaViewModel>(sbParametros.ToString(), session)).FirstOrDefault();
            if (parametroSistema == null) return;

            try
            {
                var usuariosLegado = new List<UserRegisterInputModel>();

                if (_communicationProvider is IHybrid_CM_Esolution_Communication hybridProvider)
                {
                    _logger.LogInformation("Iniciando importa??o h?brida de clientes (CM + Esolution)");

                    if (parametroSistema.IntegradoComTimeSharing == EnumSimNao.Sim)
                    {
                        _logger.LogInformation("Importando usu?rios do sistema CM (MultiPropriedade)...");
                        var usuariosCm = await hybridProvider.GetClientesUsuariosLegado_Cm(parametroSistema);
                        if (usuariosCm != null && usuariosCm.Any())
                        {
                            foreach (var usuario in usuariosCm)
                            {
                                usuario.ProviderName = "CM";
                            }
                            usuariosLegado.AddRange(usuariosCm);
                            _logger.LogInformation($"Importados {usuariosCm.Count} usu?rios do sistema CM");
                        }
                    }

                    if (parametroSistema.IntegradoComMultiPropriedade == EnumSimNao.Sim)
                    {
                        _logger.LogInformation("Importando usu?rios do sistema Esolution (TimeSharing)...");
                        var usuariosEsol = await hybridProvider.GetClientesUsuariosLegado_Esol(parametroSistema);
                        if (usuariosEsol != null && usuariosEsol.Any())
                        {
                            foreach (var usuario in usuariosEsol)
                            {
                                usuario.ProviderName = "ESOLUTION";
                            }
                            usuariosLegado.AddRange(usuariosEsol);
                            _logger.LogInformation($"Importados {usuariosEsol.Count} usu?rios do sistema Esolution");
                        }
                    }

                    _logger.LogInformation($"Total de usu?rios para importa??o h?brida: {usuariosLegado.Count}");
                }
                else
                {
                    _logger.LogInformation("Provider Não ? h?brido. Usando importa??o padr?o...");
                    usuariosLegado = await _communicationProvider.GetClientesUsuariosLegado(parametroSistema);
                }

                var sb = new StringBuilder(@"select
                    u.Login,
                    p.Nome AS FullName,
                    p.Id AS PessoaId,
                    Coalesce(p.EmailPreferencial,p.EmailAlternativo) as Email,
                    Coalesce(pd.NumeroFormatado,pd.Numero) as CpfCnpj,
                    u.Administrador as Administrator
                    From 
                    usuario u
                    Inner Join Pessoa p on u.Pessoa = p.Id
                    Left Join PessoaDocumento pd on pd.Pessoa = p.Id
                    Left Join TipoDocumentoPessoa tdp on pd.TipoDocumento = tdp.Id and Lower(tdp.Nome) in ('cpf','cnpj','passaport')
                    Where 1 = 1 
                    and u.DataHoraRemocao is null and Coalesce(u.Removido,0) = 0 ");


                var usuariosSistema =
                    (await _repository.FindBySql<UserRegisterInputModel>(sb.ToString(), session)).AsList();


                List<UserProviderTemp> pessoasJaImportadas = new List<UserProviderTemp>();
                pessoasJaImportadas = (await _repository.FindBySql<UserProviderTemp>(@"Select 
                                                                                 u.PessoaProvider,
                                                                                u.NomeProvider
                                                                             From 
                                                                                PessoaSistemaXProvider u 
                                                                             where 
                                                                                PessoaProvider is not null and NomeProvider is not null", session)).AsList();


                foreach (var item in usuariosLegado)
                {
                    if (item.PessoaId != null && pessoasJaImportadas.Any(p => p.PessoaProvider == item.PessoaId && !string.IsNullOrEmpty(p.PessoaProvider) && p.NomeProvider!.Equals(item.ProviderName,StringComparison.InvariantCultureIgnoreCase)))
                    {
                        _logger.LogInformation($"Usu?rio com PessoaId {item.PessoaId} j? foi importado anteriormente. Pulando importa??o para este usu?rio.");
                        continue;
                    }

                    try
                    {
                        _repository.BeginTransaction(session);

                        var usuJaExistente =
                                        usuariosSistema.FirstOrDefault(a => (!string.IsNullOrEmpty(a.CpfCnpj) && 
                                        !string.IsNullOrEmpty(item.CpfCnpj) && 
                                        Convert.ToInt64(Helper.ApenasNumeros(a.CpfCnpj)) ==
                                    Convert.ToInt64(Helper.ApenasNumeros(item.CpfCnpj))) || 
                                    (!string.IsNullOrEmpty(a.Login) && a.Login.Trim(' ').Equals(item.Login?.Trim(' '), StringComparison.CurrentCultureIgnoreCase)));

                        if (usuJaExistente != null)
                        {
                            if (!usuariosSistema.Any(b => !string.IsNullOrEmpty(b.Login) && b.Login.Trim(' ').Equals(usuJaExistente.Login?.Trim(' '), StringComparison.CurrentCultureIgnoreCase)))
                                usuariosSistema.Add(usuJaExistente);

                            _repository.Rollback(session);

                            continue;
                        }

                        if (!string.IsNullOrEmpty(item.CpfCnpj))
                        {
                            var apenasNumeros = Helper.ApenasNumeros(item.CpfCnpj);
                            if (string.IsNullOrEmpty(apenasNumeros))
                            {
                                _logger.LogWarning($"Não foi poss?vel determinar o tipo do documento do usu?rio com login: {item.Login} - Pois o n?mero informado Não ? um CPF ou CNPJ v?lido: '{item.CpfCnpj}'");
                                item.CpfCnpj = null;
                                _repository.Rollback(session);
                                continue;
                            }

                            if (Helper.IsCpf(apenasNumeros))
                                item.TipoDocumentoClienteNome = "CPF";
                            else if (Helper.IsCnpj(apenasNumeros))
                                item.TipoDocumentoClienteNome = "CNPJ";
                            else
                            {
                                _logger.LogWarning($"Não foi poss?vel determinar o tipo do documento do usu?rio com login: {item.Login} - Pois o n?mero informado Não ? um CPF ou CNPJ v?lido: '{item.CpfCnpj}'");
                                item.CpfCnpj = null;
                                _repository.Rollback(session);
                                continue;
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"Não foi poss?vel determinar o tipo do documento do usu?rio com login: {item.Login} - Pois o n?mero informado Não ? um CPF ou CNPJ v?lido: '{item.CpfCnpj}'");
                            item.CpfCnpj = null;
                            _repository.Rollback(session);
                            continue;
                        }

                        var pass = item.Password = "Abc@123";

                        item.Administrator = EnumSimNao.Não;
                        await RegistrarUsuarioExecute(_repository, _communicationProvider, _mapper, item,tipoTelefone, session);
                        usuariosSistema.Add(item);

                        var resultCommit = await _repository.CommitAsync(session);
                        if (resultCommit.exception != null)
                            throw resultCommit.exception;

                        pessoasJaImportadas.Add(new UserProviderTemp() { NomeProvider = item.ProviderName, PessoaProvider = item.PessoaId });
                    }
                    catch (Exception err)
                    {
                        _repository.Rollback(session);
                        _logger.LogError(err, err.Message);
                    }
                }

            }
            catch (Exception err)
            {
                _logger.LogError(err, err.Message);
            }
        }

        private async Task<Domain.Entities.Core.Sistema.Usuario?> RegistrarUsuarioExecute(IRepositoryHosted _repository, ICommunicationProvider _communicationProvider, IProjectObjectMapper _mapper, UserRegisterInputModel userInputModel, Domain.Entities.Core.DadosPessoa.TipoTelefone tipoTelefone, NHibernate.IStatelessSession? session)
        {
            if (userInputModel == null) return null;

            try
            {

                EnumTipoPessoa tipoPessoa = EnumTipoPessoa.Fisica;
                var apenasNumeros = SW_Utils.Functions.Helper.ApenasNumeros(userInputModel.CpfCnpj);
                if (apenasNumeros.Length > 11)
                {
                    tipoPessoa = EnumTipoPessoa.Juridica;
                }

                var sb = new StringBuilder(@$"From 
                                            Pessoa p 
                                          Where 1 = 1 ");

                if (!string.IsNullOrEmpty(userInputModel.Email))
                {
                    sb.AppendLine($" and (Lower(p.EmailPreferencial) = '{userInputModel.Email!.ToLower()}' or Lower(p.EmailAlternativo) like '{userInputModel.Email.ToLower()}') ");
                }

                if (!string.IsNullOrEmpty(userInputModel.TipoDocumentoClienteNome) &&
                    !string.IsNullOrEmpty(userInputModel.CpfCnpj))
                {
                    if (Helper.IsCpf(apenasNumeros) || Helper.IsCnpj(apenasNumeros))
                    {
                        sb.AppendLine(@$" and Exists(Select 
                                                    pd.Pessoa 
                                                From PessoaDocumento pd 
                                                    Inner Join TipoDocumentoPessoa tdp on pd.TipoDocumento = tdp.Id and 
                                                    Lower(tdp.Nome) in ('cpf','cnpj','{userInputModel.TipoDocumentoClienteNome.ToLower().TrimEnd()}') 
                                                Where 
                                                    pd.Pessoa = p.Id and 
                                                    pd.ValorNumerico = '{apenasNumeros}') ");
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(userInputModel.Login))
                    {
                        sb.AppendLine($" and (Lower(p.Nome) = '{userInputModel?.FullName?.ToLower().TrimEnd().TrimStart()}' ) ");
                    }
                    else
                    {
                        sb.AppendLine(@$" and (Lower(p.Nome) = '{userInputModel?.FullName?.ToLower().TrimEnd().TrimStart()}' or 
                                            Exists(Select u.Pessoa From Usuario u Where u.Pessoa = p.Id and Lower(u.Login) = '{userInputModel?.Login?.ToLower()}' and u.DataHoraRemocao is null and Coalesce(u.Removido,0) = 0)) ");
                    }
                }


                var pessoa = (await _repository.FindByHql<Domain.Entities.Core.DadosPessoa.Pessoa>(sb.ToString(), session)).FirstOrDefault();

                if (pessoa == null)
                {
                    sb = new StringBuilder(@$"From 
                                            Pessoa p 
                                          Where 1 = 1 
                                            and Exists(Select u.Pessoa From Usuario u Where u.Pessoa = p.Id and Lower(u.Login) = '{userInputModel?.Login?.ToLower()}' and u.DataHoraRemocao is null and Coalesce(u.Removido,0) = 0) ");

                    pessoa = (await _repository.FindByHql<Domain.Entities.Core.DadosPessoa.Pessoa>(sb.ToString(), session)).FirstOrDefault();
                }

                if (pessoa == null)
                {
                    pessoa = new Pessoa()
                    {
                        Nome = userInputModel?.FullName,
                        UsuarioCriacao = 1,
                        DataHoraCriacao = DateTime.Now,
                        EmailPreferencial = userInputModel?.Email,
                        TipoPessoa = tipoPessoa,
                        RegimeTributario = EnumTipoTributacao.SimplesNacional,
                        NomeFantasia = userInputModel?.FullName
                    };

                }
                else
                {
                    var usuarioJaExistente = (await _repository.FindByHql<Domain.Entities.Core.Sistema.Usuario>($"From Usuario u Inner Join Fetch u.Pessoa p Where p.Id = {pessoa.Id} and u.DataHoraRemocao is null and Coalesce(u.Removido,0) = 0 ", session)).FirstOrDefault();
                    if (usuarioJaExistente != null)
                    {
                        var pessoaTelefone = (await _repository.FindByHql<Domain.Entities.Core.DadosPessoa.PessoaTelefone>($"From PessoaTelefone pt Where pt.Pessoa.Id = {pessoa.Id}", session)).FirstOrDefault();
                        if (pessoaTelefone == null && !string.IsNullOrEmpty(userInputModel?.Telefone))
                        {
                            await SincronizarTelefone(_repository, userInputModel, session, pessoa,tipoTelefone);
                        }
                        return usuarioJaExistente;
                    } 
                }

                var login = userInputModel?.CpfCnpj ?? userInputModel?.Email;

                if (!string.IsNullOrEmpty(userInputModel?.Login))
                {
                    var loginUtilizado = (await _repository.FindBySql<UsuarioInputModel>(@$"Select 
                            u.Login From Usuario u Where Lower(RTrim(LTrim(u.Login))) = '{userInputModel.Login.ToLower().Trim(' ')}' and 
                            u.DataHoraRemocao is null and Coalesce(u.Removido,0) = 0", session)).FirstOrDefault();
                    if (loginUtilizado == null)
                    {
                        login = userInputModel.Login.TrimStart().TrimEnd().RemoveAccents();
                    }
                }

                Domain.Entities.Core.Sistema.Usuario usu = new()
                {
                    Pessoa = pessoa,
                    Login = login!.Replace(" ","").RemoveAccents(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(userInputModel?.Password),
                    DataHoraCriacao = DateTime.Now,
                    Status = EnumStatus.Ativo,
                    Administrador = userInputModel != null ? userInputModel.Administrator.GetValueOrDefault(EnumSimNao.Não) : EnumSimNao.Não
                };


                var exists = !string.IsNullOrEmpty(userInputModel?.Email) ? (await _repository.FindByHql<Domain.Entities.Core.Sistema.Usuario>($"From Usuario u Inner Join Fetch u.Pessoa p Where u.Login = '{usu.Login}'  and u.DataHoraRemocao is null and Coalesce(u.Removido,0) = 0", session)).FirstOrDefault() :
                    (await _repository.FindByHql<Domain.Entities.Core.Sistema.Usuario>($"From Usuario u Inner Join Fetch u.Pessoa p Where u.Login = '{usu.Login}'  and u.DataHoraRemocao is null and Coalesce(u.Removido,0) = 0", session)).FirstOrDefault();

                if (exists != null)
                    throw new Exception($"J? existe um usu?rio com o login: '{usu.Login}'");

                if (pessoa != null)
                {
                    await _repository.ForcedSave(pessoa, session);

                    await _repository.ForcedSave(usu, session);
                    var tipoDocumentosPessoa = (await _repository.FindBySql<TipoDocumentoPessoa>($"Select tdp.* From TipoDocumentoPessoa tdp Where Lower(tdp.Nome) in ('cpf','cnpj','passaport') and tdp.TipoPessoa = {(int?)pessoa.TipoPessoa}", session)).FirstOrDefault();
                    if (tipoDocumentosPessoa != null && (!string.IsNullOrEmpty(userInputModel?.CpfCnpj) && (Helper.IsCnpj(userInputModel.CpfCnpj) || Helper.IsCpf(userInputModel.CpfCnpj) || (!string.IsNullOrEmpty(userInputModel.TipoDocumentoClienteNome) &&
                        userInputModel.TipoDocumentoClienteNome.ToLower().Contains("passaport", StringComparison.CurrentCultureIgnoreCase)))))
                    {
                        var retorno = await SincronizarDocumentos(_repository, _mapper, pessoa, session, true, new PessoaDocumentoInputModel() { TipoDocumentoId = tipoDocumentosPessoa.Id, Numero = $"{apenasNumeros}", PessoaId = pessoa.Id });
                    }
                    if (string.IsNullOrEmpty(usu.Login))
                    {
                        usu.Login = $"User{usu.Id}";
                    }

                    usu.UsuarioCriacao = usu.Id;
                    if (pessoa.UsuarioCriacao.GetValueOrDefault(0) == 0)
                    {
                        pessoa.UsuarioCriacao = usu.Id;
                        await _repository.ForcedSave(pessoa, session);
                    }

                    await SincronizarTelefone(_repository, userInputModel, session, pessoa, tipoTelefone);

                    await _repository.ForcedSave(usu, session);

                    //PessoaId:10211|UsuarioId:9
                    usu.ProviderChaveUsuario = $"PessoaId:{userInputModel?.PessoaId}|UsuarioId:{usu.Id}";
                    await _repository.ForcedSave(usu, session);

                    var providerName = !string.IsNullOrEmpty(userInputModel?.ProviderName)
                        ? userInputModel.ProviderName
                        : _communicationProvider.CommunicationProviderName;

                    var psxpp = new PessoaSistemaXProvider()
                    {
                        PessoaSistema = $"{usu?.Pessoa?.Id}",
                        PessoaProvider = userInputModel?.PessoaId,
                        NomeProvider = providerName
                    };
                    await _repository.ForcedSave(psxpp, session);
                }

                await VincularEmpresasAoUsuario(_repository, usu, session);
                await CriarOuVincularTagGeral(_repository, usu,session);

                return usu;
            }
            catch (Exception err)
            {
                throw;
            }
        }

        private static async Task SincronizarTelefone(IRepositoryHosted _repository, UserRegisterInputModel userInputModel, IStatelessSession? session, Pessoa pessoa, Domain.Entities.Core.DadosPessoa.TipoTelefone tipoTelefone)
        {
            if (!string.IsNullOrEmpty(userInputModel?.Telefone))
            {
                var celularValido = false;

                var apenasNumerosTelefone = SW_Utils.Functions.Helper.ApenasNumeros(userInputModel.Telefone);
                if (!string.IsNullOrEmpty(apenasNumerosTelefone))
                {
                    Int64 telInt64;

                    var numeroTelefoneValidarInt64 = Int64.TryParse(apenasNumerosTelefone, out telInt64) ?  Convert.ToInt64(apenasNumerosTelefone) : 0;
                    var numeroTelefoneValidar = numeroTelefoneValidarInt64.ToString();

                    if (!string.IsNullOrEmpty(numeroTelefoneValidar))
                    {
                        if (numeroTelefoneValidar.Length == 12 || numeroTelefoneValidar.Length == 13)
                        {
                            if (numeroTelefoneValidar.Substring(4).StartsWith('9') || numeroTelefoneValidar.Substring(4).StartsWith('8') || numeroTelefoneValidar.Substring(4).StartsWith('7'))
                                celularValido = true;
                        }
                        else if (numeroTelefoneValidar.Length == 11 || numeroTelefoneValidar.Length == 10)
                        {
                            if (numeroTelefoneValidar.Substring(2).StartsWith('9') || numeroTelefoneValidar.Substring(2).StartsWith('8') || numeroTelefoneValidar.Substring(2).StartsWith('7'))
                                celularValido = true;
                        }
                        else if (numeroTelefoneValidar.Length > 16)
                        {
                            if (numeroTelefoneValidar.Substring(0, 2).StartsWith("55"))
                            {
                                numeroTelefoneValidar = numeroTelefoneValidar.Substring(2, 10);
                                celularValido = true;
                            }
                            else
                            {
                                numeroTelefoneValidar = numeroTelefoneValidar.Substring(0, 10);
                                celularValido = true;
                            }
                        }

                        if (string.IsNullOrEmpty(numeroTelefoneValidar) || numeroTelefoneValidar.Length < 10)
                            celularValido = false;

                        if (celularValido && numeroTelefoneValidar.Substring(0, 2).Equals("55"))
                        {
                            numeroTelefoneValidar = numeroTelefoneValidar.Substring(2);
                        }

                        if (!string.IsNullOrEmpty(numeroTelefoneValidar) && numeroTelefoneValidar.Length == 10)
                        {
                            numeroTelefoneValidar = $"{numeroTelefoneValidar.Substring(0, 2)}9{numeroTelefoneValidar.Substring(2)}";
                        }

                        if (!string.IsNullOrEmpty(numeroTelefoneValidar) && (numeroTelefoneValidar.Length != 11 || (!numeroTelefoneValidar.Substring(3, 1).Equals("9") &&
                            !numeroTelefoneValidar.Substring(3, 1).Equals("8") &&
                            !numeroTelefoneValidar.Substring(3, 1).Equals("7"))))
                        {
                            celularValido = false;
                        }

                        if (string.IsNullOrEmpty(numeroTelefoneValidar) || numeroTelefoneValidar.Length < 11)
                        {
                            celularValido = false;
                        }

                        if (celularValido)
                        {
                            var telefoneExistente = (await _repository.FindByHql<Domain.Entities.Core.DadosPessoa.PessoaTelefone>($"From PessoaTelefone pt Where pt.Pessoa.Id = {pessoa.Id} and pt.Numero = '{numeroTelefoneValidar}'", session)).FirstOrDefault();
                            if (telefoneExistente == null)
                            {
                                var telefone = new Domain.Entities.Core.DadosPessoa.PessoaTelefone()
                                {
                                    Pessoa = pessoa,
                                    Numero = numeroTelefoneValidar,
                                    TipoTelefone = tipoTelefone,
                                    UsuarioCriacao = 1,
                                    DataHoraCriacao = DateTime.Now,
                                    NumeroFormatado = SW_Utils.Functions.Helper.Formatar(numeroTelefoneValidar, "(##) #####-####")
                                };
                                await _repository.ForcedSave(telefone, session);
                            }
                        }
                    }
                }
            }
        }

        private async Task CriarOuVincularTagGeral(IRepositoryHosted _repository, Domain.Entities.Core.Sistema.Usuario user, NHibernate.IStatelessSession? session)
        {
            var tagId = _configuration.GetValue<int>("TagGeralId");

            var tag = (await _repository.FindByHql<Tags>($"From Tags t Where t.Id = {tagId} or Lower(t.Nome) = 'geral'",session)).FirstOrDefault();
            if (tag == null)
            {
                tag = new Tags()
                {
                    Nome = "Geral"
                };


                var result = await _repository.ForcedSave(tag, session);
                if (string.IsNullOrEmpty(tag.Path))
                {
                    tag.Path = $"tags/{tag.Id}";
                    await _repository.ForcedSave(tag, session);
                }
            }


            var userTags = (await _repository.FindBySql<UsuarioTagsModel>($"Select ut.Usuario as UsuarioId  From UsuarioTags ut Where ut.Usuario = {user.Id} and ut.Tags = {tagId}", session)).FirstOrDefault();
            if (userTags == null)
            {
                var userTag = new UsuarioTags()
                {
                    Usuario = new Domain.Entities.Core.Sistema.Usuario() { Id = user.Id },
                    Tags = tag
                };
                await _repository.ForcedSave(userTag, session);
            }
        }

        private async Task VincularEmpresasAoUsuario(IRepositoryHosted _repository, Domain.Entities.Core.Sistema.Usuario? user, NHibernate.IStatelessSession? session)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var empresaUsuario = (await _repository.FindBySql<Models.SystemModels.EmpresaUsuarioModel>($"Select eu.Usuario as UsuarioId From EmpresaUsuario eu Where eu.Usuario = {user!.Id}", session)).FirstOrDefault();
            if (empresaUsuario == null)
            {
                try
                {
                    var empresas = (await _repository.FindBySql<EmpresaModel>("Select e.Id From Empresa e", session)).AsList();
                    foreach (var empresa in empresas)
                    {
                        var empUsuario = new EmpresaUsuario
                        {
                            Empresa = new Domain.Entities.Core.Framework.Empresa() { Id = empresa.Id.GetValueOrDefault() },
                            Usuario = new Domain.Entities.Core.Sistema.Usuario() { Id = user.Id }
                        };

                        await _repository.ForcedSave(empUsuario, session);
                    }

                }
                catch (Exception err)
                {
                    _logger.LogError(err, err.Message);
                    throw err;
                }

            }
        }

        public async Task<List<int>> SincronizarDocumentos(IRepositoryHosted _repository, IProjectObjectMapper _mapper, Domain.Entities.Core.DadosPessoa.Pessoa? pessoa, IStatelessSession? session, bool validarAlteracaoDocumento = false, params PessoaDocumentoInputModel[] documentos)
        {
            if (pessoa == null) throw new ArgumentNullException(nameof(pessoa));
            List<int> result = new List<int>();
            if (documentos != null)
            {
                foreach (var documento in documentos)
                {
                    var apenasNumero = SW_Utils.Functions.Helper.ApenasNumeros(documento.Numero);
                    var tipoDocumento = (await _repository.FindBySql<TipoDocumentoPessoa>($"Select te.* From TipoDocumentoPessoa te Where te.Id =  {documento.TipoDocumentoId.GetValueOrDefault()}", session)).FirstOrDefault();
                    if (tipoDocumento == null)
                        throw new ArgumentException($"Não foi encontrado o tipo de documento informado: {documento.TipoDocumentoId.GetValueOrDefault()}");


                    if (tipoDocumento.ExigeDataEmissao.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim && documento.DataEmissao.GetValueOrDefault(DateTime.MinValue) == DateTime.MinValue)
                        throw new Exception($"O tipo de documento: {tipoDocumento.Nome} exige a informação da daa de emissão no documento: {documento.Numero}");

                    if (tipoDocumento.ExigeDataValidade.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim && documento.DataValidade.GetValueOrDefault(DateTime.MinValue) == DateTime.MinValue)
                        throw new Exception($"O tipo de documento: {tipoDocumento.Nome} exige a informação da data de validade no documento: {documento.Numero}");

                    if (tipoDocumento.ExigeOrgaoEmissor.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim && string.IsNullOrEmpty(documento.OrgaoEmissor))
                        throw new Exception($"O tipo de documento: {tipoDocumento.Nome} exige a informação do órgão emissor no documento: {documento.Numero}");

                    var documentoExistente = documento.Id.GetValueOrDefault(0) > 0 ?
                        (await _repository.FindByHql<PessoaDocumento>($"From PessoaDocumento pe Inner Join Fetch pe.TipoDocumento te Inner Join Fetch pe.Pessoa p Where pe.Id = {documento.Id.GetValueOrDefault()}", session)).FirstOrDefault() :
                        (await _repository.FindByHql<PessoaDocumento>($"From PessoaDocumento pe Inner Join Fetch pe.TipoDocumento te Inner Join Fetch pe.Pessoa p Where p.Id = {pessoa?.Id} and ((pe.Numero = '{documento.Numero?.TrimEnd()}' or pe.ValorNumerico = '{apenasNumero}') or te.Id = {tipoDocumento.Id})", session)).FirstOrDefault();


                    if (documentoExistente == null)
                    {
                        documentoExistente = _mapper.Map<PessoaDocumento>(documento);
                        documentoExistente.Pessoa = pessoa;
                        documentoExistente.Numero = Helper.ApenasNumeros(documento.Numero);
                    }
                    else
                    {

                        documentoExistente = _mapper.Map(documento, documentoExistente);
                        documentoExistente.Numero = Helper.ApenasNumeros(documento.Numero);
                        documentoExistente.Pessoa = pessoa;
                    }

                    if (documentoExistente.Numero.Length != 11 && documentoExistente.Numero.Length != 14)
                    {
                        if (documentoExistente.Numero.Length <= 10)
                            documentoExistente.Numero = documentoExistente.Numero.PadLeft(11, '0');
                        else if (documentoExistente.Numero.Length > 11)
                            documentoExistente.Numero = documentoExistente.Numero.PadLeft(14, '0');
                    }

                    documentoExistente.DataEmissao = tipoDocumento.ExigeDataEmissao.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim ? documento.DataEmissao.GetValueOrDefault() : null;
                    documentoExistente.DataValidade = tipoDocumento.ExigeDataValidade.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim ? documento.DataValidade.GetValueOrDefault() : null;
                    documentoExistente.OrgaoEmissor = tipoDocumento.ExigeOrgaoEmissor.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim ? documento.OrgaoEmissor : null;

                    documentoExistente.ValorNumerico = Helper.ApenasNumeros(documentoExistente.Numero);

                    if (!string.IsNullOrEmpty(tipoDocumento.Mascara))
                        documentoExistente.NumeroFormatado = Helper.Formatar(documentoExistente.ValorNumerico, tipoDocumento.Mascara);

                    await _repository.ForcedSave(documentoExistente, session);

                    result.Add(documentoExistente.Id);

                }
            }

            return result;
        }

        public async Task<List<int>> SincronizarTelefones(IRepositoryHosted _repository, IProjectObjectMapper _mapper, Domain.Entities.Core.DadosPessoa.Pessoa? pessoa, IStatelessSession? session, params PessoaTelefoneInputModel[] telefones)
        {
            if (pessoa == null) throw new ArgumentNullException(nameof(pessoa));
            List<int> result = new List<int>();
            if (telefones != null && telefones.Any())
            {
                foreach (var telefone in telefones)
                {
                    if (!telefone.Preferencial.HasValue)
                        telefone.Preferencial = EnumSimNao.Não;

                    var tipoTelefone = (await _repository.FindBySql<Domain.Entities.Core.DadosPessoa.TipoTelefone>($"Select te.* From TipoTelefone te Where te.Id =  {telefone.TipoTelefoneId.GetValueOrDefault()}", session)).FirstOrDefault();
                    if (tipoTelefone == null)
                        throw new ArgumentException($"Não foi encontrado o tipo de telefone informado: {telefone.TipoTelefoneId.GetValueOrDefault()}");


                    var telefoneExistente = telefone.Id.GetValueOrDefault(0) > 0 ?
                        (await _repository.FindByHql<Domain.Entities.Core.DadosPessoa.PessoaTelefone>($"From PessoaTelefone pe Inner Join Fetch pe.TipoTelefone te Inner Join Fetch pe.Pessoa p Where pe.Id = {telefone.Id.GetValueOrDefault()}", session)).FirstOrDefault() :
                        (await _repository.FindByHql<Domain.Entities.Core.DadosPessoa.PessoaTelefone>($"From PessoaTelefone pe Inner Join Fetch pe.TipoTelefone te Inner Join Fetch pe.Pessoa p Where p.Id = {pessoa?.Id} and pe.Numero = '{telefone.Numero?.TrimEnd()}'", session)).FirstOrDefault();

                    if (telefoneExistente == null)
                    {
                        telefoneExistente = _mapper.Map<Domain.Entities.Core.DadosPessoa.PessoaTelefone>(telefone);
                        telefoneExistente.Pessoa = pessoa;
                        telefoneExistente.Numero = Helper.ApenasNumeros(telefone.Numero);
                    }
                    else
                    {

                        telefoneExistente = _mapper.Map(telefone, telefoneExistente);
                        telefoneExistente.Numero = Helper.ApenasNumeros(telefone.Numero);
                        telefoneExistente.Pessoa = pessoa;
                    }


                    if (!string.IsNullOrEmpty(tipoTelefone.Mascara))
                        telefoneExistente.NumeroFormatado = Helper.Formatar(telefoneExistente.Numero, tipoTelefone.Mascara);

                    if (telefoneExistente.Preferencial == EnumSimNao.Sim && pessoa != null && pessoa.Id > 0)
                    {
                        var outrosTelefones = (await _repository.FindByHql<Domain.Entities.Core.DadosPessoa.PessoaTelefone>($"From PessoaTelefone pt Inner Join Fetch pt.TipoTelefone tt Inner Join Fetch pt.Pessoa p Where p.Id = {pessoa.Id} and pt.Id <> {telefoneExistente.Id} and pt.Preferencial = 1", session)).AsList();
                        foreach (var item in outrosTelefones)
                        {
                            item.Preferencial = EnumSimNao.Não;
                            await _repository.ForcedSave(item, session);
                        }
                    }

                    await _repository.ForcedSave(telefoneExistente, session);

                    result.Add(telefoneExistente.Id);
                }
            }
            return result;
        }

        public async Task<List<int>> SincronizarEnderecos(IRepositoryHosted _repository, IProjectObjectMapper _mapper, Pessoa? pessoa, IStatelessSession? session, params PessoaEnderecoInputModel[] enderecos)
        {
            if (pessoa == null) throw new ArgumentNullException(nameof(pessoa));

            List<int> result = new List<int>();
            if (enderecos != null && enderecos.Any())
            {
                foreach (var endereco in enderecos)
                {
                    if (!endereco.Preferencial.HasValue)
                        endereco.Preferencial = EnumSimNao.Não;

                    var tipoEndereco = (await _repository.FindBySql<Domain.Entities.Core.DadosPessoa.TipoEndereco>($"Select te.* From TipoEndereco te Where te.Id =  {endereco.TipoEnderecoId.GetValueOrDefault()}", session)).FirstOrDefault();
                    if (tipoEndereco == null)
                        throw new ArgumentException($"Não foi encontrado o tipo de endere?o informado: {endereco.TipoEnderecoId.GetValueOrDefault()}");

                    var cidade = (await _repository.FindByHql<Domain.Entities.Core.Geral.Cidade>($"From Cidade c Inner Join Fetch c.Estado e Inner Join Fetch e.Pais p Where c.Id =  {endereco.CidadeId.GetValueOrDefault()}", session)).FirstOrDefault();
                    if (cidade == null)
                        throw new ArgumentException($"Não foi encontrada a Cidade informada: {endereco.CidadeId.GetValueOrDefault()}");

                    var enderecoExistente = endereco.Id.GetValueOrDefault(0) > 0 ?
                        (await _repository.FindByHql<Domain.Entities.Core.DadosPessoa.PessoaEndereco>($"From PessoaEndereco pe Inner Join Fetch pe.TipoEndereco te Inner Join Fetch pe.Pessoa p Inner Join Fetch pe.Cidade cid Inner Join Fetch cid.Estado est Inner Join Fetch est.Pais pa Where pe.Id = {endereco.Id.GetValueOrDefault()}", session)).FirstOrDefault() :
                        (await _repository.FindByHql<Domain.Entities.Core.DadosPessoa.PessoaEndereco>($"From PessoaEndereco pe Inner Join Fetch pe.TipoEndereco te Inner Join Fetch pe.Pessoa p Inner Join Fetch pe.Cidade cid Inner Join Fetch cid.Estado est Inner Join Fetch est.Pais pa Where p.Id = {pessoa?.Id} and Lower(pe.Logradouro) = '{endereco?.Logradouro?.TrimEnd().ToLower()}'", session)).FirstOrDefault();



                    if (enderecoExistente == null)
                    {
                        enderecoExistente = _mapper.Map(endereco, enderecoExistente);
                        enderecoExistente!.Pessoa = pessoa;

                    }
                    else
                    {
                        enderecoExistente = _mapper.Map(endereco, enderecoExistente);
                        enderecoExistente.Pessoa = pessoa;

                    }

                    await _repository.ForcedSave(enderecoExistente, session);


                    result.Add(enderecoExistente.Id);
                }
            }
            return result;
        }

        private async Task<ParametroSistemaViewModel?> GetParametroSistema(IRepositoryHosted _repository, NHibernate.IStatelessSession? session)
        {
            try
            {
                var empresas = (await _repository.FindByHql<Domain.Entities.Core.Framework.Empresa>("From Empresa e", session)).AsList();
                if (empresas == null || empresas.Count() != 1)
                    return null;

                var sbParametros = new StringBuilder();
                sbParametros.AppendLine("Select");
                sbParametros.AppendLine("    p.Id,");
                sbParametros.AppendLine("    p.CriarUsuariosLegado,");
                sbParametros.AppendLine("    p.CriarUsuariosClientesLegado");
                sbParametros.AppendLine("From");
                sbParametros.AppendLine("    ParametroSistema p");
                sbParametros.AppendLine($"Where p.Empresa = {empresas.First().Id}");

                var parametroSistema = (await _repository.FindBySql<ParametroSistemaViewModel>(sbParametros.ToString(), session)).FirstOrDefault();
                return parametroSistema;
            }
            catch (Exception err)
            {
                _logger.LogWarning(err, "Não foi possível buscar ParametroSistema, usando configuração padrão");
                return null;
            }
        }

        private async Task GravarParametroSistema(IRepositoryHosted _repository, NHibernate.IStatelessSession? session)
        {
            try
            {
                _repository.BeginTransaction(session);
                var empresas = (await _repository.FindByHql<Domain.Entities.Core.Framework.Empresa>("From Empresa e", session)).AsList();
                if (empresas != null && empresas.Count() == 1)
                {
                    var parametroSistema = (await _repository.FindByHql<ParametroSistema>($"From ParametroSistema ps Inner Join Fetch ps.Empresa emp Where emp.Id = {empresas.First().Id}", session)).FirstOrDefault();
                    if (parametroSistema == null)
                    {
                        parametroSistema = new ParametroSistema()
                        {
                            Empresa = empresas.First(),
                            AgruparCertidaoPorCliente = EnumSimNao.Sim,
                            EmitirCertidaoPorUnidCliente = EnumSimNao.Não,
                            HabilitarBaixarBoleto = EnumSimNao.Não,
                            HabilitarPagamentosOnLine = EnumSimNao.Não,
                            HabilitarPagamentoEmCartao = EnumSimNao.Não,
                            HabilitarPagamentoEmPix = EnumSimNao.Não,
                            ExibirContasVencidas = EnumSimNao.Não,
                            PermitirUsuarioAlterarSeuEmail = EnumSimNao.Não,
                            PermitirUsuarioAlterarSeuDoc = EnumSimNao.Não
                        };

                        await _repository.ForcedSave(parametroSistema, session);
                    }

                }

                var resultCommit = await _repository.CommitAsync(session);
                if (resultCommit.exception != null)
                    throw resultCommit.exception;
            }
            catch (Exception err)
            {
                _repository.Rollback(session);
                _logger.LogError(err, err.Message);
            }
        }

        private async Task VincularUsuarioEmpresa(IRepositoryHosted _repository, NHibernate.IStatelessSession? session)
        {
            try
            {
                _repository.BeginTransaction(session);
                var empresas = (await _repository.FindByHql<Domain.Entities.Core.Framework.Empresa>("From Empresa e", session)).AsList();
                if (empresas != null && empresas.Count() == 1)
                {
                    var empresaFirst = empresas.First();
                    var usuarios = (await _repository.FindBySql<Models.SystemModels.UsuarioModel>(@$"Select 
                                                                                    u.Id as UsuarioId,
                                                                                    u.Id
                                                                                 From 
                                                                                    Usuario u 
                                                                                 Where 
                                                                                    Not Exists(Select eu.Usuario From EmpresaUsuario eu Where eu.Usuario = u.Id and eu.Empresa = {empresaFirst.Id})
                                                                                    and u.DataHoraRemocao is null and Coalesce(u.Removido,0) = 0", session)).AsList();
                    foreach (var usuario in usuarios)
                    {
                        var empUsuario = new EmpresaUsuario()
                        {
                            Empresa = empresaFirst,
                            Usuario = new Domain.Entities.Core.Sistema.Usuario() { Id = usuario.Id.GetValueOrDefault(usuario.UsuarioId.GetValueOrDefault()) }
                        };
                        await _repository.ForcedSave(empUsuario, session);
                    }

                }

                var resultCommit = await _repository.CommitAsync(session);
                if (resultCommit.exception != null)
                    throw resultCommit.exception;
            }
            catch (Exception err)
            {
                _repository.Rollback(session);
                _logger.LogError(err, err.Message);
            }
        }

        private async Task ConfigurarDadosPadroes(IRepositoryHosted _repository, NHibernate.IStatelessSession? session)
        {

            try
            {
                await TiposEnderecos(_repository, session);
                await TiposTelefones(_repository, session);
                await TiposDocumentos(_repository, session);
                if (_configuration.GetValue<bool>("ImportarCidades"))
                    await ImportarCidades(_repository, session);
            }
            catch (Exception err)
            {
                _logger.LogError(err, err.Message);
            }
        }

        private async Task TiposEnderecos(IRepositoryHosted _repository, NHibernate.IStatelessSession? session)
        {
            try
            {
                _repository.BeginTransaction(session);
                var tipoEnderecos = (await _repository.FindByHql<Domain.Entities.Core.DadosPessoa.TipoEndereco>("From TipoEndereco", session)).AsList();
                var tipoendComercial = tipoEnderecos.FirstOrDefault(a => a.Nome.Contains("comercial", StringComparison.CurrentCultureIgnoreCase));
                if (tipoendComercial == null)
                    tipoendComercial = new Domain.Entities.Core.DadosPessoa.TipoEndereco()
                    {
                        Nome = "Comercial"
                    };

                await _repository.ForcedSave(tipoendComercial, session);

                var tipoendResidencial = tipoEnderecos.FirstOrDefault(a => a.Nome.Contains("residencial", StringComparison.CurrentCultureIgnoreCase));
                if (tipoendResidencial == null)
                    tipoendResidencial = new Domain.Entities.Core.DadosPessoa.TipoEndereco()
                    {
                        Nome = "Residencial"
                    };

                await _repository.ForcedSave(tipoendResidencial, session);

                var resultCommit = await _repository.CommitAsync(session);
                if (resultCommit.exception != null)
                    throw resultCommit.exception;
            }
            catch (Exception err)
            {
                _repository.Rollback(session);
                _logger.LogError(err, err.Message);
            }
        }

        private async Task TiposTelefones(IRepositoryHosted _repository, NHibernate.IStatelessSession? session)
        {
            try
            {
                _repository.BeginTransaction(session);

                var tipoTelefones = (await _repository.FindByHql<Domain.Entities.Core.DadosPessoa.TipoTelefone>("From TipoTelefone", session)).AsList();
                var tipoFixo = tipoTelefones.FirstOrDefault(a => a.Nome.Contains("fixo", StringComparison.CurrentCultureIgnoreCase));
                if (tipoFixo == null)
                {
                    tipoFixo = new Domain.Entities.Core.DadosPessoa.TipoTelefone()
                    {
                        Nome = "Fixo",
                        Mascara = "(##) ####-####"
                    };

                    await _repository.ForcedSave(tipoFixo, session);
                }

                var tipoCelular = tipoTelefones.FirstOrDefault(a => a.Nome.Contains("celular", StringComparison.CurrentCultureIgnoreCase));
                if (tipoCelular == null)
                {
                    tipoCelular = new Domain.Entities.Core.DadosPessoa.TipoTelefone()
                    {
                        Nome = "Celular",
                        Mascara = "(##) #####-####"
                    };

                    await _repository.ForcedSave(tipoCelular, session);
                }

                var resultCommit = await _repository.CommitAsync(session);
                if (resultCommit.exception != null)
                    throw resultCommit.exception;
            }
            catch (Exception err)
            {
                _repository.Rollback(session);
                _logger.LogError(err, err.Message);
            }
        }


        private async Task TiposDocumentos(IRepositoryHosted _repository, NHibernate.IStatelessSession? session)
        {
            try
            {
                _repository.BeginTransaction(session);
                var tiposDocumentos = (await _repository.FindByHql<Domain.Entities.Core.DadosPessoa.TipoDocumentoPessoa>("From TipoDocumentoPessoa", session)).AsList();

                var tipoCpf = tiposDocumentos.FirstOrDefault(a => a.Nome.Contains("cpf", StringComparison.CurrentCultureIgnoreCase));
                if (tipoCpf == null)
                {
                    tipoCpf = new Domain.Entities.Core.DadosPessoa.TipoDocumentoPessoa()
                    {
                        Nome = "CPF",
                        Mascara = "###.###.###-##",
                        ExigeDataEmissao = EnumSimNao.Não,
                        ExigeDataValidade = EnumSimNao.Não,
                        TipoPessoa = EnumTiposPessoa.PessoaFisica
                    };

                    await _repository.ForcedSave(tipoCpf, session);
                }

                var tipoCnpj = tiposDocumentos.FirstOrDefault(a => a.Nome.Contains("cnpj", StringComparison.CurrentCultureIgnoreCase));
                if (tipoCnpj == null)
                {
                    tipoCnpj = new Domain.Entities.Core.DadosPessoa.TipoDocumentoPessoa()
                    {
                        Nome = "CNPJ",
                        Mascara = "##.###.###/####-##",
                        ExigeDataEmissao = EnumSimNao.Não,
                        ExigeDataValidade = EnumSimNao.Não,
                        TipoPessoa = EnumTiposPessoa.PessoaJuridica
                    };

                    await _repository.ForcedSave(tipoCnpj, session);
                }

                var tipoRG = tiposDocumentos.FirstOrDefault(a => a.Nome.Contains("RG", StringComparison.CurrentCultureIgnoreCase));
                if (tipoCnpj == null)
                {
                    tipoCnpj = new Domain.Entities.Core.DadosPessoa.TipoDocumentoPessoa()
                    {
                        Nome = "RG",
                        Mascara = null,
                        ExigeDataEmissao = EnumSimNao.Sim,
                        ExigeDataValidade = EnumSimNao.Sim,
                        TipoPessoa = EnumTiposPessoa.PessoaFisica
                    };

                    await _repository.ForcedSave(tipoRG, session);
                }


                var tipoPassaporte = tiposDocumentos.FirstOrDefault(a => a.Nome!.Contains("Passaporte", StringComparison.CurrentCultureIgnoreCase));
                if (tipoPassaporte == null)
                {
                    tipoPassaporte = new Domain.Entities.Core.DadosPessoa.TipoDocumentoPessoa()
                    {
                        Nome = "Passaporte",
                        Mascara = null,
                        ExigeDataEmissao = EnumSimNao.Sim,
                        ExigeDataValidade = EnumSimNao.Sim,
                        TipoPessoa = EnumTiposPessoa.PessoaFisica
                    };

                    await _repository.ForcedSave(tipoPassaporte, session);
                }

                var resultCommit = await _repository.CommitAsync(session);
                if (resultCommit.exception != null)
                    throw resultCommit.exception;

            }
            catch (Exception err)
            {
                _repository.Rollback(session);
                _logger.LogError(err, err.Message);
            }

        }

        private async Task ConfigurarEmpresa(IRepositoryHosted _repository, IHybrid_CM_Esolution_Communication _communicationProvider, NHibernate.IStatelessSession? session)
        {
            var empresaExistente = (await _repository.FindByHql<Domain.Entities.Core.Framework.Empresa>("From Empresa e", session)).FirstOrDefault();
            if (empresaExistente == null)
            {
                try
                {
                    _repository.BeginTransaction(session);
                    var empresaId = Environment.GetEnvironmentVariable("EMPRESA_LEGADO_AC_ID");
                    if (empresaId != null && int.TryParse(empresaId, out int empresaIdValue) && empresaIdValue > 0)
                    {

                        var cnpj = (await _repository.FindByHql<TipoDocumentoPessoa>($"From TipoDocumentoPessoa tdp Where tdp.TipoPessoa = {(int)EnumTiposPessoa.PessoaJuridica} and Lower(tdp.Nome) like '%cnpj%'", session)).FirstOrDefault();
                        if (cnpj == null)
                            throw new FileNotFoundException("Não foi encontrado o tipo de documento CNPJ para cadastrado no sisetma");

                        var empresa = await _communicationProvider.GetEmpresaVinculadaLegado_Esol(int.Parse(empresaId));
                        if (empresa != null && !string.IsNullOrEmpty(empresa.Cnpj))
                        {
                            Domain.Entities.Core.DadosPessoa.Pessoa pessoaSw = null;
                            var pessoaDocumentoSw = (await _repository.FindByHql<Domain.Entities.Core.DadosPessoa.PessoaDocumento>($"From PessoaDocumento pd Inner Join Fetch pd.Pessoa p Where pd.ValorNumerico like '%{empresa.Cnpj.TrimEnd()}%' ", session)).FirstOrDefault();
                            if (pessoaDocumentoSw == null)
                            {
                                pessoaSw = new Domain.Entities.Core.DadosPessoa.Pessoa()
                                {
                                    Nome = empresa.Nome,
                                    NomeFantasia = empresa.NomeFantasia,
                                    EmailPreferencial = empresa.Email,
                                    TipoPessoa = EnumTipoPessoa.Juridica,
                                    RegimeTributario = EnumTipoTributacao.LucroReal
                                };
                                await _repository.ForcedSave(pessoaSw, session);

                                pessoaDocumentoSw = new Domain.Entities.Core.DadosPessoa.PessoaDocumento()
                                {
                                    TipoDocumento = cnpj,
                                    Numero = empresa.Cnpj.TrimEnd(),
                                    ValorNumerico = Helper.ApenasNumeros(empresa.Cnpj),
                                    NumeroFormatado = Helper.Formatar(empresa.Cnpj.PadLeft(14, '0'), cnpj.Mascara ?? "##.###.###/####-##"),
                                    Pessoa = pessoaSw
                                };

                                await _repository.ForcedSave(pessoaDocumentoSw, session);

                            }
                            else
                            {
                                pessoaSw = pessoaDocumentoSw.Pessoa!;
                            }

                            var empresaSw = (await _repository.FindByHql<SW_PortalProprietario.Domain.Entities.Core.Framework.Empresa>($"From Empresa emp Inner Join Fetch emp.Pessoa p Where p.Id = {pessoaSw.Id}", session)).FirstOrDefault();
                            if (empresaSw == null)
                            {
                                empresaSw = new Domain.Entities.Core.Framework.Empresa()
                                {
                                    Pessoa = pessoaSw,
                                    Codigo = empresa.Codigo
                                };

                                await _repository.ForcedSave(empresaSw, session);

                            }

                        }

                    }

                    var resultCommit = await _repository.CommitAsync(session);
                    if (resultCommit.exception != null)
                        throw resultCommit.exception;
                }
                catch (Exception err)
                {
                    _repository?.Rollback(session);
                    _logger.LogError(err, err.Message);
                }
            }

        }

        private async Task ImportarCidades(IRepositoryHosted _repository, NHibernate.IStatelessSession? session)
        {
            string url = "https://servicodados.ibge.gov.br/api/v1/localidades/distritos?orderBy=nome";
            using HttpClient client = new();
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                var cidadesBrasileiras = JsonSerializer.Deserialize<List<CidadeImportacaoModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                var municipios = cidadesBrasileiras != null && cidadesBrasileiras.Any() ? cidadesBrasileiras.Select(b => b.municipio).DistinctBy(a=> a?.id).AsList() : new List<MunicipioModel?>();

                cidadesBrasileiras = null;

                List<string> listCidadesUnicas = new List<string>();

                if (municipios != null)
                {
                    try
                    {
                        _repository.BeginTransaction(session);

                        var brasil = (await _repository.FindByHql<Domain.Entities.Core.Geral.Pais>("From Pais p Where Lower(p.Nome) = 'brasil'", session)).FirstOrDefault();
                        if (brasil == null)
                        {
                            brasil = new Domain.Entities.Core.Geral.Pais()
                            {
                                CodigoIbge = "02",
                                Nome = "Brasil",
                                Ddi = "55",
                                MascaraTelefoneCelular = "(##) ##### ####",
                                MascaraTelefoneFixo = "(##) #### ####",
                                UsuarioCriacao = 1,
                                DataHoraCriacao = DateTime.Now
                            };

                            await _repository.ForcedSave(brasil, session);

                        }

                        foreach (var groupEstado in municipios.Where(a => a.microrregiao != null &&
                        a.id.GetValueOrDefault(0) > 0 &&
                        $"{a.id}".Length == 7 &&
                        a.microrregiao != null &&
                        a.microrregiao.mesorregiao != null && a.microrregiao.mesorregiao.uf != null)
                            .GroupBy(a => a?.microrregiao?.mesorregiao?.uf?.id))
                        {

                            Domain.Entities.Core.Geral.Estado? estadoAtual = null;
                            var fst = groupEstado.First();
                            if (fst != null)
                            {
                                var nomeEstado = fst.microrregiao?.mesorregiao?.uf?.nome;
                                var nomeNormalizado = SW_PortalProprietario.Domain.Functions.Helper.RemoveAccentsFromDomain(nomeEstado!);
                                if (!string.IsNullOrEmpty(nomeEstado) && !string.IsNullOrEmpty(nomeNormalizado))
                                {

                                    estadoAtual = (await _repository.FindByHql<Domain.Entities.Core.Geral.Estado>($@"From 
                                                                                Estado e 
                                                                            Where 
                                                                                (
                                                                                    Lower(e.Sigla) = '{fst.microrregiao?.mesorregiao?.uf?.sigla?.ToLower()}' or 
                                                                                    (
                                                                                        e.CodigoIbge = '{fst.microrregiao?.mesorregiao?.uf?.id}' or 
                                                                                        Lower(e.Nome) = '{nomeNormalizado.ToLower().Replace("'", "")}' or
                                                                                        Lower(e.Nome) = '{nomeEstado!.ToLower().Replace("'", "")}'
                                                                                    )
                                                                                ) ", session)
                                                                                    ).FirstOrDefault();

                                    if (estadoAtual == null)
                                    {
                                        estadoAtual = new Domain.Entities.Core.Geral.Estado()
                                        {
                                            Pais = brasil,
                                            Sigla = fst.microrregiao?.mesorregiao?.uf?.sigla!.Replace("'", ""),
                                            CodigoIbge = $"{fst.microrregiao?.mesorregiao?.uf?.id}",
                                            Nome = $"{fst.microrregiao?.mesorregiao?.uf?.nome!.Replace("'", "")}",
                                            UsuarioCriacao = 1,
                                            DataHoraCriacao = DateTime.Now
                                        };
                                        await _repository.ForcedSave(estadoAtual, session);
                                    }
                                }
                            }

                            if (estadoAtual != null)
                            {
                                foreach (var cidadeImportacao in groupEstado)
                                {

                                    Domain.Entities.Core.Geral.Cidade? cidade = null;
                                    var nomeCidade = cidadeImportacao?.nome!.Replace("'", "");
                                    var nomeCidadeNormalizado = SW_PortalProprietario.Domain.Functions.Helper.RemoveAccentsFromDomain(nomeCidade!).Replace("'", "");


                                    if (listCidadesUnicas.Any(b => b.Equals($"{nomeCidadeNormalizado.ToLower()}/{estadoAtual?.Sigla?.ToLower()}")))
                                        continue;

                                    listCidadesUnicas.Add($"{nomeCidadeNormalizado.ToLower()}/{estadoAtual?.Sigla?.ToLower()}");


                                    if (!string.IsNullOrEmpty(nomeCidade) && !string.IsNullOrEmpty(nomeCidadeNormalizado))
                                    {

                                        cidade = (await _repository.FindByHql<Domain.Entities.Core.Geral.Cidade>($@"From 
                                                                                Cidade c
                                                                                Inner Join Fetch c.Estado e
                                                                                Inner Join Fetch e.Pais p
                                                                            Where 
                                                                                (e.Id = {estadoAtual?.Id} or e.CodigoIbge = '{estadoAtual?.CodigoIbge}')
                                                                                and (
                                                                                       Lower(c.Nome) = '{cidadeImportacao?.nome?.ToLower().Replace("'", "")}' or 
                                                                                       c.CodigoIbge = '{cidadeImportacao?.id.GetValueOrDefault()}' or 
                                                                                       Lower(c.Nome) = '{nomeCidade.ToLower().Replace("'", "")}' or 
                                                                                       Lower(c.Nome) = '{nomeCidadeNormalizado.Replace("'", "")}'
                                                                                    ) ", session)
                                                                                    ).FirstOrDefault();

                                        if (cidade == null)
                                        {
                                            cidade = new Domain.Entities.Core.Geral.Cidade()
                                            {
                                                Estado = estadoAtual,
                                                Nome = $"{cidadeImportacao?.nome!.Replace("'", "")}",
                                                CodigoIbge = $"{cidadeImportacao?.id}",
                                                UsuarioCriacao = 1,
                                                DataHoraCriacao = DateTime.Now,
                                            };
                                            await cidade.SaveValidate();
                                            await _repository.ForcedSave(cidade, session);
                                        }
                                    }
                                }
                            }
                        }

                        var commitResult = await _repository.CommitAsync(session);
                        if (!commitResult.executed)
                            throw commitResult.exception ?? new Exception("Não foi possível importar as cidades");
                    }
                    catch (Exception err)
                    {
                        _logger.LogError(err, err.Message);
                        _repository.Rollback(session);
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
        }

        private async Task UpdatePermissions(IRepositoryHosted _repository, NHibernate.IStatelessSession? session)
        {

            try
            {
                _repository.BeginTransaction(session);
                var Path = _configuration.GetValue<string>("Path");
                var resultFromFile = FileUtils.ImportarEstruturaDeArquivo(@"Permissoes.xml", Path ?? _PATH, ParseXmltoPermissions);

                var listFromDataBase = await _repository.FindByHql<Permissao>("From Permissao p", session);

                foreach (Permissao itemFromFile in resultFromFile)
                {
                    bool alreadyExists = false;
                    foreach (Permissao itemDataBase in listFromDataBase)
                    {
                        if (itemDataBase.Id == itemFromFile.Id)
                        {
                            if (itemDataBase.Nome != itemFromFile.Nome ||
                                itemDataBase.NomeInterno != itemFromFile.NomeInterno ||
                                itemDataBase.UsarNomeInterno != itemFromFile.UsarNomeInterno ||
                                itemDataBase.TipoPermissao != itemFromFile.TipoPermissao)
                            {
                                itemDataBase.Nome = itemFromFile.Nome;
                                itemDataBase.NomeInterno = itemFromFile.NomeInterno;
                                itemDataBase.UsarNomeInterno = itemFromFile.UsarNomeInterno;
                                itemDataBase.TipoPermissao = itemFromFile.TipoPermissao;
                                await _repository.ForcedSave(itemDataBase, session);
                            }
                            alreadyExists = true;
                        }
                    }
                    if (!alreadyExists)
                    {
                        itemFromFile.DataHoraCriacao = DateTime.Now;
                        await _repository.Insert(itemFromFile, session);
                    }
                }
                var resultCommit = await _repository.CommitAsync(session);
                if (resultCommit.exception != null)
                    throw resultCommit.exception;

                await Task.CompletedTask;
            }
            catch (Exception err)
            {
                _repository.Rollback(session);
                _logger.LogError(err, err.Message);
            }
        }

        private async Task UpdateAreasSistema(IRepositoryHosted _repository, NHibernate.IStatelessSession? session)
        {
            try
            {
                _repository.BeginTransaction(session);
                var Path = _configuration.GetValue<string>("Path");
                var resultFromFile = FileUtils.ImportarEstruturaDeArquivo(@"AreasSistema.xml", Path ?? _PATH, ParseXmltoAreaSistema);

                var listFromDataBase = await _repository.FindByHql<AreaSistema>("From AreaSistema a", session);

                foreach (AreaSistema itemFromFile in resultFromFile)
                {
                    bool alreadyAdded = false;
                    foreach (AreaSistema itemDataBase in listFromDataBase)
                    {
                        if (itemDataBase.Id == itemFromFile.Id)
                        {
                            if (itemDataBase.Nome != itemFromFile.Nome)
                            {
                                itemDataBase.Nome = itemFromFile.Nome;
                                await _repository.ForcedSave(itemDataBase, session);
                            }
                            alreadyAdded = true;

                        }
                    }
                    if (!alreadyAdded)
                    {
                        itemFromFile.DataHoraCriacao = DateTime.Now;
                        itemFromFile.Status = EnumStatus.Ativo;
                        await _repository.ForcedSave(itemFromFile, session);
                    }

                }
                var resultCommit = await _repository.CommitAsync(session);
                if (resultCommit.exception != null)
                    throw resultCommit.exception;

                await Task.CompletedTask;
            }
            catch (Exception err)
            {
                _logger.LogError(err, err.Message);
                _repository.Rollback(session);
            }
        }

        private async Task UpdateGrupoModulos(IRepositoryHosted _repository, NHibernate.IStatelessSession? session)
        {
            try
            {
                _repository.BeginTransaction(session);
                var Path = _configuration.GetValue<string>("Path");
                var resultFromFile = FileUtils.ImportarEstruturaDeArquivo(@"GrupoModulos.xml", Path ?? _PATH, ParseXmltoGrupoModulo);

                var listFromDataBase = await _repository.FindByHql<GrupoModulo>("From GrupoModulo gm", session);

                foreach (GrupoModulo itemFromFile in resultFromFile)
                {
                    bool alreadyAdded = false;
                    foreach (GrupoModulo itemDataBase in listFromDataBase)
                    {
                        if (itemDataBase.Id == itemFromFile.Id)
                        {
                            if (itemDataBase.Nome != itemFromFile.Nome)
                            {
                                itemDataBase.Nome = itemFromFile.Nome;
                                await _repository.ForcedSave(itemDataBase, session);
                            }
                            alreadyAdded = true;

                        }
                    }
                    if (!alreadyAdded)
                    {
                        itemFromFile.DataHoraCriacao = DateTime.Now;
                        itemFromFile.Status = EnumStatus.Ativo;
                        await _repository.ForcedSave(itemFromFile, session);
                    }

                }
                var resultCommit = await _repository.CommitAsync(session);
                if (resultCommit.exception != null)
                    throw resultCommit.exception;

                await Task.CompletedTask;
            }
            catch (Exception err)
            {
                _logger.LogError(err, err.Message);
                _repository.Rollback(session);
            }
        }

        private async Task UpdateModules(IRepositoryHosted _repository, NHibernate.IStatelessSession? session)
        {
            try
            {
                _repository.BeginTransaction(session);
                var Path = _configuration.GetValue<string>("Path");
                var resultFromFile = FileUtils.ImportarEstruturaDeArquivo(@"Modulos.xml", Path ?? _PATH, ParseXmltoModules);
                var listPermissions = await _repository.FindByHql<Permissao>("From Permissao p", session);
                var listModulesPermissions = await _repository.FindByHql<ModuloPermissao>("From ModuloPermissao mp Inner join Fetch mp.Modulo m", session);
                var listFromDataBase = await _repository.FindByHql<Modulo>("From Modulo m Left Join Fetch m.GrupoModulo gm Left Join Fetch m.AreaSistema a", session);

                foreach (Modulo itemFromFile in resultFromFile)
                {
                    bool alreadyAdded = false;
                    foreach (Modulo itemDataBase in listFromDataBase)
                    {
                        if (itemDataBase.Id == itemFromFile.Id)
                        {
                            if (itemDataBase.Nome != itemFromFile.Nome ||
                                itemDataBase.NomeInterno != itemFromFile.NomeInterno ||
                                itemDataBase.Codigo != itemFromFile.Codigo)
                            {
                                itemDataBase.Codigo = itemFromFile.Codigo;
                                itemDataBase.Nome = itemFromFile.Nome;
                                itemDataBase.NomeInterno = itemFromFile.NomeInterno;
                                itemDataBase.AreaSistema = itemFromFile.AreaSistema;
                                itemDataBase.GrupoModulo = itemFromFile.GrupoModulo;

                                await _repository.ForcedSave(itemDataBase, session);
                            }
                            alreadyAdded = true;

                        }
                    }
                    if (!alreadyAdded)
                    {
                        itemFromFile.DataHoraCriacao = DateTime.Now;
                        itemFromFile.Status = EnumStatus.Ativo;
                        await _repository.ForcedSave(itemFromFile, session);
                    }

                    if (itemFromFile.Permissoes.Any())
                    {
                        var permissionsAdded = listModulesPermissions.Where(a => a.Modulo?.Id == itemFromFile.Id).ToList();
                        var permissionsItenToAdd = itemFromFile.Permissoes.Where(c => !permissionsAdded.Any(a => a.Permissao != null && a.Permissao.Id == c)).ToList();
                        foreach (var item in permissionsItenToAdd)
                        {
                            var modulePermission = new ModuloPermissao()
                            {
                                Modulo = new Modulo() { Id = itemFromFile.Id },
                                Permissao = new Permissao() { Id = item }
                            };
                            modulePermission.DataHoraCriacao = DateTime.Now;
                            await _repository.ForcedSave(modulePermission, session);
                        }

                    }
                }
                var resultCommit = await _repository.CommitAsync(session);
                if (resultCommit.exception != null)
                    throw resultCommit.exception;

                await Task.CompletedTask;
            }
            catch (Exception err)
            {
                _logger.LogError(err, err.Message);
                _repository.Rollback(session);
            }
        }

        private Permissao ParseXmltoPermissions(XmlElement xeElement)
        {
            Permissao permissao = new Permissao();
            permissao.Id = Convert.ToInt32(xeElement.GetAttribute("Id"));
            permissao.Nome = xeElement.GetAttribute("NomeNormalizado");
            permissao.NomeInterno = xeElement.GetAttribute("NomeInterno");
            permissao.UsarNomeInterno = Convert.ToBoolean(xeElement.GetAttribute("UsarNomeInterno")) ? EnumSimNao.Sim : EnumSimNao.Não;
            permissao.TipoPermissao = xeElement.GetAttribute("TipoPermissao");
            return permissao;
        }

        private AreaSistema ParseXmltoAreaSistema(XmlElement xeElement)
        {
            AreaSistema areaSistema = new AreaSistema();
            areaSistema.Id = Convert.ToInt32(xeElement.GetAttribute("Id"));
            areaSistema.Nome = xeElement.GetAttribute("Nome");
            return areaSistema;
        }

        private GrupoModulo ParseXmltoGrupoModulo(XmlElement xeElement)
        {
            GrupoModulo grupoModulo = new GrupoModulo();
            grupoModulo.Id = Convert.ToInt32(xeElement.GetAttribute("Id"));
            grupoModulo.Nome = xeElement.GetAttribute("Nome");
            return grupoModulo;
        }

        private Modulo ParseXmltoModules(XmlElement xeElement)
        {
            Modulo modulo = new Modulo();
            modulo.Id = Convert.ToInt32(xeElement.GetAttribute("Id"));
            modulo.Codigo = xeElement.GetAttribute("Id").PadLeft(3, '0');
            modulo.Nome = xeElement.GetAttribute("NomeNormalizado");
            modulo.NomeInterno = xeElement.GetAttribute("NomeInterno");
            modulo.AreaSistema = new AreaSistema() { Id = Convert.ToInt32(xeElement.GetAttribute("AreaSistema")) };
            modulo.GrupoModulo = new GrupoModulo() { Id = Convert.ToInt32(xeElement.GetAttribute("GrupoModulo")) };
            var permissionsIncluded = xeElement.GetAttribute("Permissoes");
            if (!string.IsNullOrEmpty(permissionsIncluded))
            {
                modulo.Permissoes = permissionsIncluded.Split(',').Select(c => Convert.ToInt32(c)).ToList();
            }
            return modulo;
        }
    }
}
