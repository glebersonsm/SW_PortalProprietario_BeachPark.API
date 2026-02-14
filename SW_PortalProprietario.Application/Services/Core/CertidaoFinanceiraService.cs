using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Models.UsuarioFinanceiro;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Certidao;
using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;
using SW_PortalProprietario.Domain.Entities.Core.Framework;
using SW_PortalProprietario.Domain.Functions;
using SW_Utils.Auxiliar;
using SW_Utils.Historicos;


namespace SW_PortalProprietario.Application.Services.Core
{
    public class CertidaoFinanceiraService : ICertidaoFinanceiraService
    {
        private readonly IRepositoryNH _repository;
        private readonly ILogger<CertidaoFinanceiraService> _logger;
        private readonly IProjectObjectMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IServiceBase _serviceBase;
        private readonly IFinanceiroHybridProviderService _financeiroProviderService;
        private readonly IEmpreendimentoHybridProviderService _empreendimentoProviderService;
        private readonly IHistoricosCertidoes _historicosCertidoes;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CertidaoFinanceiraService(IRepositoryNH repository,
            ILogger<CertidaoFinanceiraService> logger,
            IProjectObjectMapper mapper,
            IConfiguration configuration,
            IServiceBase serviceBase,
            IFinanceiroHybridProviderService financeiroProviderService,
            IEmpreendimentoHybridProviderService empreendimentoProviderService,
            IHistoricosCertidoes historicosCertidoes,
            IUserService userService,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _configuration = configuration;
            _serviceBase = serviceBase;
            _financeiroProviderService = financeiroProviderService;
            _historicosCertidoes = historicosCertidoes;
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
            _empreendimentoProviderService = empreendimentoProviderService;
        }

        public async Task<List<FileResultModel>> GerarCertidaoNegativaPositivaDeDebitosFinanceiros(GeracaoCertidaoInputModel geracaoCertidaoInputModel)
        {
            throw new NotImplementedException();
            //var httpContext = _httpContextAccessor?.HttpContext?.Request;
            //if (httpContext is null)
            //    throw new Exception("Não foi possível identificar a URL do servidor");

            //if (geracaoCertidaoInputModel.Data.GetValueOrDefault(DateTime.MinValue) == DateTime.MinValue)
            //    geracaoCertidaoInputModel.Data = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);


            //List<FileResultModel> listRetorno = new List<FileResultModel>();

            //try
            //{
            //    _repository.BeginTransaction();

            //    var pathValidacaoProtocoloBase = $"{_configuration.GetValue<string>($"CertidoesConfig:PathValidacaoProtocolo")}";

            //    var loggedUser = await _repository.GetLoggedUser();
            //    if (loggedUser == null)
            //        throw new ArgumentException("Não foi possível identificar o usuário logado no sistema");

            //    var pessoaVinculadaSistema = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(Convert.ToInt32(loggedUser.Value.userId), _financeiroProviderService.ProviderName);
            //    if (pessoaVinculadaSistema == null)
            //        throw new ArgumentException($"Não foi encontrada pessoa do provider: {_financeiroProviderService.ProviderName} vinculada ao usuário logado: {loggedUser.Value.userId}");

            //    if (string.IsNullOrEmpty(pessoaVinculadaSistema.PessoaProvider) || !Helper.IsNumeric(pessoaVinculadaSistema.PessoaProvider))
            //        throw new ArgumentException($"Não foi encontrada pessoa do provider: {_financeiroProviderService.ProviderName} vinculada ao usuário logado: {loggedUser.Value.userId}");

            //    var proprietario = await _empreendimentoProviderService.GetProprietarios(new Models.Empreendimento.SearchProprietarioModel() { PessoaProviderId = Convert.ToInt32(pessoaVinculadaSistema.PessoaProvider) });
            //    if (proprietario == null || proprietario.Value.proprietarios == null || !proprietario.Value.proprietarios.Any())
            //        throw new ArgumentException($"Não foi encontrada nenhuma cota vinculada a pessoa id: {pessoaVinculadaSistema.PessoaProvider} do provider: {_financeiroProviderService.ProviderName}");

            //    if (!string.IsNullOrEmpty(pessoaVinculadaSistema.PessoaProvider))
            //    {
            //        var propCache = await _serviceBase.GetContratos(new List<int>() { int.Parse(pessoaVinculadaSistema.PessoaProvider!) });
            //        if (propCache != null && propCache.Any(b=>b.frAtendimentoStatusCrcModels.Any(b=> b.BloquearCobrancaPagRec == "S" || b.BloqueaRemissaoBoletos == "S")))
            //        {
            //            throw new ArgumentException("Não foi possível gerar a certidão, motivo 0001BL");
            //        }
            //    }

            //    var usuarios = await _userService.SearchNotPaginated(new UsuarioSearchModel() { CarregarDadosPessoa = true, Id = Convert.ToInt32(loggedUser.Value.userId) });
            //    if (usuarios == null || !usuarios.Any())
            //        throw new ArgumentException($"Não foi encontrado usuário com Id: {loggedUser.Value.userId}");

            //    var usuario = usuarios.First();

            //    var contaspendentes = await _financeiroProviderService.GetContaPendenteDoUsuario(new SearchContasPendentesUsuarioLogado()
            //    {
            //        VencimentoInicial = DateTime.Today.AddYears(-100).Date,
            //        VencimentoFinal = geracaoCertidaoInputModel.Data.GetValueOrDefault(DateTime.Today).Date,
            //        NumeroDaPagina = 1,
            //        QuantidadeRegistrosRetornar = 2000
            //    });

            //    var certidaoPositivaModeloPorUnidadeNome = _configuration.GetValue<string>($"CertidoesConfig:PositivaConfigPorUnidade");
            //    var certidaoNegativaModeloPorUnidadeNome = _configuration.GetValue<string>($"CertidoesConfig:NegativaConfigPorUnidade");
            //    var certidaoPositivaModeloPorClienteNome = _configuration.GetValue<string>($"CertidoesConfig:PositivaConfigPorCliente");
            //    var certidaoNegativaModeloPorClienteNome = _configuration.GetValue<string>($"CertidoesConfig:NegativaConfigPorCliente");

            //    var agruparCertidoesPorCliente = _configuration.GetValue<bool>("CertidoesConfig:AgruparCertidaoPorCliente", true);

            //    var configuration = await _serviceBase.GetParametroSistema();
            //    if (configuration != null)
            //        agruparCertidoesPorCliente = configuration.AgruparCertidaoPorCliente.GetValueOrDefault(Domain.Enumns.EnumSimNao.Não) == Domain.Enumns.EnumSimNao.Sim;

            //    var certidoesModeloPath = _configuration.GetValue<string>($"CertidoesConfig:PositivaConfig", "C:\\inetpub\\wwwroot\\ModeloCertidoes\\");
            //    var pathGeracaoPdf = _configuration.GetValue<string>($"CertidoesConfig:PositivaConfig", "C:\\inetpub\\wwwroot\\CertidoesFinanceiras\\");

            //    if (!agruparCertidoesPorCliente)
            //    {
            //        if (string.IsNullOrEmpty(certidaoPositivaModeloPorUnidadeNome) || !certidaoPositivaModeloPorUnidadeNome.Contains("|"))
            //            throw new FileNotFoundException($"Não foi encontrada a configuração de modelo para geração de certidão positiva de débitos");

            //        if (string.IsNullOrEmpty(certidaoNegativaModeloPorUnidadeNome) || !certidaoNegativaModeloPorUnidadeNome.Contains("|"))
            //            throw new FileNotFoundException($"Não foi encontrada a configuração de modelo para geração de certidão negativa de débitos");
            //    }
            //    else
            //    {
            //        if (string.IsNullOrEmpty(certidaoPositivaModeloPorClienteNome) || !certidaoPositivaModeloPorClienteNome.Contains("|"))
            //            throw new FileNotFoundException($"Não foi encontrada a configuração de modelo para geração de certidão positiva de débitos");

            //        if (string.IsNullOrEmpty(certidaoNegativaModeloPorClienteNome) || !certidaoNegativaModeloPorClienteNome.Contains("|"))
            //            throw new FileNotFoundException($"Não foi encontrada a configuração de modelo para geração de certidão negativa de débitos");
            //    }

            //    if (!Directory.Exists(certidoesModeloPath))
            //        throw new FileNotFoundException($"Não foi encontrado o repositório de modelos para emissões de certidões");

            //    if (!Directory.Exists(pathGeracaoPdf))
            //        throw new ArgumentException($"Não foi encontrado o repositório de gravação temporária das certidões");

            //    string nomeCertidaoPositiva = !agruparCertidoesPorCliente ? certidaoPositivaModeloPorUnidadeNome!.Split("|")[0] : certidaoPositivaModeloPorClienteNome!.Split("|")[0];
            //    if (string.IsNullOrEmpty(nomeCertidaoPositiva))
            //        throw new FileNotFoundException("Não foi encontrado o modelo de certidão para emissão de certidão positiva");

            //    string nomeCertidaoNegativa = !agruparCertidoesPorCliente ? certidaoNegativaModeloPorUnidadeNome!.Split("|")[0] : certidaoNegativaModeloPorClienteNome!.Split("|")[0];
            //    if (string.IsNullOrEmpty(nomeCertidaoPositiva))
            //        throw new FileNotFoundException("Não foi encontrado o modelo de certidão para emissão de certidão negativa");

            //    string funcaoSubstituicoesEmissaoPositivas = !agruparCertidoesPorCliente ? certidaoPositivaModeloPorUnidadeNome!.Split("|")[1] : certidaoPositivaModeloPorClienteNome!.Split("|")[1];

            //    if (string.IsNullOrEmpty(funcaoSubstituicoesEmissaoPositivas))
            //        throw new FileNotFoundException("Não foi encontrada a função para emissão de certidão positiva");

            //    string funcaoSubstituicoesEmissaoNegativa = !agruparCertidoesPorCliente ? certidaoNegativaModeloPorUnidadeNome!.Split("|")[1] : certidaoNegativaModeloPorClienteNome!.Split("|")[1];

            //    if (string.IsNullOrEmpty(funcaoSubstituicoesEmissaoNegativa))
            //        throw new FileNotFoundException("Não foi encontrada a função para emissão de certidão negativa");

            //    if (contaspendentes.Value.contasPendentes.Any())
            //    {
            //        if (!File.Exists(Path.Combine(certidoesModeloPath, nomeCertidaoPositiva)))
            //            throw new ArgumentException($"Não foi encontrado o documento modelo para emissão de certidão positiva de débitos: '{Path.Combine(certidoesModeloPath, nomeCertidaoPositiva)}' ");
            //    }
            //    else
            //    {
            //        if (!File.Exists(Path.Combine(certidoesModeloPath, nomeCertidaoNegativa)))
            //            throw new ArgumentException($"Não foi encontrado o documento modelo para emissão de certidão negativa de débitos: '{Path.Combine(certidoesModeloPath, nomeCertidaoNegativa)}' ");
            //    }


            //    if (!Directory.Exists(pathGeracaoPdf))
            //        Directory.CreateDirectory(pathGeracaoPdf);

            //    var launchOptions = new LaunchOptions
            //    {
            //        Headless = true
            //    };

            //    List<CertidaoFinanceira> certidoes = new List<CertidaoFinanceira>();


            //    if (!agruparCertidoesPorCliente)
            //    {
            //        if (contaspendentes.Value.contasPendentes.Any())
            //            certidoes.AddRange(await PrepararEmitirCertidoesComPendencias(geracaoCertidaoInputModel, usuario, contaspendentes.Value.contasPendentes, certidoesModeloPath, pathGeracaoPdf, nomeCertidaoPositiva, funcaoSubstituicoesEmissaoPositivas, launchOptions, pathValidacaoProtocoloBase));

            //        foreach (var itemProprietario in proprietario.Value.proprietarios)
            //        {
            //            var existeCertidaoComPendenciaFinanceira = certidoes.Any(b =>
            //            !string.IsNullOrEmpty(b.NumeroFracao) &&
            //            !string.IsNullOrEmpty(itemProprietario.CodigoFracao) &&
            //            !string.IsNullOrEmpty(b.ImovelNumero) &&
            //            !string.IsNullOrEmpty(itemProprietario.ImovelNumero) &&
            //            b.NumeroFracao.Equals(itemProprietario.CodigoFracao, StringComparison.CurrentCultureIgnoreCase) &&
            //            b.ImovelNumero.Equals(itemProprietario.ImovelNumero, StringComparison.InvariantCultureIgnoreCase));

            //            if (!existeCertidaoComPendenciaFinanceira)
            //                certidoes.Add(await PrepararEmitirCertidoesNegativasDeDebito(geracaoCertidaoInputModel, usuario, itemProprietario, certidoesModeloPath, pathGeracaoPdf, nomeCertidaoNegativa, funcaoSubstituicoesEmissaoNegativa, launchOptions, pathValidacaoProtocoloBase));

            //        }

            //        foreach (var certidao in certidoes)
            //        {
            //            listRetorno.Add(new FileResultModel()
            //            {
            //                Id = certidao.Id,
            //                DataHoraCriacao = certidao.DataHoraCriacao,
            //                UsuarioCriacao = certidao.UsuarioCriacao,
            //                Path = certidao.PdfPath,
            //                FileName = Path.GetFileName(certidao.PdfPath)

            //            });
            //        }
            //    }
            //    else
            //    {
            //        if (contaspendentes.Value.contasPendentes.Any())
            //            certidoes.AddRange(await PrepararEmitirCertidoesComPendenciasAgrupadoPorCliente(geracaoCertidaoInputModel, usuario, contaspendentes.Value.contasPendentes, certidoesModeloPath, pathGeracaoPdf, nomeCertidaoPositiva, funcaoSubstituicoesEmissaoPositivas, launchOptions, pathValidacaoProtocoloBase));

            //        foreach (var itemProprietario in proprietario.Value.proprietarios)
            //        {
            //            var existeCertidaoComPendenciaFinanceira = certidoes.Any();

            //            if (!existeCertidaoComPendenciaFinanceira)
            //                certidoes.Add(await PrepararEmitirCertidoesNegativasDeDebitoAgrupadoPorCliente(geracaoCertidaoInputModel, usuario, itemProprietario, certidoesModeloPath, pathGeracaoPdf, nomeCertidaoNegativa, funcaoSubstituicoesEmissaoNegativa, launchOptions, pathValidacaoProtocoloBase));

            //        }

            //        foreach (var certidao in certidoes)
            //        {
            //            listRetorno.Add(new FileResultModel()
            //            {
            //                Id = certidao.Id,
            //                DataHoraCriacao = certidao.DataHoraCriacao,
            //                UsuarioCriacao = certidao.UsuarioCriacao,
            //                Path = certidao.PdfPath,
            //                FileName = Path.GetFileName(certidao.PdfPath)

            //            });
            //        }
            //    }

            //    var commitResult = await _repository.CommitAsync();
            //    if (!commitResult.executed)
            //        throw commitResult.exception ?? new Exception("Não foi possível realizar a operação");

            //    return listRetorno;

            //}
            //catch (Exception err)
            //{
            //    _logger.LogError(err, $"Não foi possível gerar a certidão financeira para o usuário logado");
            //    _repository.Rollback();
            //    throw;
            //}
        }

