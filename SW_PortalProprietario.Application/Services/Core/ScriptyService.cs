using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_Utils.Auxiliar;
using System.Text.RegularExpressions;

namespace SW_PortalProprietario.Application.Services.Core
{
    public class ScriptService : IScriptService
    {
        private readonly IRepositoryNH _repository;
        private readonly ILogger<ScriptService> _logger;
        private readonly IProjectObjectMapper _mapper;
        private readonly IConfiguration _config;
        private List<(int, string)> filesPath = new List<(int, string)>()
        {
            (0,"C:\\inetpub\\wwwroot\\ModelosComunicacoes\\ComunicacoesGerais.html")
        };

        public ScriptService(IRepositoryNH repository,
            ILogger<ScriptService> logger,
            IProjectObjectMapper mapper,
            IConfiguration configuration)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _config = configuration;
        }

        public async Task<HtmlTemplateResultModel?> GenerateHtmlFromTemplate(HtmlTemplateExecuteModel model)
        {
            var htmlTemplate = await _repository.FindById<HtmlTemplate>(model.TemplateId.GetValueOrDefault());
            if (htmlTemplate == null)
                throw new ArgumentException($"Não foi encontrado o HtmlTemplate Id: {model.TemplateId.GetValueOrDefault()}");

            var templateBase = _mapper.Map<HtmlTemplateModel>(htmlTemplate);
            var dadosRetorno = await PrepareAsync(templateBase, model.RetornarPdf.GetValueOrDefault(false), model.idObjeto.GetValueOrDefault());

            return dadosRetorno;
        }

        private async Task<HtmlTemplateResultModel?> PrepareAsync(HtmlTemplateModel template, bool retornarPdf, int objectId)
        {
            string fileBasePath = "";
            if (template.TipoComunicacao.HasValue)
            {
                var tipo = filesPath.FirstOrDefault(a => a.Item1 == (int)template.TipoComunicacao.GetValueOrDefault());
                if (string.IsNullOrEmpty(tipo.Item2))
                {
                    throw new ArgumentException($"Não foi encontrado o tipo de arquivo: {template.TipoComunicacao}");
                }

                var exist = File.Exists(tipo.Item2);
                if (!exist)
                    throw new ArgumentException($"Não foi encontrado o tipo de arquivo: {template.TipoComunicacao} no caminho: '{tipo.Item2}'");

                fileBasePath = tipo.Item2;
            }

            HtmlTemplateResultModel? result = null;
            try
            {
                string patternTagsToReplace = @"\[(.*?)\]";
                var matchesTitulo = Regex.Matches(template.Titulo, patternTagsToReplace);
                var matchesHeader = Regex.Matches(template.Header, patternTagsToReplace);
                var matchesConteudo = Regex.Matches(template.Content, patternTagsToReplace);

                var parameters = new List<Parameter>();
                foreach (var itemParametro in template.ParametrosConsulta.Split('|'))
                {
                    parameters.Add(new Parameter(itemParametro.Replace("[", "").Replace("]", ""), objectId));
                }
                List<(string key, string value)> listKeysValue = new List<(string key, string value)>();

                var consultaResult = await _repository.FindBySql<dynamic>(template.Consulta, parameters.ToArray());
                foreach (var item in consultaResult)
                {
                    foreach (var itemLookup in item)
                    {
                        listKeysValue.Add((itemLookup.Key, itemLookup.Value.ToString()));
                    }
                }

                foreach (Match match in matchesTitulo)
                {
                    var itensTitulo = listKeysValue.FirstOrDefault(a => a.key.Equals(match.Groups[1].Value, StringComparison.CurrentCultureIgnoreCase));
                    if (itensTitulo.value != null)
                        template.Titulo = template.Titulo.Replace($"[{itensTitulo.key}]", itensTitulo.value, StringComparison.InvariantCultureIgnoreCase);
                }

                foreach (Match match in matchesHeader)
                {
                    var itensHeader = listKeysValue.FirstOrDefault(a => a.key.Equals(match.Groups[1].Value, StringComparison.CurrentCultureIgnoreCase));
                    if (itensHeader.value != null)
                        template.Header = template.Header.Replace($"[{itensHeader.key}]", itensHeader.value, StringComparison.InvariantCultureIgnoreCase);
                }

                foreach (Match match in matchesConteudo)
                {
                    var itemSubstituicao = listKeysValue.FirstOrDefault(a => a.key.Equals(match.Groups[1].Value, StringComparison.CurrentCultureIgnoreCase));
                    if (itemSubstituicao.value != null)
                        template.Content = template.Content.Replace($"[{itemSubstituicao.key}]", itemSubstituicao.value, StringComparison.InvariantCultureIgnoreCase);
                }

                var pathGeracaoPdf = _config.GetValue<string>($"PathGeracaoPdfComunicacoesGeraia", "C:\\inetpub\\wwwroot\\ComunicacoesGeais\\");

                var pdfDocumentPath = Path.Combine(pathGeracaoPdf, $"{Guid.NewGuid()}.pdf");

                string htmlContent = File.ReadAllText(fileBasePath);
                var matchesFileFull = Regex.Matches(htmlContent, patternTagsToReplace);
                htmlContent = htmlContent.Replace($"[TituloDocumento]", template.Titulo);
                htmlContent = htmlContent.Replace($"[HeaderDocumento]", template.Header);
                htmlContent = htmlContent.Replace($"[ContentDocumento]", template.Content);


                result = new HtmlTemplateResultModel()
                {
                    TemplateModelPopulado = template,
                    Html = htmlContent,
                    FilePath = pdfDocumentPath
                };

                if (retornarPdf)
                {
                    if (!Directory.Exists(Path.GetDirectoryName(pathGeracaoPdf)))
                        Directory.CreateDirectory(pathGeracaoPdf);

                    await GerarHtmlExecute(result);
                }
            }
            catch (Exception err)
            {
                throw;
            }

            return result;
        }


        private async Task GerarHtmlExecute(HtmlTemplateResultModel htmlTemplateModel)
        {
            var launchOptions = new LaunchOptions
            {
                Headless = true // Define se o navegador será exibido ou não
            };

            // Inicializar o PuppeteerSharp
            await new BrowserFetcher().DownloadAsync();
            using var browser = await Puppeteer.LaunchAsync(launchOptions);
            using var page = await browser.NewPageAsync();

            // Carregar o conteúdo HTML na página
            await page.SetContentAsync(htmlTemplateModel.Html);

            // Gerar o PDF
            await page.PdfAsync(htmlTemplateModel.FilePath);

            htmlTemplateModel.FilePath = htmlTemplateModel.FilePath;

        }

    }
}
