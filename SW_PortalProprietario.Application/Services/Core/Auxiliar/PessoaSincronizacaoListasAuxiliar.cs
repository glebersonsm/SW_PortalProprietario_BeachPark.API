using Dapper;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models.PessoaModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Enumns;
using SW_Utils.Functions;

namespace SW_PortalProprietario.Application.Services.Core.Auxiliar
{
    public class PessoaSincronizacaoListasAuxiliar
    {
        private readonly IRepositoryNH _repository;
        private readonly ILogger _logger;
        private readonly IServiceBase _serviceBase;
        private readonly IProjectObjectMapper _mapper;
        public PessoaSincronizacaoListasAuxiliar(IRepositoryNH repository,
            ILogger logger,
            IServiceBase serviceBase,
            IProjectObjectMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _serviceBase = serviceBase;
            _mapper = mapper;
        }


        public async Task<List<int>> SincronizarDocumentos(Domain.Entities.Core.DadosPessoa.Pessoa? pessoa, bool validarAlteracaoDocumento = false, params PessoaDocumentoInputModel[] documentos)
        {
            var companyConfiguration = await _serviceBase.GetParametroSistema();

            if (pessoa == null) throw new ArgumentNullException(nameof(pessoa));
            List<int> result = new List<int>();
            if (documentos != null)
            {
                foreach (var documento in documentos)
                {
                    if (string.IsNullOrEmpty(documento.Numero)) continue;
                    var apenasNumero = SW_Utils.Functions.Helper.ApenasNumeros(documento.Numero);
                    var tipoDocumento = (await _repository.FindBySql<TipoDocumentoPessoa>($"Select te.* From TipoDocumentoPessoa te Where te.Id =  {documento.TipoDocumentoId.GetValueOrDefault()}")).FirstOrDefault();
                    if (tipoDocumento == null)
                        throw new ArgumentException($"Não foi encontrado o tipo de documento informado: {documento.TipoDocumentoId.GetValueOrDefault()}");


                    if (tipoDocumento.ExigeDataEmissao.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim && documento.DataEmissao.GetValueOrDefault(DateTime.MinValue) == DateTime.MinValue)
                        throw new Exception($"O tipo de documento: {tipoDocumento.Nome} exige a informação da daa de emissão no documento: {documento.Numero}");

                    if (tipoDocumento.ExigeDataValidade.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim && documento.DataValidade.GetValueOrDefault(DateTime.MinValue) == DateTime.MinValue)
                        throw new Exception($"O tipo de documento: {tipoDocumento.Nome} exige a informação da data de validade no documento: {documento.Numero}");

                    if (tipoDocumento.ExigeOrgaoEmissor.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim && string.IsNullOrEmpty(documento.OrgaoEmissor))
                        throw new Exception($"O tipo de documento: {tipoDocumento.Nome} exige a informação do orgão emissão no documento: {documento.Numero}");

                    var documentoExistente = documento.Id.GetValueOrDefault(0) > 0 ?
                        (await _repository.FindByHql<PessoaDocumento>($"From PessoaDocumento pe Inner Join Fetch pe.TipoDocumento te Inner Join Fetch pe.Pessoa p Where pe.Id = {documento.Id.GetValueOrDefault()}")).FirstOrDefault() :
                        (await _repository.FindByHql<PessoaDocumento>($"From PessoaDocumento pe Inner Join Fetch pe.TipoDocumento te Inner Join Fetch pe.Pessoa p Where p.Id = {pessoa?.Id} and ((pe.Numero = '{documento.Numero?.TrimEnd()}' or pe.ValorNumerico = '{apenasNumero}') or te.Id = {tipoDocumento.Id})")).FirstOrDefault();


                    PessoaDocumento? pessoDocumentOld = null;

                    if (documentoExistente == null)
                    {
                        documentoExistente = _mapper.Map(documento, documentoExistente);
                        documentoExistente.Pessoa = pessoa;
                        documentoExistente.Numero = Helper.ApenasNumeros(documento.Numero);
                    }
                    else
                    {

                        if (validarAlteracaoDocumento && !_repository.IsAdm)
                        {
                            if (companyConfiguration != null && companyConfiguration.PermitirUsuarioAlterarSeuDoc.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Não)
                            {
                                if (documentoExistente.Numero != Helper.ApenasNumeros(documento.Numero))
                                    throw new ArgumentException("Alteração de documento não permitida");
                            }
                        }


                        pessoDocumentOld = await _serviceBase.GetObjectOld<PessoaDocumento>(documento.Id.GetValueOrDefault());

                        documentoExistente = _mapper.Map(documento, documentoExistente);
                        documentoExistente.Numero = Helper.ApenasNumeros(documento.Numero);
                        documentoExistente.Pessoa = pessoa;
                    }

                    documentoExistente.DataEmissao = tipoDocumento.ExigeDataEmissao.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim ? documento.DataEmissao.GetValueOrDefault() : null;
                    documentoExistente.DataValidade = tipoDocumento.ExigeDataValidade.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim ? documento.DataValidade.GetValueOrDefault() : null;
                    documentoExistente.OrgaoEmissor = tipoDocumento.ExigeOrgaoEmissor.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim ? documento.OrgaoEmissor : null;


                    documentoExistente.ValorNumerico = Helper.ApenasNumeros(documentoExistente.Numero);

                    if (!string.IsNullOrEmpty(tipoDocumento.Mascara))
                        documentoExistente.NumeroFormatado = Helper.Formatar(documentoExistente.ValorNumerico, tipoDocumento.Mascara);

                    var loggedUser = await _repository.GetLoggedUser();
                    if (string.IsNullOrEmpty(loggedUser.Value.userId))
                        await _repository.ForcedSave(documentoExistente);
                    else await _repository.Save(documentoExistente);

                    _serviceBase.Compare(pessoDocumentOld, documentoExistente);

                    result.Add(documentoExistente.Id);

                }
            }

            return result;
        }

