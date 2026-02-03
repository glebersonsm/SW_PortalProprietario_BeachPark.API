using Dapper;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_Utils.Auxiliar;
using System.Text;
using System.Text.RegularExpressions;

namespace SW_PortalProprietario.Application.Services.Core
{
    public class HtmlTemplateService : IHtmlTemplateService
    {
        private readonly IRepositoryNH _repository;
        private readonly ILogger<HtmlTemplateService> _logger;
        private readonly IProjectObjectMapper _mapper;
        private readonly IServiceBase _serviceBase;
        private readonly IEmpreendimentoHybridProviderService _empreendimentoService;


        private List<(int, string)> filesPath = new List<(int, string)>()
        {
            (0,"C:\\inetpub\\wwwroot\\ModelosComunicacoes\\ComunicacoesGerais.html")
        };

        public HtmlTemplateService(IRepositoryNH repository,
            ILogger<HtmlTemplateService> logger,
            IProjectObjectMapper mapper,
            IServiceBase serviceBase,
            ICommunicationProvider communicationProvider,
            IEmpreendimentoHybridProviderService empreendimentoProviderService)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _serviceBase = serviceBase;
            _empreendimentoService = empreendimentoProviderService;
        }

        public async Task<DeleteResultModel> DeleteHtmlTemplate(int id)
        {
            var result = new DeleteResultModel();
            result.Id = id;

            try
            {

                var htmlTemlate = await _repository.FindById<HtmlTemplate>(id);
                if (htmlTemlate is null)
                {
                    throw new ArgumentException($"Não foi encontrado o HtmlTemplate com Id: {id}!");
                }


                _repository.BeginTransaction();
                await _repository.Remove(htmlTemlate);

                var resultCommit = await _repository.CommitAsync();
                if (resultCommit.executed)
                {
                    result.Result = "Removido com sucesso!";
                }
                else
                {
                    throw resultCommit.exception ?? new Exception("Não foi possível realizar a operação");
                }

                return result;

            }
            catch (Exception err)
            {
                _repository.Rollback();
                _logger.LogError(err, $"Não foi possível deletar o HtmlTemplate: {id}");
                throw;
            }
        }

        public async Task<List<KeyValueModel>> GetKeyValueListFromContratoSCP(GetHtmlValuesModel model)
        {

            var result = await _empreendimentoService.GetKeyValueListFromContratoSCP(model,"AGUARDANDO CONFIRMAÇÃO",DateTime.Today);
            return result;
        }


