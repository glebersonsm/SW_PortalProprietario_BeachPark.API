using Dapper;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.PessoaModels;
using SW_PortalProprietario.Application.Services.Core.Auxiliar;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;
using SW_Utils.Auxiliar;
using System.Text;

namespace SW_PortalProprietario.Application.Services.Core.Pessoa
{
    public class PessoaDocumentoService : IPessoaDocumentoService
    {
        private readonly IRepositoryNH _repository;
        private readonly ILogger<PessoaDocumentoService> _logger;
        private readonly IServiceBase _serviceBase;
        private readonly IProjectObjectMapper _mapper;
        public PessoaDocumentoService(IRepositoryNH repository,
            ILogger<PessoaDocumentoService> logger,
            IServiceBase serviceBase,
            IProjectObjectMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _serviceBase = serviceBase;
            _mapper = mapper;

        }

        public async Task<DeleteResultModel> Remover(int id)
        {
            var result = new DeleteResultModel();
            result.Id = id;

            try
            {

                var documentoPessoa = await _repository.FindById<PessoaDocumento>(id);
                if (documentoPessoa is null)
                {
                    throw new FileNotFoundException($"Não foi encontrado documento com Id: {id}!");
                }

                var pessoaDocumentoUsing = (await _repository.FindBySql<PessoaDocumentoModel>("Select s.Id, s.Numero From PessoaDocumento s Where s.TipoDocumento =:tipodocumentoId", new Parameter("tipodocumentoId", id))).Take(5).ToList();
                if (pessoaDocumentoUsing.Any())
                {
                    foreach (var itempessoaDocumentoUsing in pessoaDocumentoUsing)
                    {
                        throw new ArgumentException($"O Documento Id: {itempessoaDocumentoUsing.Id} - Nome: {itempessoaDocumentoUsing.PessoaId} utiliza o tipo de documento: {id}");
                    }
                }

                _repository.BeginTransaction();
                await _repository.Remove(documentoPessoa);

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

        public async Task<PessoaDocumentoModel> Salvar(PessoaDocumentoInputModel model)
        {
            try
            {
                _repository.BeginTransaction();

                var pessoa = (await _repository.FindBySql<Domain.Entities.Core.DadosPessoa.Pessoa>($"Select p.* From Pessoa p Where p.Id = {model.PessoaId.GetValueOrDefault()}")).FirstOrDefault() ?? throw new ArgumentException($"Não foi encontrada a Pessoa: {model.PessoaId}");

                var pessoaSincronizacaoListaAuxiliar = new PessoaSincronizacaoListasAuxiliar(_repository, _logger, _serviceBase, _mapper);
                var result = await pessoaSincronizacaoListaAuxiliar.SincronizarDocumentos(pessoa, false, model);

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    var resultSearch = (await Search(new SearchPadraoComListaIdsModel() { Id = result.First() })).AsList();
                    if (resultSearch != null && resultSearch.Any())
                        return resultSearch.First();
                }

                throw exception ?? new Exception($"Não foi possível salvar o documento da pessoa: ({model.PessoaId})");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível salvar o documento da pessoa: ({model.PessoaId})");
                _repository.Rollback();
                throw;
            }
        }


        public async Task<List<PessoaDocumentoModel>> SalvarLista(List<PessoaDocumentoInputModel> pessoaDocumentos)
        {
            try
            {
                _repository.BeginTransaction();

                var pessoa = (await _repository.FindBySql<Domain.Entities.Core.DadosPessoa.Pessoa>($"Select p.* From Pessoa p Where p.Id = {pessoaDocumentos.First().PessoaId.GetValueOrDefault()}")).FirstOrDefault() ?? throw new ArgumentException($"Não foi encontrada a Pessoa: {pessoaDocumentos.First().PessoaId}");

                var pessoaSincronizacaoListaAuxiliar = new PessoaSincronizacaoListasAuxiliar(_repository, _logger, _serviceBase, _mapper);
                var result = await pessoaSincronizacaoListaAuxiliar.SincronizarDocumentos(pessoa, false, pessoaDocumentos.ToArray());

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    var resultSearch = (await Search(new SearchPadraoComListaIdsModel() { Ids = result.AsList() })).AsList();
                    if (resultSearch != null && resultSearch.Any())
                        return resultSearch.AsList();
                }

                throw exception ?? new Exception($"Não foi possível salvar um ou mais documentos da pessoa: ({pessoaDocumentos.First().PessoaId})");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível salvar um ou mais documentos da pessoa: ({pessoaDocumentos.First().PessoaId})");
                _repository.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<PessoaDocumentoModel>?> Search(SearchPadraoComListaIdsModel searchModel)
        {
            List<Parameter> parameters = new();
            List<Parameter> parameters1 = new();
            StringBuilder sb = new("From PessoaDocumento ge Inner Join Fetch ge.Pessoa p Inner Join Fetch ge.TipoDocumento td Where 1 = 1");

            if (!string.IsNullOrEmpty(searchModel.Nome))
            {
                sb.AppendLine($" and ge.Nome = :nome");
                parameters.Add(new Parameter("nome", searchModel.Nome.ToLower()));
            }

            if (searchModel.PessoaId.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" and p.Id = {searchModel.PessoaId.GetValueOrDefault()} ");
            }

            if (searchModel.Id.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" and ge.Id = {searchModel.Id.GetValueOrDefault()} ");
            }

            if (searchModel.Ids != null && searchModel.Ids.Any())
            {
                sb.AppendLine($" and ge.Id in ({string.Join(",", searchModel.Ids).AsList()}) ");
            }

            var tipoDocumento = await _repository.FindByHql<PessoaDocumento>(sb.ToString(), parameters.ToArray());

            if (tipoDocumento.Any())
            {
                return await _serviceBase.SetUserName(tipoDocumento.Select(a => _mapper.Map<PessoaDocumentoModel>(a)).ToList());
            }

            return default;
        }

        public async Task<PessoaDocumentoModel> Update(PessoaDocumentoInputModel model)
        {
            if (model.Id.GetValueOrDefault(0) == 0)
                throw new ArgumentException("Dever ser informado o Id do documento a ser alterado");

            _repository.BeginTransaction();
            try
            {
                var pessoa = (await _repository.FindBySql<Domain.Entities.Core.DadosPessoa.Pessoa>($"Select p.* From Pessoa p Where p.Id = {model.PessoaId.GetValueOrDefault()}")).FirstOrDefault() ?? throw new ArgumentException($"Não foi encontrado a Pessoa: {model.PessoaId.GetValueOrDefault()}");

                var pessoaSincronizacaoListaAuxiliar = new PessoaSincronizacaoListasAuxiliar(_repository, _logger, _serviceBase, _mapper);
                var result = await pessoaSincronizacaoListaAuxiliar.SincronizarDocumentos(pessoa, false, model);

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    var resultSearch = (await Search(new SearchPadraoComListaIdsModel() { Id = result.First() })).AsList();
                    if (resultSearch != null && resultSearch.Any())
                        return resultSearch.First();
                }

                throw exception ?? new Exception("Erro na operação");
            }
            catch (Exception)
            {
                _repository.Rollback();
                throw;
            }

        }
    }
}