        public async Task<List<int>> SincronizarTelefones(Domain.Entities.Core.DadosPessoa.Pessoa? pessoa, params PessoaTelefoneInputModel[] telefones)
        {
            if (pessoa == null) throw new ArgumentNullException(nameof(pessoa));
            List<int> result = new List<int>();
            if (telefones != null && telefones.Any())
            {
                foreach (var telefone in telefones)
                {
                    if (!telefone.Preferencial.HasValue)
                        telefone.Preferencial = EnumSimNao.Não;

                    var tipoTelefone = (await _repository.FindBySql<TipoTelefone>($"Select te.* From TipoTelefone te Where te.Id =  {telefone.TipoTelefoneId.GetValueOrDefault()}")).FirstOrDefault();
                    if (tipoTelefone == null)
                        throw new ArgumentException($"Não foi encontrado o tipo de telefone informado: {telefone.TipoTelefoneId.GetValueOrDefault()}");


                    var telefoneExistente = telefone.Id.GetValueOrDefault(0) > 0 ?
                        (await _repository.FindByHql<PessoaTelefone>($"From PessoaTelefone pe Inner Join Fetch pe.TipoTelefone te Inner Join Fetch pe.Pessoa p Where pe.Id = {telefone.Id.GetValueOrDefault()}")).FirstOrDefault() :
                        (await _repository.FindByHql<PessoaTelefone>($"From PessoaTelefone pe Inner Join Fetch pe.TipoTelefone te Inner Join Fetch pe.Pessoa p Where p.Id = {pessoa?.Id} and pe.Numero = '{telefone.Numero?.TrimEnd()}'")).FirstOrDefault();

                    PessoaTelefone? pessoaTelefoneOld = null;

                    if (telefoneExistente == null)
                    {

                        telefoneExistente = _mapper.Map<PessoaTelefoneInputModel, PessoaTelefone>(telefone, telefoneExistente);
                        telefoneExistente.Pessoa = pessoa;
                        telefoneExistente.Numero = Helper.ApenasNumeros(telefone.Numero);
                    }
                    else
                    {
                        pessoaTelefoneOld = await _serviceBase.GetObjectOld<PessoaTelefone>(telefoneExistente.Id);

                        telefoneExistente = _mapper.Map(telefone, telefoneExistente);
                        telefoneExistente.Numero = Helper.ApenasNumeros(telefone.Numero);
                        telefoneExistente.Pessoa = pessoa;
                    }


                    if (!string.IsNullOrEmpty(tipoTelefone.Mascara))
                        telefoneExistente.NumeroFormatado = Helper.Formatar(telefoneExistente.Numero, tipoTelefone.Mascara);

                    if (telefoneExistente.Preferencial == EnumSimNao.Sim && pessoa != null && pessoa.Id > 0)
                    {
                        var outrosTelefones = (await _repository.FindByHql<PessoaTelefone>($"From PessoaTelefone pt Inner Join Fetch pt.TipoTelefone tt Inner Join Fetch pt.Pessoa p Where p.Id = {pessoa.Id} and pt.Id <> {telefoneExistente.Id} and pt.Preferencial = 1")).AsList();
                        foreach (var item in outrosTelefones)
                        {
                            item.Preferencial = EnumSimNao.Não;
                            await _repository.Save(item);
                        }
                    }

                    var loggedUser = await _repository.GetLoggedUser();
                    if (string.IsNullOrEmpty(loggedUser.Value.userId))
                        await _repository.ForcedSave(telefoneExistente);
                    else await _repository.Save(telefoneExistente);

                    _serviceBase.Compare(pessoaTelefoneOld, telefoneExistente);

                    result.Add(telefoneExistente.Id);
                }
            }
            return result;
        }

