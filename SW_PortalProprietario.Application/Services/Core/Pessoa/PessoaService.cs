using Dapper;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.PessoaModels;
using SW_PortalProprietario.Application.Services.Core.Auxiliar;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_Utils.Auxiliar;
using System.Text;

namespace SW_PortalProprietario.Application.Services.Core.Pessoa
{
    public class PessoaService : IPessoaService
    {
        private readonly IRepositoryNH _repository;
        private readonly ILogger<PessoaService> _logger;
        private readonly IServiceBase _serviceBase;
        private readonly IProjectObjectMapper _mapper;
        public PessoaService(IRepositoryNH repository,
            ILogger<PessoaService> logger,
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

                var pessoa = await _repository.FindById<Domain.Entities.Core.DadosPessoa.Pessoa>(id);
                if (pessoa is null)
                {
                    throw new ArgumentException($"Não foi encontrado pessoa com Id: {id}!");
                }


                _repository.BeginTransaction();
                _repository.Remove(pessoa);

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
                _logger.LogError(err, $"Não foi possível deletar a Pessoa: {id}");
                throw;
            }
        }

        public async Task<PessoaCompletaModel> SalvarPessoaFisica(PessoaFisicaInputModel pessoaFisica)
        {
            try
            {
                _repository.BeginTransaction();
                var pessoa = pessoaFisica.Id > 0 ? await _repository.FindById<Domain.Entities.Core.DadosPessoa.Pessoa>(pessoaFisica.Id.GetValueOrDefault()) :
                    _mapper.Map(pessoaFisica, new Domain.Entities.Core.DadosPessoa.Pessoa());

                var pessoaSalvar = _mapper.Map(pessoaFisica, pessoa);

                pessoaSalvar = await _repository.Save(pessoaSalvar);

                var pessoaSincronizacaoAuxiliar = new PessoaSincronizacaoListasAuxiliar(_repository, _logger, _serviceBase, _mapper);

                if (pessoa != null && pessoaFisica.Enderecos != null && pessoaFisica.Enderecos.Any())
                    await pessoaSincronizacaoAuxiliar.SincronizarEnderecos(pessoa, pessoaFisica.Enderecos.ToArray());

                if (pessoa != null && pessoaFisica.Telefones != null && pessoaFisica.Telefones.Any())
                    await pessoaSincronizacaoAuxiliar.SincronizarTelefones(pessoa, pessoaFisica.Telefones.ToArray());

                if (pessoa != null && pessoaFisica.Documentos != null && pessoaFisica.Documentos.Any())
                    await pessoaSincronizacaoAuxiliar.SincronizarDocumentos(pessoa, false, pessoaFisica.Documentos.ToArray());

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"Pessoa: ({pessoa.Id} - {pessoaFisica.Nome}) salva com sucesso!");

                    var resultRetorno = await Search(new PessoaSearchModel() { Id = pessoa.Id, CarregarCompleto = true });
                    if (resultRetorno != null && resultRetorno.Any())
                    {
                        return resultRetorno.First();
                    }
                }

