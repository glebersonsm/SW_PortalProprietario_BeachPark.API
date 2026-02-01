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
    public class PessoaTelefoneService : IPessoaTelefoneService
    {
        private readonly IRepositoryNH _repository;
        private readonly ILogger<PessoaTelefoneService> _logger;
        private readonly IServiceBase _serviceBase;
        private readonly IProjectObjectMapper _mapper;
        public PessoaTelefoneService(IRepositoryNH repository,
            ILogger<PessoaTelefoneService> logger,
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

                var pessoaTelefone = await _repository.FindById<PessoaTelefone>(id);
                if (pessoaTelefone is null)
                {
                    throw new FileNotFoundException($"Não foi encontrado o telefone com Id: {id}!");
                }


                _repository.BeginTransaction();
                await _repository.Remove(pessoaTelefone);

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
                _logger.LogError(err, $"Não foi possível deletar o telefone: {id}");
                throw;
            }
        }

        public async Task<PessoaTelefoneModel> Salvar(PessoaTelefoneInputModel pessoaTelefone)
        {
            try
            {
                _repository.BeginTransaction();

                var pessoa = (await _repository.FindBySql<Domain.Entities.Core.DadosPessoa.Pessoa>($"Select * From Pessoa Where Id = {pessoaTelefone.PessoaId}")).FirstOrDefault() ?? throw new ArgumentException($"Não foi encontrada a pessoa: {pessoaTelefone.PessoaId}");

                var pessoaSincronizacaoListaAuxliar = new PessoaSincronizacaoListasAuxiliar(_repository, _logger, _serviceBase, _mapper);
                var resultSave = await pessoaSincronizacaoListaAuxliar.SincronizarTelefones(pessoa, pessoaTelefone);

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    var resultSearch = (await Search(new SearchPadraoComListaIdsModel() { Id = resultSave.First() })).AsList();
                    if (resultSearch != null && resultSearch.Any())
                        return resultSearch.First();

                }

                throw exception ?? new Exception($"Não foi possível salvar o Tipo Telefone: ({pessoaTelefone.PessoaId})");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível salvar o Tipo Telefone: ({pessoaTelefone.PessoaId})");
                _repository.Rollback();
                throw;
            }
        }

        public async Task<List<PessoaTelefoneModel>> SalvarLista(List<PessoaTelefoneInputModel> pessoaTelefones)
        {
            try
            {
                List<int> telefonesIds = new List<int>();
                _repository.BeginTransaction();
                var pessoaSincronizacaoListaAuxliar = new PessoaSincronizacaoListasAuxiliar(_repository, _logger, _serviceBase, _mapper);

                if (pessoaTelefones.Any(c => c.PessoaId.GetValueOrDefault(0) == 0))
                    throw new ArgumentException($"Deve ser informada a Pessoa no Telefone!");

                var pessoa = (await _repository.FindBySql<Domain.Entities.Core.DadosPessoa.Pessoa>($"Select * From Pessoa Where Id = {pessoaTelefones.First().PessoaId}")).FirstOrDefault() ??
                    throw new ArgumentException($"Não foi encontrada a pessoa: {pessoaTelefones.First().PessoaId}");

                var resultSave = await pessoaSincronizacaoListaAuxliar.SincronizarTelefones(pessoa, pessoaTelefones.ToArray());
                telefonesIds = resultSave ?? new List<int>();


                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    var resultSearch = (await Search(new SearchPadraoComListaIdsModel() { Ids = telefonesIds })).AsList();
                    if (resultSearch != null && resultSearch.Any())
                        return resultSearch.AsList();

                }

                throw exception ?? new Exception($"Não foi possível salvar o(s) telefone(s)");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível salvar o(s) Telefone(s)");
                _repository.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<PessoaTelefoneModel>?> Search(SearchPadraoComListaIdsModel searchModel)
        {
            List<Parameter> parameters = new();
            List<Parameter> parameters1 = new();
            StringBuilder sb = new("From PessoaTelefone ge Inner Join Fetch ge.Pessoa p Inner Join Fetch ge.TipoTelefone tt Where 1 = 1");

            if (!string.IsNullOrEmpty(searchModel.Nome))
            {
                sb.AppendLine($" and ge.Nome = :nome");
                parameters.Add(new Parameter("nome", searchModel.Nome.ToLower()));
            }

            if (searchModel.Id.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" and ge.Id = {searchModel.Id.GetValueOrDefault()} ");
            }

            if (searchModel.Ids != null && searchModel.Ids.Count > 0)
            {
                sb.AppendLine($" and ge.Id in ({string.Join(",", searchModel.Ids)}) ");
            }

            if (searchModel.PessoaId != null && searchModel.PessoaId.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" and p.Id = {searchModel.PessoaId} ");
            }

            if (searchModel.UsuarioCriacao.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" and ge.UsuarioCriacao = {searchModel.UsuarioCriacao.GetValueOrDefault()}");
            }

            var tipoTelefone = await _repository.FindByHql<PessoaTelefone>(sb.ToString(), parameters.ToArray());

            if (tipoTelefone.Any())
                return await _serviceBase.SetUserName(tipoTelefone.Select(a => _mapper.Map<PessoaTelefoneModel>(a)).AsList());

            return default;
        }

        public async Task<PessoaTelefoneModel> Update(PessoaTelefoneInputModel model)
        {
            _repository.BeginTransaction();
            try
            {
                var pessoa = (await _repository.FindBySql<Domain.Entities.Core.DadosPessoa.Pessoa>($"Select * From Pessoa Where Id = {model.PessoaId}")).FirstOrDefault() ?? throw new ArgumentException($"Não foi encontrada a pessoa: {model.PessoaId}");

                var pessoaSincronizacaoListaAuxliar = new PessoaSincronizacaoListasAuxiliar(_repository, _logger, _serviceBase, _mapper);
                var resultSave = await pessoaSincronizacaoListaAuxliar.SincronizarTelefones(pessoa, model);

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    var resultSearch = (await Search(new SearchPadraoComListaIdsModel() { Id = resultSave.First() })).AsList();
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
