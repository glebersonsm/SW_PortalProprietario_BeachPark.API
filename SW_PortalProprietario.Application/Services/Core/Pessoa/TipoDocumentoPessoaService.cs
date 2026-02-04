using Dapper;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.PessoaModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;
using SW_PortalProprietario.Domain.Enumns;
using SW_Utils.Auxiliar;
using System.Text;

namespace SW_PortalProprietario.Application.Services.Core.Pessoa
{
    public class TipoDocumentoPessoaService : ITipoDocumentoPessoaService
    {
        private readonly IRepositoryNH _repository;
        private readonly ILogger<TipoDocumentoPessoaService> _logger;
        private readonly IServiceBase _serviceBase;
        public TipoDocumentoPessoaService(IRepositoryNH repository,
            ILogger<TipoDocumentoPessoaService> logger,
            IServiceBase serviceBase)
        {
            _repository = repository;
            _logger = logger;
            _serviceBase = serviceBase;
        }

        public async Task<DeleteResultModel> Remover(int id)
        {
            var result = new DeleteResultModel();
            result.Id = id;

            try
            {

                var tipoDocumentoPessoa = await _repository.FindById<TipoDocumentoPessoa>(id);
                if (tipoDocumentoPessoa is null)
                {
                    throw new FileNotFoundException($"Não foi encontrado o tipo de documento com Id: {id}!");
                }

                var pessoaDocumentoUsing = (await _repository.FindBySql<TipoDocumentoPessoaModel>("Select s.Id, s.Numero From PessoaDocumento s Where s.TipoDocumento =:tipodocumentoId", session: null, new Parameter("tipodocumentoId", id))).Take(5).ToList();
                if (pessoaDocumentoUsing.Any())
                {
                    foreach (var itempessoaDocumentoUsing in pessoaDocumentoUsing)
                    {
                        throw new ArgumentException($"A Pessoa Documento Id: {itempessoaDocumentoUsing.Id} - Nome: {itempessoaDocumentoUsing.Nome} utiliza o tipo de documento: {id}");
                    }
                }

                _repository.BeginTransaction();
                _repository.Remove(tipoDocumentoPessoa);

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
                _logger.LogError(err, $"Não foi possível deletar o Tipo de documento: {id}");
                throw;
            }

        }

        public async Task<TipoDocumentoPessoaModel> Salvar(TipoDocumentoPessoaInputModel tipoDocumento)
        {
            try
            {
                _repository.BeginTransaction();
                var are = (TipoDocumentoPessoa)tipoDocumento;

                var result = await _repository.Save(are);

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"Tipo Documento: ({result.Id} - {tipoDocumento.Nome}) salvo com sucesso!");

                    if (result != null)
                        return (TipoDocumentoPessoaModel)result;

                }

                throw exception ?? new Exception($"Não foi possível salvar o Tipo Documento: ({tipoDocumento.Nome})");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível salvar o Tipo Documento: ({tipoDocumento.Nome})");
                _repository.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<TipoDocumentoPessoaModel>?> Search(SearchPadraoComTipoPessoaModel searchModel)
        {
            List<Parameter> parameters = new();
            List<Parameter> parameters1 = new();
            StringBuilder sb = new("From TipoDocumentoPessoa ge Where 1 = 1");

            if (!string.IsNullOrEmpty(searchModel.Nome))
            {
                sb.AppendLine($" and ge.Nome = :nome");
                parameters.Add(new Parameter("nome", searchModel.Nome.ToLower()));
            }

            if (searchModel.Id.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" and ge.Id = {searchModel.Id.GetValueOrDefault()} ");
            }

            if (searchModel.Ids != null && searchModel.Ids.Any())
            {
                sb.AppendLine($" and ge.Id in ({string.Join(",", searchModel.Ids)}) ");
            }

            if (searchModel.UsuarioCriacao.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" and ge.UsuarioCriacao = {searchModel.UsuarioCriacao.GetValueOrDefault()}");
            }

            if (searchModel.TipoPessoa != null)
            {
                if (searchModel.TipoPessoa == Domain.Enumns.EnumTiposPessoa.PessoaFisica ||
                    searchModel.TipoPessoa == Domain.Enumns.EnumTiposPessoa.PessoaJuridica)
                {
                    sb.AppendLine($" and ge.TipoPessoa in ({(int)searchModel.TipoPessoa},{(int)EnumTiposPessoa.PessoaFisicaEJuridica})");
                }
            }

            var tipoDocumento = await _repository.FindByHql<TipoDocumentoPessoa>(sb.ToString(), session: null, parameters.ToArray());

            if (tipoDocumento.Any())
                return await _serviceBase.SetUserName(tipoDocumento.Select(a => (TipoDocumentoPessoaModel)a).AsList());

            return default;
        }

        public async Task<TipoDocumentoPessoaModel> Update(TipoDocumentoPessoaInputModel model)
        {
            _repository.BeginTransaction();
            try
            {
                var tipodocumento = (await _repository.FindByHql<TipoDocumentoPessoa>("From TipoDocumentoPessoa ge Where ge.Id = :id", session: null, new Parameter[]
                { new Parameter("id", model.Id) })).FirstOrDefault() ?? throw new Exception($"Não foi encontrado o Tipo Documento: {model.Id}");

                tipodocumento.Nome = model.Nome ?? tipodocumento.Nome;
                tipodocumento.Mascara = model.Mascara ?? tipodocumento.Mascara;
                tipodocumento.ExigeOrgaoEmissor = model.ExigeOrgaoEmissor ?? tipodocumento.ExigeOrgaoEmissor;
                tipodocumento.ExigeDataEmissao = model.ExigeDataEmissao ?? tipodocumento.ExigeDataEmissao;
                tipodocumento.ExigeDataValidade = model.ExigeDataValidade ?? tipodocumento.ExigeDataValidade;
                tipodocumento.TipoPessoa = model.TipoPessoa ?? tipodocumento.TipoPessoa;
                await _repository.Save(tipodocumento);
                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                    return (TipoDocumentoPessoaModel)tipodocumento;
                else throw exception ?? new Exception("Erro na operação");
            }
            catch (Exception)
            {
                _repository.Rollback();
                throw;
            }

        }
    }
}