                throw exception ?? new Exception($"Não foi possível salvar a Pessoa: ({pessoaFisica.Nome})");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível salvar a Pessoa: ({pessoaFisica.Nome})");
                _repository.Rollback();
                throw;
            }
        }

        public async Task<PessoaCompletaModel> SalvarPessoaJuridica(PessoaJuridicaInputModel pessoaJuridica)
        {
            try
            {
                _repository.BeginTransaction();
                var pessoa = pessoaJuridica.Id > 0 ? await _repository.FindById<Domain.Entities.Core.DadosPessoa.Pessoa>(pessoaJuridica.Id.GetValueOrDefault()) :
                    _mapper.Map(pessoaJuridica, new Domain.Entities.Core.DadosPessoa.Pessoa());

                var pessoaSalvar = _mapper.Map(pessoaJuridica, pessoa);

                pessoaSalvar = await _repository.Save(pessoaSalvar);

                var pessoaSincronizacaoAuxiliar = new PessoaSincronizacaoListasAuxiliar(_repository, _logger, _serviceBase, _mapper);

                if (pessoa != null && pessoaJuridica.Enderecos != null && pessoaJuridica.Enderecos.Any())
                    await pessoaSincronizacaoAuxiliar.SincronizarEnderecos(pessoa, pessoaJuridica.Enderecos.ToArray());

                if (pessoa != null && pessoaJuridica.Telefones != null && pessoaJuridica.Telefones.Any())
                    await pessoaSincronizacaoAuxiliar.SincronizarTelefones(pessoa, pessoaJuridica.Telefones.ToArray());

                if (pessoa != null && pessoaJuridica.Documentos != null && pessoaJuridica.Documentos.Any())
                    await pessoaSincronizacaoAuxiliar.SincronizarDocumentos(pessoa, false, pessoaJuridica.Documentos.ToArray());

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"Pessoa: ({pessoa.Id} - {pessoaJuridica.RazaoSocial}) salva com sucesso!");

                    var resultRetorno = await Search(new PessoaSearchModel() { Id = pessoa.Id, CarregarCompleto = true });
                    if (resultRetorno != null && resultRetorno.Any())
                    {
                        return resultRetorno.First();
                    }
                }

                throw exception ?? new Exception($"Não foi possível salvar a Pessoa: ({pessoaJuridica.RazaoSocial})");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível salvar a Pessoa: ({pessoaJuridica.RazaoSocial})");
                _repository.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<PessoaCompletaModel>?> Search(PessoaSearchModel searchModel)
        {
            List<Parameter> parameters = new();
            List<Parameter> parameters1 = new();
            StringBuilder sb = new("From Pessoa p Where 1 = 1");

            if (!string.IsNullOrEmpty(searchModel.Nome))
            {
                sb.AppendLine($" and Lower(p.Nome) like '{searchModel.Nome.ToLower().TrimEnd()}%'");
            }

            if (searchModel.Id.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine(" and p.Id = :id");
                parameters.Add(new Parameter("id", searchModel.Id.GetValueOrDefault()));
            }

            if (!string.IsNullOrEmpty(searchModel.Documento))
            {
                sb.AppendLine(" and exists(Select pd.Pessoa From PessoaDocumento pd Where pd.Numero = :documento and pd.Pessoa = p.Id) ");
                parameters.Add(new Parameter("documento", searchModel.Documento));
            }

            if (!string.IsNullOrEmpty(searchModel.Email))
            {
                sb.AppendLine(" and (Coalesce(Lower(p.EmailPreferencial),'') = :email or Coalesce(Lower(p.EmailAlternativo),'') = :email) ");
                parameters.Add(new Parameter("email", searchModel.Email.TrimEnd().ToLower()));
            }

            if (searchModel.Tipo.HasValue)
            {
                sb.AppendLine(" and p.TipoPessoa = :tipo");
                parameters.Add(new Parameter("tipo", searchModel.Tipo));
            }


            var pessoas = await _repository.FindByHql<Domain.Entities.Core.DadosPessoa.Pessoa>(sb.ToString(), session: null, parameters.ToArray());

            if (pessoas.Any())
            {
                if (!searchModel.CarregarCompleto.GetValueOrDefault(false))
                {
                    return await _serviceBase.SetUserName(pessoas.Select(a => _mapper.Map<PessoaCompletaModel>(a)).AsList());
                }
                else
                {
                    List<PessoaCompletaModel> listRetorno = pessoas.Select(a => _mapper.Map(a, new PessoaCompletaModel())).AsList();
                    foreach (var item in listRetorno)
                    {
                        item.Telefones = (await _repository.FindBySql<PessoaTelefoneModel>(@$"Select 
																							pt.Id,
																							pt.DataHoraCriacao,
																							pt.UsuarioCriacao,
																							pt.DataHoraAlteracao,
																							pt.UsuarioAlteracao,
																							pt.Pessoa as PessoaId, 
																							pt.TipoTelefone as TipoTelefoneId, 
																							tt.Nome as TipoTelefoneNome,
																							pt.Numero,
																						    tt.Mascara as TipoTelefoneMascara,
																							Coalesce(pt.Preferencial,0) AS Preferencial,
																						    pt.NumeroFormatado
																							From 
																							PessoaTelefone pt 
																							Inner Join TipoTelefone tt on pt.TipoTelefone = tt.Id
																							Where 
																							pt.Pessoa = {item.Id}")).AsList();


                        item.Enderecos = (await _repository.FindBySql<PessoaEnderecoModel>(@$"Select
																							pt.Id,
																							pt.UsuarioCriacao,
																						    pt.DataHoraCriacao,
																							pt.UsuarioAlteracao,
																							pt.DataHoraAlteracao,
																							pt.Pessoa as PessoaId,
																							c.Id as CidadeId,
																					        c.Nome as CidadeNome,
																							e.Id as EstadoId,
																							e.Sigla as EstadoSigla,
																							e.Nome as EstadoNome,
																							pt.TipoEndereco as TipoEnderecoId, 
																							tt.Nome as TipoEnderecoNome,
																							pt.Numero,
																							pt.Logradouro,
																							pt.Bairro,
																							pt.Complemento,
																							pt.Cep,
																							Coalesce(pt.Preferencial,0) AS Preferencial
																							From 
																							PessoaEndereco pt 
																							Inner Join Cidade c ON pt.Cidade = c.Id
																							Inner Join Estado e on c.Estado = e.Id
																							Inner Join TipoEndereco tt on pt.TipoEndereco = tt.Id
																							Where 
																							pt.Pessoa = {item.Id}")).AsList();


                        item.Documentos = (await _repository.FindBySql<PessoaDocumentoModel>(@$"Select
																							pd.Id,
																							pd.UsuarioCriacao,
																						    pd.DataHoraCriacao,
																							pd.UsuarioAlteracao,
																							pd.DataHoraAlteracao,
																							pd.Pessoa as PessoaId,
																							td.Id as TipoDocumentoId, 
																							td.Nome as TipoDocumentoNome,
																							pd.Numero,
																							pd.OrgaoEmissor,
																							pd.DataEmissao,
																							pd.DataValidade,
																							td.Mascara as TipoDocumentoMascara,
																							pd.NumeroFormatado
																							From 
																						    PessoaDocumento pd
																							Inner Join TipoDocumentoPessoa td on pd.TipoDocumento = td.Id
																							Where 
																							pd.Pessoa = {item.Id}")).AsList();
                    }

                    return await _serviceBase.SetUserName(listRetorno);

                }

            }

            return default;
        }


    }
}
