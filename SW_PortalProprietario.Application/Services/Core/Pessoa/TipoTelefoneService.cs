using Dapper;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.PessoaModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;
using SW_Utils.Auxiliar;
using System.Text;

namespace SW_PortalProprietario.Application.Services.Core.Pessoa
{
    public class TipoTelefoneService : ITipoTelefoneService
    {
        private readonly IRepositoryNH _repository;
        private readonly ILogger<TipoTelefoneService> _logger;
        private readonly IServiceBase _serviceBase;
        public TipoTelefoneService(IRepositoryNH repository,
            ILogger<TipoTelefoneService> logger,
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

                var tipoTelefone = await _repository.FindById<TipoTelefone>(id);
                if (tipoTelefone is null)
                {
                    throw new FileNotFoundException($"Não foi encontrado tipo de telefone com Id: {id}!");
                }


                _repository.BeginTransaction();
                await _repository.Remove(tipoTelefone);

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
                _logger.LogError(err, $"Não foi possível deletar o Tipo de Telefone: {id}");
                throw;
            }
        }

        public async Task<TipoTelefoneModel> Salvar(TipoTelefoneInputModel tipoTelefone)
        {
            try
            {
                _repository.BeginTransaction();
                var are = (TipoTelefone)tipoTelefone;

                var result = await _repository.Save(are);

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"Telefone: ({result.Id} - {tipoTelefone.Nome}) salvo com sucesso!");

                    if (result != null)
                        return (TipoTelefoneModel)result;

                }

                throw exception ?? new Exception($"Não foi possível salvar o tipo telefone: ({tipoTelefone.Nome})");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível salvar o tipo telefone: ({tipoTelefone.Nome})");
                _repository.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<TipoTelefoneModel>?> Search(SearchPadraoModel searchModel)
        {
            List<Parameter> parameters = new();
            List<Parameter> parameters1 = new();
            StringBuilder sb = new("From TipoTelefone ge Where 1 = 1");

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

            var tipoTelefone = await _repository.FindByHql<TipoTelefone>(sb.ToString(), parameters.ToArray());

            if (tipoTelefone.Any())
                return await _serviceBase.SetUserName(tipoTelefone.Select(a => (TipoTelefoneModel)a).AsList());

            return default;
        }

        public async Task<TipoTelefoneModel> Update(TipoTelefoneInputModel model)
        {
            _repository.BeginTransaction();
            try
            {
                var tipotelefone = (await _repository.FindByHql<TipoTelefone>("From TipoTelefone ge Where ge.Id = :id", new Parameter[]
                { new Parameter("id", model.Id) })).FirstOrDefault() ?? throw new Exception($"Não foi encontrado o Tipo Telefone: {model.Id}");

                tipotelefone.Nome = model.Nome ?? tipotelefone.Nome;
                tipotelefone.Mascara = model.Mascara ?? tipotelefone.Mascara;
                await _repository.Save(tipotelefone);
                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                    return (TipoTelefoneModel)tipotelefone;
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