        public async Task<HtmlTemplateModel> SaveHtmlTemplate(HtmlTemplateInputModel model)
        {
            try
            {
                _repository.BeginTransaction();

                bool inclusao = model.Id.GetValueOrDefault(0) == 0;

                HtmlTemplate htmlTemplate = null;
                if (model.Id.GetValueOrDefault(0) > 0)
                    htmlTemplate = (await _repository.FindByHql<HtmlTemplate>($"From HtmlTemplate h Where h.Id = {model.Id}")).FirstOrDefault();

                var htmlTemplateSalvar = htmlTemplate != null ? _mapper.Map(model, htmlTemplate) : _mapper.Map(model, new HtmlTemplate());
                if (htmlTemplateSalvar.TipoComunicacao.HasValue)
                {
                    var tipo = filesPath.FirstOrDefault(a => a.Item1 == (int)htmlTemplateSalvar.TipoComunicacao.GetValueOrDefault());
                    if (string.IsNullOrEmpty(tipo.Item2))
                    {
                        throw new ArgumentException($"Não foi encontrado o tipo de arquivo: {htmlTemplateSalvar.TipoComunicacao}");
                    }

                    var exist = File.Exists(tipo.Item2);
                    if (!exist)
                        throw new ArgumentException($"Não foi encontrado o tipo de arquivo: {htmlTemplateSalvar.TipoComunicacao} no caminho: '{tipo.Item2}'");
                }

                if (!string.IsNullOrEmpty(model.Consulta) && !string.IsNullOrEmpty(htmlTemplateSalvar.Consulta))
                {
                    htmlTemplateSalvar.ParametrosConsulta = "";
                    htmlTemplateSalvar.ColunasDeRetorno = "";

                    if (htmlTemplateSalvar.Consulta.Contains("Like", StringComparison.CurrentCultureIgnoreCase))
                        throw new ArgumentException("Não é permitida a palavra Like nas consulta, todos os filtros deverão ser relizados pelo Id do objeto no formato: ObjetoId =:idDoObjeto");
                    if (htmlTemplateSalvar.Consulta.Contains("delete", StringComparison.CurrentCultureIgnoreCase))
                        throw new ArgumentException("Não é permitida a palavra delete");
                    if (htmlTemplateSalvar.Consulta.Contains("update", StringComparison.CurrentCultureIgnoreCase))
                        throw new ArgumentException("Não é permitida a palavra update");
                    if (htmlTemplateSalvar.Consulta.Contains("insert", StringComparison.CurrentCultureIgnoreCase))
                        throw new ArgumentException("Não é permitida a palavra insert");
                    if (htmlTemplateSalvar.Consulta.Contains("truncate", StringComparison.CurrentCultureIgnoreCase))
                        throw new ArgumentException("Não é permitida a palavra truncate");
                    if (htmlTemplateSalvar.Consulta.Contains("table", StringComparison.CurrentCultureIgnoreCase))
                        throw new ArgumentException("Não é permitida a palavra table");

                    string patternFiltros = @"(\S+)\s*=\s*(?::'?(\w+)'?)";
                    MatchCollection matches = Regex.Matches(htmlTemplateSalvar.Consulta, patternFiltros, RegexOptions.IgnoreCase);
                    foreach (Match match in matches)
                    {
                        string fieldName = match.Groups[1].Value;
                        string parameterName = match.Groups[2].Value;
                        if (string.IsNullOrEmpty(htmlTemplateSalvar.ParametrosConsulta))
                        {
                            htmlTemplateSalvar.ParametrosConsulta = $"[{parameterName}]";
                        }
                        else htmlTemplateSalvar.ParametrosConsulta += $"|[{parameterName}]";
                    }

                    //string patternConteudos = @"\[(.*?)\]";

                    //MatchCollection replacesTitulo = Regex.Matches(htmlTemplateSalvar.Titulo, patternConteudos, RegexOptions.IgnoreCase);
                    //foreach (Match item in replacesTitulo)
                    //{
                    //    if (!htmlTemplateSalvar.ColunasDeRetorno.Split('|').Any(b => b.EndsWith($"{item.Groups[1].Value}", StringComparison.CurrentCultureIgnoreCase)))
                    //        throw new ArgumentException($"Não foi encontrada a coluna: '{item.Groups[1].Value}' na consulta vinculada");
                    //}

                    //MatchCollection replacesHeader = Regex.Matches(htmlTemplateSalvar.Header, patternConteudos, RegexOptions.IgnoreCase);
                    //foreach (Match item in replacesHeader)
                    //{
                    //    if (!htmlTemplateSalvar.ColunasDeRetorno.Split('|').Any(b => b.EndsWith(item.Groups[1].Value, StringComparison.CurrentCultureIgnoreCase)))
                    //        throw new ArgumentException($"Não foi encontrada a coluna: '{item.Groups[1].Value}' na consulta vinculada");
                    //}

                    //MatchCollection replacesConteudo = Regex.Matches(htmlTemplateSalvar.Content, patternConteudos, RegexOptions.IgnoreCase);
                    //foreach (Match item in replacesConteudo)
                    //{
                    //    if (!htmlTemplateSalvar.ColunasDeRetorno.Split('|').Any(b => b.EndsWith(item.Groups[1].Value, StringComparison.CurrentCultureIgnoreCase)))
                    //        throw new ArgumentException($"Não foi encontrada a coluna: '{item.Groups[1].Value}' na consulta vinculada");
                    //}
                }

                //Desafios, pegar as colunas do próprio retorno da consulta
                //Considerar usar Hql, por causa das funções exemplo GetDate()

                var parametros = new List<Parameter>();
                foreach (var itemParametro in htmlTemplateSalvar.ParametrosConsulta.Split('|'))
                {
                    parametros.Add(new Parameter(itemParametro.Replace("[", "").Replace("]", ""), -1));
                }

                try
                {
                    var resultConsulta = await _repository.FindBySql<dynamic>(htmlTemplateSalvar.Consulta, parametros.ToArray());

                }
                catch (Exception err)
                {
                    throw new ArgumentException($"Não foi possível executar a consulta: {htmlTemplateSalvar.Consulta} - Erro: {err.Message} - Inner: {err.InnerException?.Message}");
                }

                var result = await _repository.Save(htmlTemplateSalvar);
                htmlTemplate = result;

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"Html Template: ({result.Id}) salvo com sucesso!");

                    if (result != null)
                        return _mapper.Map<HtmlTemplateModel>(result);

                }
                throw exception ?? new Exception($"Não foi possível salvar o HtmlTemplate: ({htmlTemplateSalvar.Titulo})");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível salvar o HtmlTemplate: ({model.Titulo})");
                _repository.Rollback();
                throw;
            }
        }

        public async Task<List<HtmlTemplateModel>?> Search(SearchHtmlTemplateModel searchModel)
        {
            List<Parameter> parameters = new();
            List<Parameter> parameters1 = new();
            StringBuilder sb = new("From HtmlTemplate h Where 1 = 1");
            if (!string.IsNullOrEmpty(searchModel.Titulo))
                sb.AppendLine($" and Lower(h.Titulo) like '%{searchModel.Titulo.ToLower()}%'");

            if (searchModel.Id.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" and h.Id = :id ");
                parameters.Add(new Parameter("id", searchModel.Id.GetValueOrDefault()));
            }

            if (searchModel.TipoComunicacao.HasValue)
            {
                sb.AppendLine($" and TipoComunicacao = :tipoComunicacao ");
                parameters.Add(new Parameter("tipoComunicacao", (int)searchModel.TipoComunicacao.GetValueOrDefault()));
            }

            var htmlTemplates = await _repository.FindByHql<HtmlTemplate>(sb.ToString(), parameters.ToArray());

            if (htmlTemplates.Any())
            {
                var htmlTemplatesReturn = await _serviceBase.SetUserName(htmlTemplates.Select(a => _mapper.Map<HtmlTemplateModel>(a)).AsList());

                return htmlTemplatesReturn;

            }

            return default;
        }
    }
}