        private async Task<List<CertidaoFinanceira>> PrepararEmitirCertidoesComPendencias(GeracaoCertidaoInputModel geracaoCertidaoInputModel, UsuarioModel usuario, List<ContaPendenteModel> contaspendentes, string? certidoesModeloPath, string? pathGeracaoPdf, string nomeCertidao, string funcaoSubstituicoes, LaunchOptions launchOptions, string pathValidacaoProtocoloBase)
        {
            var certidoes = new List<CertidaoFinanceira>();

            foreach (var itemGrouped in contaspendentes.GroupBy(a => new
            {
                a.PessoaProviderId
            }))
            {
                var itens = itemGrouped.AsList();
                //Gero a certidão
                CertidaoFinanceira certidao = new CertidaoFinanceira()
                {
                    Pessoa = new Domain.Entities.Core.DadosPessoa.Pessoa() { Id = usuario.PessoaId.GetValueOrDefault() },
                    Tipo = contaspendentes.Any() ? Domain.Enumns.EnumCertidaoTipo.PositivaDeDebitos : Domain.Enumns.EnumCertidaoTipo.NegativaDeDebitos,
                    CompetenciaInicial = DateTime.Today.AddYears(-100).Date,
                    CompetenciaFinal = geracaoCertidaoInputModel.Data.GetValueOrDefault(DateTime.Today).Date,
                    NumeroFracao = itens.Any(b => !string.IsNullOrEmpty(b.FracaoCota)) ? string.Join(",", itens.Where(b => !string.IsNullOrEmpty(b.FracaoCota)).ToList()) : "",
                    ImovelNumero = itens.Any(b => !string.IsNullOrEmpty(b.NumeroImovel)) ? string.Join(",", itens.Where(b => !string.IsNullOrEmpty(b.NumeroImovel)).ToList()) : "",
                    Competencia = $"{geracaoCertidaoInputModel.Data.GetValueOrDefault(DateTime.Today).Date:MMMM}, {geracaoCertidaoInputModel.Data.GetValueOrDefault(DateTime.Today).Date:yyyy}"
                };

                await _repository.Save(certidao);
                certidao.Protocolo = $"{Guid.NewGuid().ToString().Replace("-", "").Replace(" ", "").Substring(0, 10)}{usuario.Pessoa.Id.ToString().PadLeft(06, '0').Substring(0, 6)}";
                certidao.Protocolo = certidao.Protocolo.ToUpper();

                var pdfDocumentPath = Path.Combine(pathGeracaoPdf, $"{certidao.Protocolo}.pdf");
                var htmlDocumentPath = Path.Combine(certidoesModeloPath, nomeCertidao);

                string htmlContent = File.ReadAllText(htmlDocumentPath);

                var substituicoes = _historicosCertidoes.GetHistoricos(funcaoSubstituicoes);

                certidao.UrlValidacaoProtocolo = $"{pathValidacaoProtocoloBase}/{certidao.Protocolo}";
                List<ParameterValueResult>? values = await PrepararSubstituicoesEmissaoPositivaDeDebitos(substituicoes, funcaoSubstituicoes, usuario, certidao, itens, new List<ParameterValueResult>() {
                new ParameterValueResult("competencia", $"{certidao.Competencia}"),
                new ParameterValueResult("numeroprotocolo", $"{certidao.Protocolo}"),
                new ParameterValueResult("enderecovalidacaodocumento", certidao.UrlValidacaoProtocolo)});

                htmlContent = await AplicarSubstituicoes(htmlContent, values);
                certidao.Conteudo = htmlContent;

                // Inicializar o PuppeteerSharp
                await new BrowserFetcher().DownloadAsync();
                using var browser = await Puppeteer.LaunchAsync(launchOptions);
                using var page = await browser.NewPageAsync();

                // Carregar o conteúdo HTML na página
                await page.SetContentAsync(htmlContent);

                // Gerar o PDF
                await page.PdfAsync(pdfDocumentPath);
                certidao.PdfPath = pdfDocumentPath;

                await _repository.Save(certidao);
                certidoes.Add(certidao);

            }
            return certidoes;
        }

