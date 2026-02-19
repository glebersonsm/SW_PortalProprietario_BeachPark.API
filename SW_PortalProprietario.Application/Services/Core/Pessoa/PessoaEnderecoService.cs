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
    public class PessoaEnderecoService : IPessoaEnderecoService
    {
        private readonly IRepositoryNH _repository;
        private readonly ILogger<PessoaEnderecoService> _logger;
        private readonly IServiceBase _serviceBase;
        private readonly IProjectObjectMapper _mapper;
        public PessoaEnderecoService(IRepositoryNH repository,
            ILogger<PessoaEnderecoService> logger,
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

                var pessoaEndereco = await _repository.FindById<PessoaEndereco>(id);
                if (pessoaEndereco is null)
                {
                    throw new FileNotFoundException($"NÃ£o foi encontrado o endereÃ§o com Id: {id}!");
                }


                _repository.BeginTransaction();
                await _repository.Remove(pessoaEndereco);

                var resultCommit = await _repository.CommitAsync();
                if (resultCommit.executed)
                {
                    result.Result = "Removido com sucesso!";
                }
                else
                {
                    throw resultCommit.exception ?? new Exception("NÃ£o foi possÃ­vel realizar a operaÃ§Ã£o");
                }

                return result;

            }
            catch (Exception err)
            {
                _repository.Rollback();
                _logger.LogError(err, $"NÃ£o foi possÃ­vel deletar o Tipo de endereco: {id}");
                throw;
            }

        }

        public async Task<PessoaEnderecoModel> Salvar(PessoaEnderecoInputModel pessoaEndereco)
        {
            try
            {
                _repository.BeginTransaction();
                var pessoa = (await _repository.FindBySql<Domain.Entities.Core.DadosPessoa.Pessoa>($"Select p.* From Pessoa p Where p.Id = {pessoaEndereco.PessoaId.GetValueOrDefault()}")).FirstOrDefault() ?? throw new ArgumentException($"NÃ£o foi encontrado a Pessoa: {pessoaEndereco.PessoaId.GetValueOrDefault()}");
                var pessoaSincronizacaoListasAuxiliar = new PessoaSincronizacaoListasAuxiliar(_repository, _logger, _serviceBase, _mapper);
                var resultSave = await pessoaSincronizacaoListasAuxiliar.SincronizarEnderecos(pessoa, pessoaEndereco);


                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    var resultSearch = (await Search(new SearchPadraoComListaIdsModel() { Id = resultSave.First() })).AsList();
                    if (resultSearch != null && resultSearch.Any())
                        return resultSearch.First();
                }

                throw exception ?? new Exception($"NÃ£o foi possÃ­vel salvar o Tipo Endereco: ({pessoaEndereco.PessoaId})");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"NÃ£o foi possÃ­vel salvar o Tipo Endereco: ({pessoaEndereco.PessoaId})");
                _repository.Rollback();
                throw;
            }
        }

        public async Task<List<PessoaEnderecoModel>> SalvarLista(List<PessoaEnderecoInputModel> pessoaEnderecos)
        {
            try
            {
                List<int> enderecosIds = new List<int>();
                _repository.BeginTransaction();
                var pessoa = (await _repository.FindBySql<Domain.Entities.Core.DadosPessoa.Pessoa>($"Select p.* From Pessoa p Where p.Id = {pessoaEnderecos.First().PessoaId.GetValueOrDefault()}")).FirstOrDefault() ?? throw new ArgumentException($"NÃ£o foi encontrado a Pessoa: {pessoaEnderecos.First().PessoaId.GetValueOrDefault()}");
                var pessoaSincronizacaoListasAuxiliar = new PessoaSincronizacaoListasAuxiliar(_repository, _logger, _serviceBase, _mapper);
                var resultSave = await pessoaSincronizacaoListasAuxiliar.SincronizarEnderecos(pessoa, pessoaEnderecos.ToArray());
                enderecosIds = resultSave ?? new List<int>();

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    var resultSearch = (await Search(new SearchPadraoComListaIdsModel() { Ids = enderecosIds })).AsList();
                    if (resultSearch != null && resultSearch.Any())
                        return resultSearch.AsList();
                }

                throw exception ?? new Exception($"NÃ£o foi possÃ­vel salvar os Endereco(s)");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"NÃ£o foi possÃ­vel salvar os Endereco(s)");
                _repository.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<PessoaEnderecoModel>?> Search(SearchPadraoComListaIdsModel searchModel)
        {
            List<Parameter> parameters = new();
            List<Parameter> parameters1 = new();
            StringBuilder sb = new("From PessoaEndereco ge Inner Join Fetch ge.Pessoa p Inner Join Fetch ge.TipoEndereco te Left Join Fetch ge.Cidade cid Left Outer Join Fetch cid.Estado est Left Join Fetch est.Pais pa Where 1 = 1");

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
                sb.AppendLine($" and ge.Id in ({string.Join(",", searchModel.Ids)}) ");
            }

            if (searchModel.UsuarioCriacao.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" and ge.UsuarioCriacao = {searchModel.UsuarioCriacao.GetValueOrDefault()}");
            }

            var tipoEndereco = await _repository.FindByHql<PessoaEndereco>(sb.ToString(), session: null, parameters.ToArray());

            if (tipoEndereco.Any())
            {
                return await _serviceBase.SetUserName(tipoEndereco.Select(a => _mapper.Map<PessoaEnderecoModel>(a)).ToList());
            }

            return default;
        }

        public async Task<PessoaEnderecoModel> Update(PessoaEnderecoInputModel model)
        {
            _repository.BeginTransaction();
            try
            {

                var pessoa = (await _repository.FindBySql<Domain.Entities.Core.DadosPessoa.Pessoa>($"Select p.* From Pessoa p Where p.Id = {model.PessoaId.GetValueOrDefault()}")).FirstOrDefault() ?? throw new ArgumentException($"NÃ£o foi encontrado a Pessoa: {model.PessoaId.GetValueOrDefault()}");
                var pessoaSincronizacaoListasAuxiliar = new PessoaSincronizacaoListasAuxiliar(_repository, _logger, _serviceBase, _mapper);
                var resultSave = await pessoaSincronizacaoListasAuxiliar.SincronizarEnderecos(pessoa, model);


                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    var resultSearch = (await Search(new SearchPadraoComListaIdsModel() { Id = resultSave.First() })).AsList();
                    if (resultSearch != null && resultSearch.Any())
                        return resultSearch.First();
                }

                throw exception ?? new Exception("Erro na operaÃ§Ã£o");
            }
            catch (Exception)
            {
                _repository.Rollback();
                throw;
            }

        }
    }
}
