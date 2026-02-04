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
    public class TipoEnderecoService : ITipoEnderecoService
    {
        private readonly IRepositoryNH _repository;
        private readonly ILogger<TipoEnderecoService> _logger;
        private readonly IServiceBase _serviceBase;
        public TipoEnderecoService(IRepositoryNH repository,
            ILogger<TipoEnderecoService> logger,
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

                var tipoEndereco = await _repository.FindById<TipoEndereco>(id);
                if (tipoEndereco is null)
                {
                    throw new FileNotFoundException($"Não foi encontrado o tipo de endereco com Id: {id}!");
                }


                _repository.BeginTransaction();
                _repository.Remove(tipoEndereco);

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
                _logger.LogError(err, $"Não foi possível deletar o Tipo de Endereço: {id}");
                throw;

            }
        }

        public async Task<TipoEnderecoModel> Salvar(TipoEnderecoInputModel tipoEndereco)
        {
            try
            {
                _repository.BeginTransaction();
                var are = (TipoEndereco)tipoEndereco;

                var result = await _repository.Save(are);

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"Endereço: ({result.Id} - {tipoEndereco.Nome}) salvo com sucesso!");

                    if (result != null)
                        return (TipoEnderecoModel)result;

                }

                throw exception ?? new Exception($"Não foi possível salvar o tipo Endereço: ({tipoEndereco.Nome})");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível salvar o tipo Endereço: ({tipoEndereco.Nome})");
                _repository.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<TipoEnderecoModel>?> Search(SearchPadraoModel searchModel)
        {
            List<Parameter> parameters = new();
            List<Parameter> parameters1 = new();
            StringBuilder sb = new("From TipoEndereco ge Where 1 = 1");

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

            var tipoEndereco = await _repository.FindByHql<TipoEndereco>(sb.ToString(), session: null, parameters.ToArray());

            if (tipoEndereco.Any())
                return await _serviceBase.SetUserName(tipoEndereco.Select(a => (TipoEnderecoModel)a).AsList());

            return default;
        }

        public async Task<TipoEnderecoModel> Update(TipoEnderecoInputModel model)
        {
            _repository.BeginTransaction();
            try
            {
                var tipoendereco = (await _repository.FindByHql<TipoEndereco>("From TipoEndereco ge Where ge.Id = :id", session: null, new Parameter[]
                { new Parameter("id", model.Id) })).FirstOrDefault() ?? throw new Exception($"Não foi encontrado o Tipo Endereço: {model.Id}");

                tipoendereco.Nome = model.Nome ?? tipoendereco.Nome;
                await _repository.Save(tipoendereco);
                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                    return (TipoEnderecoModel)tipoendereco;
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