        private async Task<CertidaoFinanceira> PrepararEmitirCertidoesNegativasDeDebito(GeracaoCertidaoInputModel geracaoCertidaoInputModel, UsuarioModel usuario, ProprietarioSimplificadoModel proprietario, string? certidoesModeloPath, string? pathGeracaoPdf, string nomeCertidao, string funcaoSubstituicoes, LaunchOptions launchOptions, string pathValidacaoProtocoloBase)
        {
            //Gero a certidão
            CertidaoFinanceira certidao = new CertidaoFinanceira()
            {
                Pessoa = new Domain.Entities.Core.DadosPessoa.Pessoa() { Id = usuario.PessoaId.GetValueOrDefault() },
                Tipo = Domain.Enumns.EnumCertidaoTipo.NegativaDeDebitos,
                CompetenciaInicial = DateTime.Today.AddYears(-100).Date,
                CompetenciaFinal = geracaoCertidaoInputModel.Data.GetValueOrDefault(DateTime.Today).Date,
                NumeroFracao = proprietario.CodigoFracao,
                ImovelNumero = proprietario.ImovelNumero,
                Competencia = $"{geracaoCertidaoInputModel.Data.GetValueOrDefault(DateTime.Today).Date:MMMM}, {geracaoCertidaoInputModel.Data.GetValueOrDefault(DateTime.Today).Date:yyyy}"
            };

            await _repository.Save(certidao);
            certidao.Protocolo = $"{Guid.NewGuid().ToString().Replace("-", "").Replace(" ", "").Substring(0, 10)}{usuario.Pessoa.Id.ToString().PadLeft(06, '0').Substring(0, 6)}";
            certidao.Protocolo = certidao.Protocolo.ToUpper();

            var pdfDocumentPath = Path.Combine(pathGeracaoPdf, $"{certidao.Protocolo}.pdf");
            var htmlDocumentPath = Path.Combine(certidoesModeloPath, nomeCertidao);

            string htmlContent = File.ReadAllText(htmlDocumentPath);

            var substituicoes = _historicosCertidoes.GetHistoricos(funcaoSubstituicoes);
            certidao.UrlValidacaoProtocolo = $"{pathValidacaoProtocoloBase.Replace("{protocolo}", certidao.Protocolo)}";


            List<ParameterValueResult>? values = await PrepararSubstituicoesEmissaoNegativaDeDebitos(substituicoes, funcaoSubstituicoes, usuario, certidao, proprietario, new List<ParameterValueResult>() {
                new ParameterValueResult("competencia", $"{certidao.Competencia}"),
                new ParameterValueResult("numeroprotocolo", $"{certidao.Protocolo}"),
                new ParameterValueResult("enderecovalidacaodocumento",certidao.UrlValidacaoProtocolo)});

            htmlContent = await AplicarSubstituicoes(htmlContent, values);
            certidao.Conteudo = htmlContent;

            // Inicializar o PuppeteerSharp
            await new BrowserFetcher().DownloadAsync();
            using var browser = await Puppeteer.LaunchAsync(launchOptions);
            using var page = await browser.NewPageAsync();

            // Carregar o conteúdo HTML na página
            await page.SetContentAsync(htmlContent);

            // Gerar o PDF
            await page.PdfAsync(pdfDocumentPath);
            certidao.PdfPath = pdfDocumentPath;

            await _repository.Save(certidao);

            return certidao;
        }

        private async Task<List<CertidaoFinanceira>> PrepararEmitirCertidoesComPendenciasAgrupadoPorCliente(GeracaoCertidaoInputModel geracaoCertidaoInputModel, UsuarioModel usuario, List<ContaPendenteModel> contaspendentes, string? certidoesModeloPath, string? pathGeracaoPdf, string nomeCertidao, string funcaoSubstituicoes, LaunchOptions launchOptions, string pathValidacaoProtocoloBase)
        {
            var certidoes = new List<CertidaoFinanceira>();

            foreach (var itemGrouped in contaspendentes.GroupBy(a => new
            {
                a.PessoaProviderId
            }))
            {
                var itens = itemGrouped.AsList();
                //Gero a certidão
                CertidaoFinanceira certidao = new CertidaoFinanceira()
                {
                    Pessoa = new Domain.Entities.Core.DadosPessoa.Pessoa() { Id = usuario.PessoaId.GetValueOrDefault() },
                    Tipo = contaspendentes.Any() ? Domain.Enumns.EnumCertidaoTipo.PositivaDeDebitos : Domain.Enumns.EnumCertidaoTipo.NegativaDeDebitos,
                    CompetenciaInicial = DateTime.Today.AddYears(-100).Date,
                    CompetenciaFinal = geracaoCertidaoInputModel.Data.GetValueOrDefault(DateTime.Today).Date,
                    NumeroFracao = "",
                    ImovelNumero = "",
                    Competencia = $"{geracaoCertidaoInputModel.Data.GetValueOrDefault(DateTime.Today).Date:MMMM}, {geracaoCertidaoInputModel.Data.GetValueOrDefault(DateTime.Today).Date:yyyy}"
                };

                await _repository.Save(certidao);
                certidao.Protocolo = $"{Guid.NewGuid().ToString().Replace("-", "").Replace(" ", "").Substring(0, 10)}{usuario.Pessoa.Id.ToString().PadLeft(06, '0').Substring(0, 6)}";
                certidao.Protocolo = certidao.Protocolo.ToUpper();

                var pdfDocumentPath = Path.Combine(pathGeracaoPdf, $"{certidao.Protocolo}.pdf");
                var htmlDocumentPath = Path.Combine(certidoesModeloPath, nomeCertidao);

                string htmlContent = File.ReadAllText(htmlDocumentPath);

                var substituicoes = _historicosCertidoes.GetHistoricos(funcaoSubstituicoes);

                certidao.UrlValidacaoProtocolo = $"{pathValidacaoProtocoloBase.Replace("{protocolo}", certidao.Protocolo)}";
                List<ParameterValueResult>? values = await PrepararSubstituicoesEmissaoPositivaDeDebitos(substituicoes, funcaoSubstituicoes, usuario, certidao, itens, new List<ParameterValueResult>() {
                new ParameterValueResult("competencia", $"{certidao.Competencia}"),
                new ParameterValueResult("numeroprotocolo", $"{certidao.Protocolo}"),
                new ParameterValueResult("enderecovalidacaodocumento", certidao.UrlValidacaoProtocolo)});

                htmlContent = await AplicarSubstituicoes(htmlContent, values);
                certidao.Conteudo = htmlContent;


                // Inicializar o PuppeteerSharp
                await new BrowserFetcher().DownloadAsync();
                using var browser = await Puppeteer.LaunchAsync(launchOptions);
                using var page = await browser.NewPageAsync();

                // Carregar o conteúdo HTML na página
                await page.SetContentAsync(htmlContent);

                // Gerar o PDF
                await page.PdfAsync(pdfDocumentPath);
                certidao.PdfPath = pdfDocumentPath;

                await _repository.Save(certidao);
                certidoes.Add(certidao);

            }
            return certidoes;
        }