        public async Task<List<int>> SincronizarEnderecos(Domain.Entities.Core.DadosPessoa.Pessoa? pessoa, params PessoaEnderecoInputModel[] enderecos)
        {
            if (pessoa == null) throw new ArgumentNullException(nameof(pessoa));

            List<int> result = new List<int>();
            if (enderecos != null && enderecos.Any())
            {
                foreach (var endereco in enderecos)
                {
                    if (!endereco.Preferencial.HasValue)
                        endereco.Preferencial = EnumSimNao.Não;

                    var tipoEndereco = (await _repository.FindBySql<TipoEndereco>($"Select te.* From TipoEndereco te Where te.Id =  {endereco.TipoEnderecoId.GetValueOrDefault()}")).FirstOrDefault();
                    if (tipoEndereco == null)
                        throw new ArgumentException($"Não foi encontrado o tipo de endereço informado: {endereco.TipoEnderecoId.GetValueOrDefault()}");

                    var cidade = (await _repository.FindByHql<Cidade>($"From Cidade c Inner Join Fetch c.Estado e Inner Join Fetch e.Pais p Where c.Id =  {endereco.CidadeId.GetValueOrDefault()}")).FirstOrDefault();
                    if (cidade == null)
                        throw new ArgumentException($"Não foi encontrada a Cidade informada: {endereco.CidadeId.GetValueOrDefault()}");

                    var enderecoExistente = endereco.Id.GetValueOrDefault(0) > 0 ?
                        (await _repository.FindByHql<PessoaEndereco>($"From PessoaEndereco pe Inner Join Fetch pe.TipoEndereco te Inner Join Fetch pe.Pessoa p Inner Join Fetch pe.Cidade cid Inner Join Fetch cid.Estado est Inner Join Fetch est.Pais pa Where pe.Id = {endereco.Id.GetValueOrDefault()}")).FirstOrDefault() :
                        (await _repository.FindByHql<PessoaEndereco>($"From PessoaEndereco pe Inner Join Fetch pe.TipoEndereco te Inner Join Fetch pe.Pessoa p Inner Join Fetch pe.Cidade cid Inner Join Fetch cid.Estado est Inner Join Fetch est.Pais pa Where p.Id = {pessoa?.Id} and Lower(pe.Logradouro) = '{endereco?.Logradouro?.TrimEnd().ToLower()}'")).FirstOrDefault();


                    PessoaEndereco? pessoaEnderecoOld = null;

                    if (enderecoExistente == null)
                    {
                        enderecoExistente = _mapper.Map(endereco, enderecoExistente);
                        enderecoExistente.Pessoa = pessoa;

                    }
                    else
                    {
                        pessoaEnderecoOld = await _serviceBase.GetObjectOld<PessoaEndereco>(enderecoExistente.Id);

                        enderecoExistente = _mapper.Map(endereco, enderecoExistente);
                        enderecoExistente.Pessoa = pessoa;

                    }

                    var loggedUser = await _repository.GetLoggedUser();
                    if (string.IsNullOrEmpty(loggedUser.Value.userId))
                        await _repository.ForcedSave(enderecoExistente);
                    else await _repository.Save(enderecoExistente);

                    _serviceBase.Compare(pessoaEnderecoOld, enderecoExistente);

                    result.Add(enderecoExistente.Id);
                }
            }
            return result;
        }

    }
}