        private async Task<CertidaoFinanceira> PrepararEmitirCertidoesNegativasDeDebitoAgrupadoPorCliente(GeracaoCertidaoInputModel geracaoCertidaoInputModel, UsuarioModel usuario, ProprietarioSimplificadoModel proprietario, string? certidoesModeloPath, string? pathGeracaoPdf, string nomeCertidao, string funcaoSubstituicoes, LaunchOptions launchOptions, string pathValidacaoProtocoloBase)
        {
            //Gero a certidão
            CertidaoFinanceira certidao = new CertidaoFinanceira()
            {
                Pessoa = new Domain.Entities.Core.DadosPessoa.Pessoa() { Id = usuario.PessoaId.GetValueOrDefault() },
                Tipo = Domain.Enumns.EnumCertidaoTipo.NegativaDeDebitos,
                CompetenciaInicial = DateTime.Today.AddYears(-100).Date,
                CompetenciaFinal = geracaoCertidaoInputModel.Data.GetValueOrDefault(DateTime.Today).Date,
                NumeroFracao = "",
                ImovelNumero = "",
                Competencia = $"{geracaoCertidaoInputModel.Data.GetValueOrDefault(DateTime.Today).Date:MMMM}, {geracaoCertidaoInputModel.Data.GetValueOrDefault(DateTime.Today).Date:yyyy}"
            };

            await _repository.Save(certidao);
            certidao.Protocolo = $"{Guid.NewGuid().ToString().Replace("-", "").Replace(" ", "").Substring(0, 10)}{usuario.Pessoa.Id.ToString().PadLeft(06, '0').Substring(0, 6)}";
            certidao.Protocolo = certidao.Protocolo.ToUpper();

            var pdfDocumentPath = Path.Combine(pathGeracaoPdf, $"{certidao.Protocolo}.pdf");
            var htmlDocumentPath = Path.Combine(certidoesModeloPath, nomeCertidao);

            string htmlContent = File.ReadAllText(htmlDocumentPath);

            var substituicoes = _historicosCertidoes.GetHistoricos(funcaoSubstituicoes);
            certidao.UrlValidacaoProtocolo = $"{pathValidacaoProtocoloBase.Replace("{protocolo}", certidao.Protocolo)}";

            List<ParameterValueResult>? values = await PrepararSubstituicoesEmissaoNegativaDeDebitos(substituicoes, funcaoSubstituicoes, usuario, certidao, proprietario, new List<ParameterValueResult>() {
                new ParameterValueResult("competencia", $"{certidao.Competencia}"),
                new ParameterValueResult("numeroprotocolo", $"{certidao.Protocolo}"),
                new ParameterValueResult("enderecovalidacaodocumento",certidao.UrlValidacaoProtocolo)});

            htmlContent = await AplicarSubstituicoes(htmlContent, values);
            certidao.Conteudo = htmlContent;

            // Inicializar o PuppeteerSharp
            await new BrowserFetcher().DownloadAsync();
            using var browser = await Puppeteer.LaunchAsync(launchOptions);
            using var page = await browser.NewPageAsync();

            // Carregar o conteúdo HTML na página
            await page.SetContentAsync(htmlContent);

            // Gerar o PDF
            await page.PdfAsync(pdfDocumentPath);
            certidao.PdfPath = pdfDocumentPath;

            await _repository.Save(certidao);

            return certidao;
        }

        private async Task<List<ParameterValueResult>?> PrepararSubstituicoesEmissaoPositivaDeDebitos(List<ParameterValueResult>? substituicoes, string funcaoSubstituicoes, UsuarioModel usuarioModel, CertidaoFinanceira certidao, List<ContaPendenteModel> itens, List<ParameterValueResult> parameterDefault = null)
        {
            var parametrosSistema = await _repository.GetParametroSistemaViewModel();
            if (parametrosSistema == null)
                throw new Exception("Deve ser configurado os parâmetros da empresa");

            var nomeSistema = _configuration.GetValue<string>("NomeDoSistema", "Portal de multipropriedade SW - Soluções");

            if (itens == null || !itens.Any())
                throw new FileNotFoundException("Deve ser enviado pelo menos uma conta pendente em itens para emissão de certidão positiva de débitos");

            var nomeProprietario = itens != null ? itens.First().NomePessoa : "";

            var empresa = (await _repository.FindByHql<Empresa>($"From Empresa e Inner Join Fetch e.Pessoa p Where e.Id = {parametrosSistema.EmpresaId}")).FirstOrDefault();
            if (empresa == null)
                throw new FileNotFoundException($"Não foi encontrada a empresa com o Id: {parametrosSistema.EmpresaId}");

            if (empresa.Pessoa == null || empresa.Pessoa.Id == 0)
                throw new FileNotFoundException($"Não foi encontrada a pessoa da empresa com o Id: {parametrosSistema.EmpresaId}");

            var enderecoEmpresa = (await _repository.FindByHql<PessoaEndereco>($"From PessoaEndereco pe Inner Join Fetch pe.Cidade cid Inner Join Fetch pe.TipoEndereco te Where pe.Pessoa = {empresa.Pessoa.Id} and Coalesce(pe.Preferencial,0) = 1")).FirstOrDefault();
            if (enderecoEmpresa == null && string.IsNullOrEmpty(empresa.EnderecoCondominio) && string.IsNullOrEmpty(empresa.EnderecoAdministradoraCondominio) && string.IsNullOrEmpty(parametrosSistema.EnderecoAdministradoraCondominio) && string.IsNullOrEmpty(parametrosSistema.EnderecoCondominio))
                throw new FileNotFoundException($"Não foi encontrado endereço preferencial para a pessoa da empresa com o Id: {parametrosSistema.EmpresaId}, nem nos campos Endereço Condomínio/Endereco Administradora Condomínio na empresa: {empresa.Id} e nem nos campos Endereço empreendimento/Endereço administradora empreendimento no cadastro de parâmetros.");

            var cnpjEmpresa = (await _repository.FindByHql<PessoaDocumento>($"From PessoaDocumento pd Inner Join Fetch pd.TipoDocumento td Where pd.Pessoa = {empresa.Pessoa.Id} and Lower(td.Nome) = 'cnpj'")).FirstOrDefault();
            if (cnpjEmpresa == null && string.IsNullOrEmpty(empresa.CnpjCondominio) && string.IsNullOrEmpty(empresa.CnpjAdministradoraCondominio) && string.IsNullOrEmpty(parametrosSistema.CnpjCondominio) && string.IsNullOrEmpty(parametrosSistema.CnpjAdministradoraCondominio))
                throw new FileNotFoundException($"Não foi encontrado o CNPJ da empresa com o Id: {parametrosSistema.EmpresaId} e nem nos campos Cnpj Condomínio/Cnpj Administradora Condomínio no cadastro da empresa: {empresa.Id} e nem nos campos Endereço administradora/Endereço administradora empreendimento no cadastro de parâmetros.");

            List<ParameterValueResult>? result = new List<ParameterValueResult>();
            if (funcaoSubstituicoes.Equals("certidaopositivadebitos", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var item in substituicoes.Where(c => !string.IsNullOrEmpty(c.Key)))
                {
                    switch (item.Key.ToLower())
                    {
                        case "cnpjadministradora":
                            {
                                if (!string.IsNullOrEmpty(parametrosSistema.CnpjAdministradoraCondominio) || !string.IsNullOrEmpty(parametrosSistema.CnpjCondominio))
                                {
                                    result.Add(new ParameterValueResult(item.Key, parametrosSistema.CnpjAdministradoraCondominio ?? parametrosSistema.CnpjCondominio));
                                    break;
                                }

                                var cnpjAdministradora = !string.IsNullOrEmpty(empresa.CnpjAdministradoraCondominio) ? empresa.CnpjAdministradoraCondominio : empresa.CnpjCondominio;
                                if (string.IsNullOrEmpty(cnpjAdministradora))
                                {
                                    cnpjAdministradora = cnpjEmpresa.NumeroFormatado;
                                }
                                result.Add(new ParameterValueResult(item.Key, cnpjAdministradora!));
                            }
                            break;
                        case "nomeadministradora":
                            {
                                if (!string.IsNullOrEmpty(parametrosSistema.NomeAdministradoraCondominio) || !string.IsNullOrEmpty(parametrosSistema.NomeCondominio))
                                {
                                    result.Add(new ParameterValueResult(item.Key, parametrosSistema.NomeAdministradoraCondominio ?? parametrosSistema.NomeCondominio));
                                    break;
                                }

                                var nomeAdminstradora = !string.IsNullOrEmpty(empresa.NomeAdministradoraCondominio) ? empresa.NomeAdministradoraCondominio : empresa.NomeCondominio;
                                if (string.IsNullOrEmpty(nomeAdminstradora))
                                {
                                    nomeAdminstradora = empresa.Pessoa.Nome;
                                }
                                result.Add(new ParameterValueResult(item.Key, nomeAdminstradora!));
                            }
                            break;
                        case "enderecoadministradora":
                            {
                                if (!string.IsNullOrEmpty(parametrosSistema.EnderecoAdministradoraCondominio) || !string.IsNullOrEmpty(parametrosSistema.EnderecoCondominio))
                                {
                                    result.Add(new ParameterValueResult(item.Key, parametrosSistema.EnderecoAdministradoraCondominio ?? parametrosSistema.EnderecoCondominio));
                                    break;
                                }

                                var enderecoAdministradora = !string.IsNullOrEmpty(empresa.EnderecoAdministradoraCondominio) ? empresa.EnderecoAdministradoraCondominio : empresa.EnderecoCondominio;
                                if (string.IsNullOrEmpty(enderecoAdministradora))
                                {
                                    if (enderecoEmpresa.Cidade != null && enderecoEmpresa.Cidade.Estado != null && enderecoEmpresa.Cidade.Estado.Sigla is not null)
                                    {
                                        enderecoAdministradora = $"{enderecoEmpresa.Logradouro}, {enderecoEmpresa.Bairro}, {enderecoEmpresa.Cidade.Nome}/{enderecoEmpresa.Cidade.Estado.Sigla}";
                                    }
                                }
                                result.Add(new ParameterValueResult(item.Key, enderecoAdministradora!));

                            }
                            break;
                        case "endereco":
                            {
                                if (!string.IsNullOrEmpty(parametrosSistema.EnderecoAdministradoraCondominio) || !string.IsNullOrEmpty(parametrosSistema.EnderecoCondominio))
                                {
                                    result.Add(new ParameterValueResult(item.Key, parametrosSistema.EnderecoAdministradoraCondominio ?? parametrosSistema.EnderecoCondominio));
                                    break;
                                }

                                var enderecoAdministradora = !string.IsNullOrEmpty(empresa.EnderecoAdministradoraCondominio) ? empresa.EnderecoAdministradoraCondominio : empresa.EnderecoCondominio;
                                if (string.IsNullOrEmpty(enderecoAdministradora))
                                {
                                    if (enderecoEmpresa.Cidade != null && enderecoEmpresa.Cidade.Estado != null && enderecoEmpresa.Cidade.Estado.Sigla is not null)
                                    {
                                        enderecoAdministradora = $"{enderecoEmpresa.Logradouro}, {enderecoEmpresa.Bairro}, {enderecoEmpresa.Cidade.Nome}/{enderecoEmpresa.Cidade.Estado.Sigla}";
                                    }
                                }
                                result.Add(new ParameterValueResult(item.Key, enderecoAdministradora!));

                            }
                            break;
                        case "data":
                            {
                                if (parameterDefault != null && parameterDefault.Any(a => !string.IsNullOrEmpty(a.Key) && a.Key.Equals(item.Key, StringComparison.CurrentCulture)))
                                {
                                    var valueUtilizar = parameterDefault.First(a => a.Key.Equals(item.Key, StringComparison.CurrentCultureIgnoreCase));
                                    if (valueUtilizar != null && !string.IsNullOrEmpty(valueUtilizar.FriendlyName))
                                        result.Add(new ParameterValueResult(item.Key, valueUtilizar.FriendlyName));
                                }
                                else
                                {
                                    certidao.Data = DateTime.Now;
                                    certidao.CertidaoEmitidaEm = $"{certidao.Data:dddd, dd MMMM} {certidao.Data:yyyy} {certidao.Data:HH:mm:ss:fff}";
                                    result.Add(new ParameterValueResult(item.Key, certidao.CertidaoEmitidaEm));
                                }
                            }
                            break;
                        case "competencia":
                            {
                                if (parameterDefault != null && parameterDefault.Any(a => !string.IsNullOrEmpty(a.Key) && a.Key.Equals(item.Key, StringComparison.CurrentCulture)))
                                {
                                    var valueUtilizar = parameterDefault.First(a => a.Key.Equals(item.Key, StringComparison.CurrentCultureIgnoreCase));
                                    if (valueUtilizar != null && !string.IsNullOrEmpty(valueUtilizar.FriendlyName))
                                        result.Add(new ParameterValueResult(item.Key, valueUtilizar.FriendlyName));
                                }
                                else
                                {
                                    certidao.Competencia = $"{DateTime.Today:MMMM}, {DateTime.Today:yyyy}";
                                    result.Add(new ParameterValueResult(item.Key, certidao.Competencia));
                                }
                            }
                            break;
                        case "nomesistema":
                            {
                                if (parameterDefault != null && parameterDefault.Any(a => !string.IsNullOrEmpty(a.Key) && a.Key.Equals(item.Key, StringComparison.CurrentCulture)))
                                {
                                    var valueUtilizar = parameterDefault.First(a => a.Key.Equals(item.Key, StringComparison.CurrentCultureIgnoreCase));
                                    if (valueUtilizar != null && !string.IsNullOrEmpty(valueUtilizar.FriendlyName))
                                        result.Add(new ParameterValueResult(item.Key, valueUtilizar.FriendlyName));
                                }
                                else result.Add(new ParameterValueResult(item.Key, nomeSistema));

                            }
                            break;
                        case "proprietario":
                            {
                                if (!string.IsNullOrEmpty(nomeProprietario))
                                {
                                    result.Add(new ParameterValueResult(item.Key, nomeProprietario));
                                }
                                else if (parameterDefault != null && parameterDefault.Any(a => !string.IsNullOrEmpty(a.Key) && a.Key.Equals(item.Key, StringComparison.CurrentCulture)))
                                {
                                    var valueUtilizar = parameterDefault.First(a => a.Key.Equals(item.Key, StringComparison.CurrentCultureIgnoreCase));
                                    if (valueUtilizar != null && !string.IsNullOrEmpty(valueUtilizar.FriendlyName))
                                        result.Add(new ParameterValueResult(item.Key, valueUtilizar.FriendlyName));
                                }
                                else
                                {
                                    certidao.MultiProprietario = usuarioModel.NomePessoa.ToUpper().TrimEnd();
                                    result.Add(new ParameterValueResult(item.Key, certidao.MultiProprietario));
                                }
                            }
                            break;
                        case "nomecampotorre":
                            {
                                if (parameterDefault != null && parameterDefault.Any(a => !string.IsNullOrEmpty(a.Key) && a.Key.Equals(item.Key, StringComparison.CurrentCulture)))
                                {
                                    var valueUtilizar = parameterDefault.First(a => a.Key.Equals(item.Key, StringComparison.CurrentCultureIgnoreCase));
                                    if (valueUtilizar != null && !string.IsNullOrEmpty(valueUtilizar.FriendlyName))
                                        result.Add(new ParameterValueResult(item.Key, valueUtilizar.FriendlyName));
                                }
                                else
                                {
                                    certidao.TorreBlocoNome = "Torre/Bloco";
                                    result.Add(new ParameterValueResult(item.Key, certidao.TorreBlocoNome));
                                }

                            }
                            break;
                        case "nomecampocpfcnpj":
                            {
                                if (parameterDefault != null && parameterDefault.Any(a => !string.IsNullOrEmpty(a.Key) && a.Key.Equals(item.Key, StringComparison.CurrentCulture)))
                                {
                                    var valueUtilizar = parameterDefault.First(a => a.Key.Equals(item.Key, StringComparison.CurrentCultureIgnoreCase));
                                    if (valueUtilizar != null && !string.IsNullOrEmpty(valueUtilizar.FriendlyName))
                                        result.Add(new ParameterValueResult(item.Key, valueUtilizar.FriendlyName));
                                }
                                else
                                {
                                    certidao.NomeCampoCpfCnpj = usuarioModel.Pessoa.TipoPessoa == Domain.Enumns.EnumTipoPessoa.Fisica ? "CPF" : "CNPJ";
                                    result.Add(new ParameterValueResult(item.Key, certidao.NomeCampoCpfCnpj));
                                }

                            }
                            break;
                        case "numerocpfcnpj":
                            {
                                var cpf = usuarioModel.Pessoa.Documentos.Any(a => a.TipoDocumentoNome.Contains("CPF", StringComparison.CurrentCultureIgnoreCase)) ? usuarioModel.Pessoa.Documentos.First(a => a.TipoDocumentoNome.Contains("CPF", StringComparison.OrdinalIgnoreCase)).NumeroFormatado : "";
                                var cnpj = usuarioModel.Pessoa.Documentos.Any(a => a.TipoDocumentoNome.Contains("CNPJ", StringComparison.CurrentCultureIgnoreCase)) ? usuarioModel.Pessoa.Documentos.First(a => a.TipoDocumentoNome.Contains("CNPJ", StringComparison.OrdinalIgnoreCase)).NumeroFormatado : "";

                                if (!string.IsNullOrEmpty(cpf))
                                {
                                    cpf = SW_Utils.Functions.Helper.MaskSubstring(cpf, 4, cpf.Length - 3, '*');
                                }
                                else if (!string.IsNullOrEmpty(cnpj))
                                {
                                    cnpj = SW_Utils.Functions.Helper.MaskSubstring(cnpj, 4, cnpj.Length - 3, '*');
                                }

                                if (parameterDefault != null && parameterDefault.Any(a => !string.IsNullOrEmpty(a.Key) && a.Key.Equals(item.Key, StringComparison.CurrentCulture)))
                                {
                                    var valueUtilizar = parameterDefault.First(a => a.Key.Equals(item.Key, StringComparison.CurrentCultureIgnoreCase));
                                    if (valueUtilizar != null && !string.IsNullOrEmpty(valueUtilizar.FriendlyName))
                                        result.Add(new ParameterValueResult(item.Key, valueUtilizar.FriendlyName));
                                }
                                else
                                {
                                    certidao.CpfCnpj = (!string.IsNullOrEmpty(cpf) ? cpf : cnpj);
                                    result.Add(new ParameterValueResult(item.Key, certidao.CpfCnpj));
                                }
                            }
                            break;
                        case "numerotorre":
                            {
                                if (parameterDefault != null && parameterDefault.Any(a => !string.IsNullOrEmpty(a.Key) && a.Key.Equals(item.Key, StringComparison.CurrentCulture)))
                                {
                                    var valueUtilizar = parameterDefault.First(a => a.Key.Equals(item.Key, StringComparison.CurrentCultureIgnoreCase));
                                    if (valueUtilizar != null && !string.IsNullOrEmpty(valueUtilizar.FriendlyName))
                                        result.Add(new ParameterValueResult(item.Key, valueUtilizar.FriendlyName));
                                }
                                else
                                {
                                    certidao.TorreBlocoNumero = itens.First().BlocoCodigo;
                                    result.Add(new ParameterValueResult(item.Key, certidao.TorreBlocoNumero));
                                }

                            }
                            break;
                        case "numeroapto":
                            {
                                if (parameterDefault != null && parameterDefault.Any(a => !string.IsNullOrEmpty(a.Key) && a.Key.Equals(item.Key, StringComparison.CurrentCulture)))
                                {
                                    var valueUtilizar = parameterDefault.First(a => a.Key.Equals(item.Key, StringComparison.CurrentCultureIgnoreCase));
                                    if (valueUtilizar != null && !string.IsNullOrEmpty(valueUtilizar.FriendlyName))
                                        result.Add(new ParameterValueResult(item.Key, valueUtilizar.FriendlyName));
                                }
                                else
                                {
                                    certidao.ImovelNumero = itens.First().NumeroImovel;
                                    result.Add(new ParameterValueResult(item.Key, certidao.ImovelNumero));
                                }

                            }
                            break;
                        case "numerocota":
                            {
                                if (parameterDefault != null && parameterDefault.Any(a => !string.IsNullOrEmpty(a.Key) && a.Key.Equals(item.Key, StringComparison.CurrentCulture)))
                                {
                                    var valueUtilizar = parameterDefault.First(a => a.Key.Equals(item.Key, StringComparison.CurrentCultureIgnoreCase));
                                    if (valueUtilizar != null && !string.IsNullOrEmpty(valueUtilizar.FriendlyName))
                                        result.Add(new ParameterValueResult(item.Key, valueUtilizar.FriendlyName));
                                }
                                else
                                {
                                    certidao.NumeroFracao = itens.First().FracaoCota;
                                    result.Add(new ParameterValueResult(item.Key, certidao.NumeroFracao));
                                }

                            }
                            break;
                        case "totalcontaspendentes":
                            {
                                if (parameterDefault != null && parameterDefault.Any(a => !string.IsNullOrEmpty(a.Key) && a.Key.Equals(item.Key, StringComparison.CurrentCulture)))
                                {
                                    var valueUtilizar = parameterDefault.First(a => a.Key.Equals(item.Key, StringComparison.CurrentCultureIgnoreCase));
                                    if (valueUtilizar != null && !string.IsNullOrEmpty(valueUtilizar.FriendlyName))
                                        result.Add(new ParameterValueResult(item.Key, valueUtilizar.FriendlyName));
                                }
                                else
                                {
                                    result.Add(new ParameterValueResult(item.Key, $"{itens.Sum(a => a.ValorAtualizado.GetValueOrDefault()):N2}"));
                                }

                            }
                            break;
                        case "numeroprotocolo":
                            {
                                if (parameterDefault != null && parameterDefault.Any(a => !string.IsNullOrEmpty(a.Key) && a.Key.Equals(item.Key, StringComparison.CurrentCulture)))
                                {
                                    var valueUtilizar = parameterDefault.First(a => a.Key.Equals(item.Key, StringComparison.CurrentCultureIgnoreCase));
                                    if (valueUtilizar != null && !string.IsNullOrEmpty(valueUtilizar.FriendlyName))
                                        result.Add(new ParameterValueResult(item.Key, valueUtilizar.FriendlyName));
                                }
                                else
                                {
                                    certidao.Protocolo = "Não informado";
                                    result.Add(new ParameterValueResult(item.Key, certidao.Protocolo));
                                }

                            }
                            break;
                        case "enderecovalidacaodocumento":
                            {
                                if (parameterDefault != null && parameterDefault.Any(a => !string.IsNullOrEmpty(a.Key) && a.Key.Equals(item.Key, StringComparison.CurrentCulture)))
                                {
                                    var valueUtilizar = parameterDefault.First(a => a.Key.Equals(item.Key, StringComparison.CurrentCultureIgnoreCase));
                                    if (valueUtilizar != null && !string.IsNullOrEmpty(valueUtilizar.FriendlyName))
                                        result.Add(new ParameterValueResult(item.Key, valueUtilizar.FriendlyName));
                                }
                                else result.Add(new ParameterValueResult(item.Key, $"Não informado"));

                            }
                            break;
                    }
                }

            }

            return await Task.FromResult(result);
        }

        private async Task<List<ParameterValueResult>?> PrepararSubstituicoesEmissaoNegativaDeDebitos(List<ParameterValueResult>? substituicoes, string funcaoSubstituicoes, UsuarioModel usuarioModel, CertidaoFinanceira certidao, ProprietarioSimplificadoModel proprietario, List<ParameterValueResult> parameterDefault = null)
        {
            var parametrosSistema = await _repository.GetParametroSistemaViewModel();
            if (parametrosSistema == null)
                throw new Exception("Deve ser configurado os parâmetros da empresa");

            var nomeSistema = _configuration.GetValue<string>("NomeDoSistema", "Portal de multipropriedade SW - Soluções");

            var empresa = (await _repository.FindByHql<Empresa>($"From Empresa e Inner Join Fetch e.Pessoa p Where e.Id = {parametrosSistema.EmpresaId}")).FirstOrDefault();
            if (empresa == null)
                throw new FileNotFoundException($"Não foi encontrada a empresa com o Id: {parametrosSistema.EmpresaId}");

            if (empresa.Pessoa == null || empresa.Pessoa.Id == 0)
                throw new FileNotFoundException($"Não foi encontrada a pessoa da empresa com o Id: {parametrosSistema.EmpresaId}");

            var enderecoEmpresa = (await _repository.FindByHql<PessoaEndereco>($"From PessoaEndereco pe Inner Join Fetch pe.Cidade cid Inner Join Fetch pe.TipoEndereco te Where pe.Pessoa = {empresa.Pessoa.Id} and Coalesce(pe.Preferencial,0) = 1")).FirstOrDefault();
            if (enderecoEmpresa == null && string.IsNullOrEmpty(empresa.EnderecoCondominio) && string.IsNullOrEmpty(empresa.EnderecoAdministradoraCondominio) && string.IsNullOrEmpty(parametrosSistema.EnderecoAdministradoraCondominio) && string.IsNullOrEmpty(parametrosSistema.EnderecoCondominio))
                throw new FileNotFoundException($"Não foi encontrado endereço preferencial para a pessoa da empresa com o Id: {parametrosSistema.EmpresaId}, nem nos campos Endereço Condomínio/Endereco Administradora Condomínio na empresa: {empresa.Id} e nem nos campos Endereço empreendimento/Endereço administradora empreendimento no cadastro de parâmetros.");

            var cnpjEmpresa = (await _repository.FindByHql<PessoaDocumento>($"From PessoaDocumento pd Inner Join Fetch pd.TipoDocumento td Where pd.Pessoa = {empresa.Pessoa.Id} and Lower(td.Nome) = 'cnpj'")).FirstOrDefault();
            if (cnpjEmpresa == null && string.IsNullOrEmpty(empresa.CnpjCondominio) && string.IsNullOrEmpty(empresa.CnpjAdministradoraCondominio) && string.IsNullOrEmpty(parametrosSistema.CnpjCondominio) && string.IsNullOrEmpty(parametrosSistema.CnpjAdministradoraCondominio))
                throw new FileNotFoundException($"Não foi encontrado o CNPJ da empresa com o Id: {parametrosSistema.EmpresaId} e nem nos campos Cnpj Condomínio/Cnpj Administradora Condomínio no cadastro da empresa: {empresa.Id} e nem nos campos Endereço administradora/Endereço administradora empreendimento no cadastro de parâmetros.");


            List<ParameterValueResult>? result = new List<ParameterValueResult>();
            if (funcaoSubstituicoes.Equals("certidaonegativadebitos", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var item in substituicoes.Where(c => !string.IsNullOrEmpty(c.Key)))
                {

                    switch (item.Key.ToLower())
                    {
                        case "cnpjadministradora":
                            {
                                if (!string.IsNullOrEmpty(parametrosSistema.CnpjAdministradoraCondominio) || !string.IsNullOrEmpty(parametrosSistema.CnpjCondominio))
                                {
                                    result.Add(new ParameterValueResult(item.Key, parametrosSistema.CnpjAdministradoraCondominio ?? parametrosSistema.CnpjCondominio));
                                    break;
                                }

                                var cnpjAdministradora = !string.IsNullOrEmpty(empresa.CnpjAdministradoraCondominio) ? empresa.CnpjAdministradoraCondominio : empresa.CnpjCondominio;
                                if (string.IsNullOrEmpty(cnpjAdministradora))
                                {
                                    cnpjAdministradora = cnpjEmpresa.NumeroFormatado;
                                }
                                result.Add(new ParameterValueResult(item.Key, cnpjAdministradora!));
                            }
                            break;
                        case "nomeadministradora":
                            {
                                if (!string.IsNullOrEmpty(parametrosSistema.NomeAdministradoraCondominio) || !string.IsNullOrEmpty(parametrosSistema.NomeCondominio))
                                {
                                    result.Add(new ParameterValueResult(item.Key, parametrosSistema.NomeAdministradoraCondominio ?? parametrosSistema.NomeCondominio));
                                    break;
                                }

                                var nomeAdminstradora = !string.IsNullOrEmpty(empresa.NomeAdministradoraCondominio) ? empresa.NomeAdministradoraCondominio : empresa.NomeCondominio;
                                if (string.IsNullOrEmpty(nomeAdminstradora))
                                {
                                    nomeAdminstradora = empresa.Pessoa.Nome;
                                }
                                result.Add(new ParameterValueResult(item.Key, nomeAdminstradora!));
                            }
                            break;
                        case "enderecoadministradora":
                            {
                                if (!string.IsNullOrEmpty(parametrosSistema.EnderecoAdministradoraCondominio) || !string.IsNullOrEmpty(parametrosSistema.EnderecoCondominio))
                                {
                                    result.Add(new ParameterValueResult(item.Key, parametrosSistema.EnderecoAdministradoraCondominio ?? parametrosSistema.EnderecoCondominio));
                                    break;
                                }

                                var enderecoAdministradora = !string.IsNullOrEmpty(empresa.EnderecoAdministradoraCondominio) ? empresa.EnderecoAdministradoraCondominio : empresa.EnderecoCondominio;
                                if (string.IsNullOrEmpty(enderecoAdministradora))
                                {
                                    if (enderecoEmpresa.Cidade != null && enderecoEmpresa.Cidade.Estado != null && enderecoEmpresa.Cidade.Estado.Sigla is not null)
                                    {
                                        enderecoAdministradora = $"{enderecoEmpresa.Logradouro}, {enderecoEmpresa.Bairro}, {enderecoEmpresa.Cidade.Nome}/{enderecoEmpresa.Cidade.Estado.Sigla}";
                                    }
                                }
                                result.Add(new ParameterValueResult(item.Key, enderecoAdministradora!));

                            }
                            break;
                        case "endereco":
                            {
                                if (!string.IsNullOrEmpty(parametrosSistema.EnderecoAdministradoraCondominio) || !string.IsNullOrEmpty(parametrosSistema.EnderecoCondominio))
                                {
                                    result.Add(new ParameterValueResult(item.Key, parametrosSistema.EnderecoAdministradoraCondominio ?? parametrosSistema.EnderecoCondominio));
                                    break;
                                }

                                var enderecoAdministradora = !string.IsNullOrEmpty(empresa.EnderecoAdministradoraCondominio) ? empresa.EnderecoAdministradoraCondominio : empresa.EnderecoCondominio;
                                if (string.IsNullOrEmpty(enderecoAdministradora))
                                {
                                    if (enderecoEmpresa.Cidade != null && enderecoEmpresa.Cidade.Estado != null && enderecoEmpresa.Cidade.Estado.Sigla is not null)
                                    {
                                        enderecoAdministradora = $"{enderecoEmpresa.Logradouro}, {enderecoEmpresa.Bairro}, {enderecoEmpresa.Cidade.Nome}/{enderecoEmpresa.Cidade.Estado.Sigla}";
                                    }
                                }
                                result.Add(new ParameterValueResult(item.Key, enderecoAdministradora!));

                            }
                            break;
                        case "data":
                            {
                                if (parameterDefault != null && parameterDefault.Any(a => !string.IsNullOrEmpty(a.Key) && a.Key.Equals(item.Key, StringComparison.CurrentCulture)))
                                {
                                    var valueUtilizar = parameterDefault.First(a => a.Key.Equals(item.Key, StringComparison.CurrentCultureIgnoreCase));
                                    if (valueUtilizar != null && !string.IsNullOrEmpty(valueUtilizar.FriendlyName))
                                        result.Add(new ParameterValueResult(item.Key, valueUtilizar.FriendlyName));
                                }
                                else
                                {
                                    certidao.Data = DateTime.Now;
                                    certidao.CertidaoEmitidaEm = $"{certidao.Data:dddd, dd MMMM} {certidao.Data:yyyy} {certidao.Data:HH:mm:ss:fff}";
                                    result.Add(new ParameterValueResult(item.Key, certidao.CertidaoEmitidaEm));
                                }
                            }
                            break;
                        case "competencia":
                            {
                                if (parameterDefault != null && parameterDefault.Any(a => !string.IsNullOrEmpty(a.Key) && a.Key.Equals(item.Key, StringComparison.CurrentCulture)))
                                {
                                    var valueUtilizar = parameterDefault.First(a => a.Key.Equals(item.Key, StringComparison.CurrentCultureIgnoreCase));
                                    if (valueUtilizar != null && !string.IsNullOrEmpty(valueUtilizar.FriendlyName))
                                        result.Add(new ParameterValueResult(item.Key, valueUtilizar.FriendlyName));
                                }
                                else
                                {
                                    certidao.Competencia = $"{DateTime.Today:MMMM}, {DateTime.Today:yyyy}";
                                    result.Add(new ParameterValueResult(item.Key, certidao.Competencia));
                                }
                            }
                            break;
                        case "nomesistema":
                            {
                                if (parameterDefault != null && parameterDefault.Any(a => !string.IsNullOrEmpty(a.Key) && a.Key.Equals(item.Key, StringComparison.CurrentCulture)))
                                {
                                    var valueUtilizar = parameterDefault.First(a => a.Key.Equals(item.Key, StringComparison.CurrentCultureIgnoreCase));
                                    if (valueUtilizar != null && !string.IsNullOrEmpty(valueUtilizar.FriendlyName))
                                        result.Add(new ParameterValueResult(item.Key, valueUtilizar.FriendlyName));
                                }
                                else result.Add(new ParameterValueResult(item.Key, nomeSistema));

                            }
                            break;
                        case "proprietario":
                            {
                                if (parameterDefault != null && parameterDefault.Any(a => !string.IsNullOrEmpty(a.Key) && a.Key.Equals(item.Key, StringComparison.CurrentCulture)))
                                {
                                    var valueUtilizar = parameterDefault.First(a => a.Key.Equals(item.Key, StringComparison.CurrentCultureIgnoreCase));
                                    if (valueUtilizar != null && !string.IsNullOrEmpty(valueUtilizar.FriendlyName))
                                        result.Add(new ParameterValueResult(item.Key, valueUtilizar.FriendlyName));
                                }
                                else
                                {
                                    certidao.MultiProprietario = usuarioModel.NomePessoa.ToUpper().TrimEnd();
                                    result.Add(new ParameterValueResult(item.Key, certidao.MultiProprietario));
                                }

                            }
                            break;
                        case "nomecampotorre":
                            {
                                if (parameterDefault != null && parameterDefault.Any(a => !string.IsNullOrEmpty(a.Key) && a.Key.Equals(item.Key, StringComparison.CurrentCulture)))
                                {
                                    var valueUtilizar = parameterDefault.First(a => a.Key.Equals(item.Key, StringComparison.CurrentCultureIgnoreCase));
                                    if (valueUtilizar != null && !string.IsNullOrEmpty(valueUtilizar.FriendlyName))
                                        result.Add(new ParameterValueResult(item.Key, valueUtilizar.FriendlyName));
                                }
                                else
                                {
                                    certidao.TorreBlocoNome = "Torre/Bloco";
                                    result.Add(new ParameterValueResult(item.Key, certidao.TorreBlocoNome));
                                }

                            }
                            break;
                        case "nomecampocpfcnpj":
                            {
                                if (parameterDefault != null && parameterDefault.Any(a => !string.IsNullOrEmpty(a.Key) && a.Key.Equals(item.Key, StringComparison.CurrentCulture)))
                                {
                                    var valueUtilizar = parameterDefault.First(a => a.Key.Equals(item.Key, StringComparison.CurrentCultureIgnoreCase));
                                    if (valueUtilizar != null && !string.IsNullOrEmpty(valueUtilizar.FriendlyName))
                                        result.Add(new ParameterValueResult(item.Key, valueUtilizar.FriendlyName));
                                }
                                else
                                {
                                    certidao.NomeCampoCpfCnpj = usuarioModel.Pessoa.TipoPessoa == Domain.Enumns.EnumTipoPessoa.Fisica ? "CPF" : "CNPJ";
                                    result.Add(new ParameterValueResult(item.Key, certidao.NomeCampoCpfCnpj));
                                }

                            }
                            break;
                        case "numerocpfcnpj":
                            {
                                var cpf = usuarioModel.Pessoa.Documentos.Any(a => a.TipoDocumentoNome.Contains("CPF", StringComparison.CurrentCultureIgnoreCase)) ? usuarioModel.Pessoa.Documentos.First(a => a.TipoDocumentoNome.Contains("CPF", StringComparison.OrdinalIgnoreCase)).NumeroFormatado : "";
                                var cnpj = usuarioModel.Pessoa.Documentos.Any(a => a.TipoDocumentoNome.Contains("CNPJ", StringComparison.CurrentCultureIgnoreCase)) ? usuarioModel.Pessoa.Documentos.First(a => a.TipoDocumentoNome.Contains("CNPJ", StringComparison.OrdinalIgnoreCase)).NumeroFormatado : "";


                                if (!string.IsNullOrEmpty(cpf))
                                {
                                    cpf = SW_Utils.Functions.Helper.MaskSubstring(cpf, 4, cpf.Length - 3, '*');
                                }
                                else if (!string.IsNullOrEmpty(cnpj))
                                {
                                    cnpj = SW_Utils.Functions.Helper.MaskSubstring(cnpj, 4, cnpj.Length - 3, '*');
                                }

                                if (parameterDefault != null && parameterDefault.Any(a => !string.IsNullOrEmpty(a.Key) && a.Key.Equals(item.Key, StringComparison.CurrentCulture)))
                                {
                                    var valueUtilizar = parameterDefault.First(a => a.Key.Equals(item.Key, StringComparison.CurrentCultureIgnoreCase));
                                    if (valueUtilizar != null && !string.IsNullOrEmpty(valueUtilizar.FriendlyName))
                                        result.Add(new ParameterValueResult(item.Key, valueUtilizar.FriendlyName));
                                }
                                else
                                {
                                    certidao.CpfCnpj = (!string.IsNullOrEmpty(cpf) ? cpf : cnpj);
                                    result.Add(new ParameterValueResult(item.Key, certidao.CpfCnpj));
                                }
                            }
                            break;
                        case "numerotorre":
                            {
                                if (parameterDefault != null && parameterDefault.Any(a => !string.IsNullOrEmpty(a.Key) && a.Key.Equals(item.Key, StringComparison.CurrentCulture)))
                                {
                                    var valueUtilizar = parameterDefault.First(a => a.Key.Equals(item.Key, StringComparison.CurrentCultureIgnoreCase));
                                    if (valueUtilizar != null && !string.IsNullOrEmpty(valueUtilizar.FriendlyName))
                                        result.Add(new ParameterValueResult(item.Key, valueUtilizar.FriendlyName));
                                }
                                else
                                {
                                    certidao.TorreBlocoNumero = proprietario.BlocoCodigo;
                                    result.Add(new ParameterValueResult(item.Key, certidao.TorreBlocoNumero));
                                }

                            }
                            break;
                        case "numeroapto":
                            {
                                if (parameterDefault != null && parameterDefault.Any(a => !string.IsNullOrEmpty(a.Key) && a.Key.Equals(item.Key, StringComparison.CurrentCulture)))
                                {
                                    var valueUtilizar = parameterDefault.First(a => a.Key.Equals(item.Key, StringComparison.CurrentCultureIgnoreCase));
                                    if (valueUtilizar != null && !string.IsNullOrEmpty(valueUtilizar.FriendlyName))
                                        result.Add(new ParameterValueResult(item.Key, valueUtilizar.FriendlyName));
                                }
                                else
                                {
                                    certidao.ImovelNumero = proprietario.ImovelNumero;
                                    result.Add(new ParameterValueResult(item.Key, certidao.ImovelNumero));
                                }

                            }
                            break;
                        case "numerocota":
                            {
                                if (parameterDefault != null && parameterDefault.Any(a => !string.IsNullOrEmpty(a.Key) && a.Key.Equals(item.Key, StringComparison.CurrentCulture)))
                                {
                                    var valueUtilizar = parameterDefault.First(a => a.Key.Equals(item.Key, StringComparison.CurrentCultureIgnoreCase));
                                    if (valueUtilizar != null && !string.IsNullOrEmpty(valueUtilizar.FriendlyName))
                                        result.Add(new ParameterValueResult(item.Key, valueUtilizar.FriendlyName));
                                }
                                else
                                {
                                    certidao.NumeroFracao = proprietario.CodigoFracao;
                                    result.Add(new ParameterValueResult(item.Key, certidao.NumeroFracao));
                                }

                            }
                            break;
                        case "numeroprotocolo":
                            {
                                if (parameterDefault != null && parameterDefault.Any(a => !string.IsNullOrEmpty(a.Key) && a.Key.Equals(item.Key, StringComparison.CurrentCulture)))
                                {
                                    var valueUtilizar = parameterDefault.First(a => a.Key.Equals(item.Key, StringComparison.CurrentCultureIgnoreCase));
                                    if (valueUtilizar != null && !string.IsNullOrEmpty(valueUtilizar.FriendlyName))
                                        result.Add(new ParameterValueResult(item.Key, valueUtilizar.FriendlyName));
                                }
                                else
                                {
                                    certidao.Protocolo = "Não informado";
                                    result.Add(new ParameterValueResult(item.Key, certidao.Protocolo));
                                }

                            }
                            break;
                        case "enderecovalidacaodocumento":
                            {
                                if (parameterDefault != null && parameterDefault.Any(a => !string.IsNullOrEmpty(a.Key) && a.Key.Equals(item.Key, StringComparison.CurrentCulture)))
                                {
                                    var valueUtilizar = parameterDefault.First(a => a.Key.Equals(item.Key, StringComparison.CurrentCultureIgnoreCase));
                                    if (valueUtilizar != null && !string.IsNullOrEmpty(valueUtilizar.FriendlyName))
                                        result.Add(new ParameterValueResult(item.Key, valueUtilizar.FriendlyName));
                                }
                                else result.Add(new ParameterValueResult(item.Key, $"Não informado"));

                            }
                            break;
                    }
                }

            }

            return await Task.FromResult(result);
        }

        public async Task<CertidaoViewModel?> ValidarCertidao(string protocolo)
        {
            if (string.IsNullOrEmpty(protocolo))
                throw new FileNotFoundException("Não foi encontrada certidão com o protocolo informado.");

            var certidao = (await _repository.FindBySql<CertidaoViewModel>($"Select c.Protocolo, c.Competencia, c.MultiProprietario, c.CpfCnpj, c.ImovelNumero, c.TorreBlocoNumero, c.NumeroFracao as Cota, c.CertidaoEmitidaEm, c.NomeCampoCpfCnpj as NomeDocumento  From CertidaoFinanceira c Where Lower(c.Protocolo) = '{protocolo.TrimEnd().ToLower()}'")).FirstOrDefault();

            if (!string.IsNullOrEmpty(certidao.CpfCnpj))
            {
                certidao.CpfCnpj = SW_Utils.Functions.Helper.MaskSubstring(certidao.CpfCnpj, 4, certidao.CpfCnpj.Length - 3, '*');
            }

            return certidao;

        }

        private async Task<string> AplicarSubstituicoes(string htmlContent, List<ParameterValueResult>? substituicoes)
        {
            var htmlAjustado = htmlContent;
            if (substituicoes == null || !substituicoes.Any())
                return await Task.FromResult(htmlAjustado);

            foreach (var item in substituicoes)
            {
                htmlAjustado = htmlAjustado.Replace($"[{item.Key}]", item.FriendlyName, StringComparison.OrdinalIgnoreCase);
            }

            return await Task.FromResult(htmlAjustado);
        }

    }
}
