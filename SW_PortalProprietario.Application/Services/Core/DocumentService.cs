using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Functions;
using SW_Utils.Auxiliar;
using System.Text;

namespace SW_PortalProprietario.Application.Services.Core
{
    public class DocumentService : IDocumentService
    {
        private readonly IRepositoryNH _repository;
        private readonly ILogger<DocumentService> _logger;
        private readonly IProjectObjectMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IServiceBase _serviceBase;
        private readonly ICommunicationProvider _communicationProvider;

        public DocumentService(IRepositoryNH repository,
            ILogger<DocumentService> logger,
            IProjectObjectMapper mapper,
            IConfiguration configuration,
            IServiceBase serviceBase,
            ICommunicationProvider communicationProvider)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _configuration = configuration;
            _serviceBase = serviceBase;
            _communicationProvider = communicationProvider;
        }


        public async Task<DeleteResultModel> DeleteDocument(int id)
        {
            var result = new DeleteResultModel();
            result.Id = id;

            try
            {
                var loggedUser = (await _repository.GetLoggedUser());

                var document = (await _repository.FindByHql<Documento>($"From Documento d Where d.DataHoraRemocao is null and d.UsuarioRemocao is null and d.Id = {id}")).FirstOrDefault();
                if (document is null)
                {
                    throw new ArgumentException($"Não foi encontrado o documento com Id: {id}!");
                }

                _repository.BeginTransaction();

                var historicoDocumento = new HistoricoDocumento()
                {
                    Documento = document,
                    Acao = "Deletou",
                    Path = document.NomeArquivo ?? "N/A",
                    DataHoraRemocao = DateTime.Now,
                    UsuarioRemocao = loggedUser.HasValue && !string.IsNullOrEmpty(loggedUser.Value.userId) ? Convert.ToInt32(loggedUser.Value.userId) : null
                };

                await _repository.Save(historicoDocumento);

                var tagsRemover = (await _repository.FindByHql<DocumentoTags>($"From DocumentoTags dt Inner Join Fetch dt.Documento d Where d.Id = {document.Id}")).AsList();
                foreach (var tag in tagsRemover)
                {
                    tag.UsuarioRemocao = historicoDocumento.UsuarioRemocao;
                    tag.DataHoraRemocao = historicoDocumento.DataHoraRemocao;
                    await _repository.Save(tag);

                }

                var documentosHistoricos = (await _repository.FindByHql<HistoricoDocumento>($"From HistoricoDocumento hd Inner Join Fetch hd.Documento d Where d.Id = {document.Id}")).AsList();
                foreach (var historico in documentosHistoricos)
                {
                    historico.UsuarioRemocao = historicoDocumento.UsuarioRemocao;
                    historico.DataHoraRemocao = historicoDocumento.DataHoraRemocao;
                    await _repository.Save(historico);
                }

                document.DataHoraRemocao = DateTime.Now;
                document.UsuarioRemocao = historicoDocumento.UsuarioRemocao;


                await _repository.Save(document);


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
                _logger.LogError(err, $"Não foi possível deletar o documento: {id}");
                throw;
            }

        }

        public async Task<DocumentoModel> DownloadFile(int id)
        {
            try
            {
                _repository.BeginTransaction();

                Documento documento = null;
                if (id > 0)
                    documento = (await _repository.FindByHql<Documento>($"From Documento d Inner Join Fetch d.GrupoDocumento gd Left Join Fetch gd.Empresa emp Where d.Id = {id}")).FirstOrDefault();

                if (documento is null)
                    throw new ArgumentException($"Não foi encontrado o documento com Id: {id}");

                if (documento.Arquivo == null || documento.Arquivo.Length == 0)
                    throw new ArgumentException($"O arquivo do documento com Id: {id} não foi encontrado");

                // Garante que sempre haverá um TipoMime válido para o download
                if (string.IsNullOrWhiteSpace(documento.TipoMime))
                {
                    documento.TipoMime = "application/octet-stream";
                }

                // Garante um nome de arquivo válido
                if (string.IsNullOrWhiteSpace(documento.NomeArquivo))
                {
                    var extensao = ".bin";
                    if (documento.TipoMime.ToLower().Contains("pdf"))
                    {
                        extensao = ".pdf";
                    }

                    var baseName = string.IsNullOrWhiteSpace(documento.Nome) ? "documento" : documento.Nome;
                    documento.NomeArquivo = $"{baseName}_{id}{extensao}";
                }

                var historicoDocumento = new HistoricoDocumento()
                {
                    Documento = documento,
                    Acao = "Baixou",
                    Path = documento.NomeArquivo ?? "N/A"
                };

                await _repository.Save(historicoDocumento);


                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"Documento: ({documento.Id} - {documento.Nome}) baixado com sucesso!");
                }

                return _mapper.Map(documento, new DocumentoModel());

            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível baixar o documento com Id: ({id})");
                _repository.Rollback();
                throw;
            }
        }

        public async Task<DocumentoModel> SaveDocument(DocumentInputModel model)
        {
            try
            {
                _repository.BeginTransaction();

                if (model.Arquivo == null && model.Id.GetValueOrDefault(0) == 0)
                    throw new ArgumentException("Deve ser enviado um arquivo.");

                if (model.GrupoDocumentoId.GetValueOrDefault(0) == 0)
                    throw new ArgumentException("Deve ser informado o GrupoDocumentoId.");

                if (string.IsNullOrEmpty(model.Nome))
                    throw new ArgumentException("Deve ser informado o nome do documento");

                Documento documento = null;
                if (model.Id.GetValueOrDefault(0) > 0)
                {
                    documento = (await _repository.FindByHql<Documento>($"From Documento d Where d.Id = {model.Id}")).FirstOrDefault();
                }

                if (documento == null)
                {
                    documento = new Documento();
                }
                else
                {
                    // Se for edição e não houver novo arquivo, mantém o arquivo existente
                    if (model.Arquivo == null)
                    {
                        model.Arquivo = new MemoryFormFile(
                            documento.Arquivo ?? Array.Empty<byte>(),
                            documento.NomeArquivo ?? "document.pdf"
                        );
                    }
                }

                var grupoDocumento = (await _repository.FindByHql<GrupoDocumento>($"From GrupoDocumento gd Inner Join Fetch gd.Empresa e Where gd.Id = {model.GrupoDocumentoId}")).FirstOrDefault();
                if (grupoDocumento == null)
                    throw new ArgumentException($"Não foi encontrado o Grupo de Documento com o Id: {model.GrupoDocumentoId}.");

                documento.GrupoDocumento = grupoDocumento;
                documento.Nome = model.Nome;
                documento.DocumentoPublico = model.DocumentoPublico;
                documento.Disponivel = model.Disponivel;
                documento.DataInicioVigencia = model.DataInicioVigencia;
                documento.DataFimVigencia = model.DataFimVigencia;

                // Se for uma inclusão e não tiver ordem definida, definir ordem padrão dentro do grupo
                if (documento.Id == 0 && (model.Ordem == null || model.Ordem == 0))
                {
                    var maxOrdem = (await _repository.FindBySql<int?>($"Select Max(Ordem) From Documento Where UsuarioRemocao is null and DataHoraRemocao is null and GrupoDocumento = {model.GrupoDocumentoId.Value}")).FirstOrDefault();
                    documento.Ordem = (maxOrdem ?? 0) + 1;
                }
                else if (model.Ordem.HasValue)
                {
                    documento.Ordem = model.Ordem;
                }

                // Converter IFormFile para byte[] apenas se um novo arquivo foi enviado
                if (model.Arquivo != null)
                {
                    // Validar tamanho máximo de 10MB
                    long maxSizeBytes = 10 * 1024 * 1024; // 10MB
                    if (model.Arquivo.Length > maxSizeBytes)
                    {
                        throw new ArgumentException("O arquivo deve ter no máximo 10MB.");
                    }

                    // Validar tipo de arquivo
                    var extensao = Path.GetExtension(model.Arquivo.FileName).ToLower();
                    var tiposPermitidos = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png" };
                    if (!tiposPermitidos.Contains(extensao))
                    {
                        throw new ArgumentException($"Tipo de arquivo não suportado. Tipos permitidos: PDF, DOC, DOCX, XLS, XLSX");
                    }

                    var tipoMime = Functions.FileUtils.ObterTipoMIMEPorExtensao(extensao);
                    if (string.IsNullOrEmpty(tipoMime))
                        throw new Exception($"Tipo de arquivo: ({extensao}) não suportado.");

                    documento.TipoMime = tipoMime;
                    documento.NomeArquivo = model.Arquivo.FileName;

                    using (var memoryStream = new MemoryStream())
                    {
                        await model.Arquivo.CopyToAsync(memoryStream);
                        documento.Arquivo = memoryStream.ToArray();
                    }
                }

                // Garantir que NomeArquivo e TipoMime sejam sempre preenchidos (inclusive para documentos antigos)
                if (string.IsNullOrWhiteSpace(documento.NomeArquivo))
                {
                    var extensaoPadrao = ".pdf";
                    if (!string.IsNullOrWhiteSpace(documento.TipoMime) && !documento.TipoMime.ToLower().Contains("pdf"))
                    {
                        extensaoPadrao = ".bin";
                    }

                    var baseName = string.IsNullOrWhiteSpace(documento.Nome) ? "documento" : documento.Nome;
                    documento.NomeArquivo = $"{baseName}{extensaoPadrao}";
                }

                if (string.IsNullOrWhiteSpace(documento.TipoMime))
                {
                    var ext = Path.GetExtension(documento.NomeArquivo ?? string.Empty).ToLower();
                    var tipoMime = !string.IsNullOrEmpty(ext)
                        ? Functions.FileUtils.ObterTipoMIMEPorExtensao(ext)
                        : null;

                    documento.TipoMime = string.IsNullOrEmpty(tipoMime)
                        ? "application/octet-stream"
                        : tipoMime;
                }

                var result = await _repository.Save(documento);
                await SincronizarTagsRequeridas(result, model.TagsRequeridas ?? new List<int>(), model.RemoverTagsNaoEnviadas.GetValueOrDefault(false));

                var historicoDocumento = new HistoricoDocumento()
                {
                    Documento = documento,
                    Acao = "Salvou",
                    Path = documento.NomeArquivo ?? "N/A"
                };

                await _repository.Save(historicoDocumento);

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"Documento: ({result.Id} - {documento.Nome}) salvo com sucesso!");

                    if (result != null)
                    {
                        var searchResult = (await Search(new SearchPadraoModel() { Id = result.Id }));
                        if (searchResult != null && searchResult.Any())
                            return searchResult.First();
                    }
                }
                throw exception ?? new Exception($"Não foi possível salvar o Documento: ({documento.Nome})");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível salvar o Documento: ({model.Nome})");
                _repository.Rollback();
                throw;
            }
        }

        public async Task<DocumentoModel> UpdateDocument(AlteracaoDocumentInputModel model)
        {
            try
            {
                _repository.BeginTransaction();


                if (model.Id.GetValueOrDefault(0) == 0)
                    throw new ArgumentException("Deve ser informado Id do documento.");

                Documento documento = (await _repository.FindByHql<Documento>($"From Documento d Inner Join Fetch d.GrupoDocumento gd Inner Join Fetch gd.Empresa emp Where d.UsuarioRemocao is null and d.DataHoraRemocao is null and d.Id = {model.Id}")).FirstOrDefault();
                if (documento == null)
                    throw new ArgumentException($"Não foi encontrado o documenoto com Id: {model.Id}");

                documento.Nome = model.Nome;
                documento.DocumentoPublico = model.DocumentoPublico;
                documento.Disponivel = model.Disponivel;
                documento.DataInicioVigencia = model.DataInicioVigencia;
                documento.DataFimVigencia = model.DataFimVigencia;

                // Garantir NomeArquivo e TipoMime mesmo em alterações sem troca de arquivo
                if (string.IsNullOrWhiteSpace(documento.NomeArquivo))
                {
                    var extensaoPadrao = ".pdf";
                    if (!string.IsNullOrWhiteSpace(documento.TipoMime) && !documento.TipoMime.ToLower().Contains("pdf"))
                    {
                        extensaoPadrao = ".bin";
                    }

                    var baseName = string.IsNullOrWhiteSpace(documento.Nome) ? "documento" : documento.Nome;
                    documento.NomeArquivo = $"{baseName}_{documento.Id}{extensaoPadrao}";
                }

                if (string.IsNullOrWhiteSpace(documento.TipoMime))
                {
                    var ext = Path.GetExtension(documento.NomeArquivo ?? string.Empty).ToLower();
                    var tipoMime = !string.IsNullOrEmpty(ext)
                        ? Functions.FileUtils.ObterTipoMIMEPorExtensao(ext)
                        : null;

                    documento.TipoMime = string.IsNullOrEmpty(tipoMime)
                        ? "application/octet-stream"
                        : tipoMime;
                }

                var documentoSalvar = documento;

                var result = await _repository.Save(documentoSalvar);
                await SincronizarTagsRequeridas(result, model.TagsRequeridas ?? new List<int>(), model.RemoverTagsNaoEnviadas.GetValueOrDefault(false));

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"Documento: ({result.Id} - {documentoSalvar.Nome}) salvo com sucesso!");

                    if (result != null)
                        return (await Search(new SearchPadraoModel() { Id = result.Id })).FirstOrDefault();

                }
                throw exception ?? new Exception($"Não foi possível salvar o Documento: ({documentoSalvar.Nome})");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível salvar o Documento: ({model.Nome})");
                _repository.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<DocumentoModel>?> Search(SearchPadraoModel searchModel)
        {
            var adm = _repository.IsAdm;
            string userId = "0";

            if (!adm)
            {
                var loggedUser = await _repository.GetLoggedUser();
                if (loggedUser == null || string.IsNullOrEmpty(loggedUser.Value.providerKeyUser) || !loggedUser.Value.providerKeyUser.Contains("PessoaId", StringComparison.InvariantCultureIgnoreCase))
                    throw new ArgumentNullException("Não foi possível identificar o usuário para comunicação com o eSolution!");

                userId = loggedUser.Value.userId;
                if (string.IsNullOrEmpty(userId) || !Helper.IsNumeric(userId))
                    throw new ArgumentNullException("Não foi possível identificar o id do usuário logado.");

                var pessoaProvider = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(Convert.ToInt32(userId), _communicationProvider.CommunicationProviderName);
                if (pessoaProvider == null || string.IsNullOrEmpty(pessoaProvider.PessoaProvider))
                    throw new ArgumentNullException($"Não foi possível encontrar a pessoa do provider: {_communicationProvider.CommunicationProviderName} vinculada a pessoa: {loggedUser.Value.providerKeyUser}");
            }

            List<Parameter> parameters = new();
            List<Parameter> parameters1 = new();
            StringBuilder sb = new("From Documento d Inner Join Fetch d.GrupoDocumento gd Inner Join Fetch gd.Empresa emp Where d.DataHoraRemocao is null and d.UsuarioRemocao is null ");
            if (!string.IsNullOrEmpty(searchModel.Nome))
                sb.AppendLine($" and Lower(d.Nome) like '%{searchModel.Nome.ToLower()}%'");

            if (searchModel.Id.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" and d.Id = :id");
                parameters.Add(new Parameter("id", searchModel.Id.GetValueOrDefault()));
            }

            if (searchModel.Ids != null && searchModel.Ids.Any())
            {
                sb.AppendLine($" and d.Id in ({string.Join(",", searchModel.Ids.AsList())})");
            }

            if (searchModel.UsuarioCriacao.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" and d.UsuarioCriacao = {searchModel.UsuarioCriacao.GetValueOrDefault()}");
            }

            if (!adm)
            {
                sb.AppendLine(" and Coalesce(d.Disponivel,0) = 1 ");
                sb.AppendLine(@$" and (not exists(Select dt.Documento From DocumentoTags dt Where dt.UsuarioRemocao is null and dt.DataHoraRemocao is null and dt.Documento = d.Id) 
                                      or Exists(Select dt.Documento From DocumentoTags dt Inner Join UsuarioTags ut on dt.Tags = ut.Tags Where dt.UsuarioRemocao is null and dt.DataHoraRemocao is null and  dt.Documento = d.Id and ut.Usuario = {userId}))");
                sb.AppendLine(@$" and (not exists(Select gdt.GrupoDocumento From GrupoDocumentoTags gdt Where gdt.GrupoDocumento = gd.Id) 
                                      or Exists(Select gdt.GrupoDocumento From GrupoDocumentoTags gdt Inner Join UsuarioTags ut on gdt.Tags = ut.Tags Where gdt.GrupoDocumento = gd.Id and ut.Usuario = {userId}))");

            }

            sb.AppendLine(" Order By Coalesce(d.Ordem, 999999), d.Id ");
            var documentos = await _repository.FindByHql<Documento>(sb.ToString(), parameters.ToArray());

            var listDocumentosRetorno = documentos.Select(a => _mapper.Map(a, new DocumentoModel())).ToList();

            if (listDocumentosRetorno != null && listDocumentosRetorno.Any())
            {

                var tagsDoDocumento = (await _repository.FindByHql<DocumentoTags>($"From DocumentoTags dt Inner Join Fetch dt.Documento d Inner Join Fetch dt.Tags t Where dt.UsuarioRemocao is null and dt.DataHoraRemocao is null and d.Id in ({string.Join(",", listDocumentosRetorno.Select(a => a.Id).AsList())})")).AsList();
                foreach (var item in listDocumentosRetorno)
                {
                    item.Arquivo = null;
                    var tagsRelacionadas = tagsDoDocumento.Where(b => b.Documento.Id == item.Id).AsList();
                    if (tagsRelacionadas.Any())
                    {
                        item.TagsRequeridas = tagsRelacionadas.Select(b => new DocumentoTagsModel()
                        {
                            DocumentoId = b.Documento.Id,
                            Id = b.Id,
                            Tags = _mapper.Map(b.Tags, new TagsModel())
                        }).AsList();
                    }
                }
            }

            return await _serviceBase.SetUserName(listDocumentosRetorno);
        }

        public async Task<bool> ReorderDocuments(List<ReorderDocumentModel> documents)
        {
            try
            {
                _repository.BeginTransaction();

                foreach (var document in documents)
                {
                    var documento = await _repository.FindById<Documento>(document.Id);
                    if (documento != null && documento.UsuarioRemocao == null && documento.DataHoraRemocao == null)
                    {
                        documento.Ordem = document.Ordem;
                        await _repository.Save(documento);
                    }
                }

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"Ordem dos documentos atualizada com sucesso!");
                    return true;
                }
                throw exception ?? new Exception("Não foi possível atualizar a ordem dos documentos");
            }
            catch (Exception err)
            {
                _logger.LogError(err, "Erro ao atualizar ordem dos documentos");
                _repository.Rollback();
                throw;
            }
        }

        private async Task SincronizarTagsRequeridas(Documento documento, List<int> listTags, bool removerTagsNaoEnviadas = false)
        {
            if (removerTagsNaoEnviadas)
            {
                if (listTags == null || listTags.Count == 0)
                {
                    await _repository.ExecuteSqlCommand($"Delete From DocumentoTags Where Documento = {documento.Id}");
                    return;
                }
                else
                {
                    await _repository.ExecuteSqlCommand($"Delete From DocumentoTags Where Documento = {documento.Id} and tags not in ({string.Join(",", listTags)})");
                }
            }

            if (listTags != null && listTags.Any())
            {
                var allTags = (await _repository.FindBySql<TagsModel>($"Select t.Id From Tags t Where t.Id in ({string.Join(",", listTags)})")).AsList();
                var tagsInexistentes = listTags.Where(c => !allTags.Any(b => b.Id == c)).AsList();
                if (tagsInexistentes.Count > 0)
                {
                    throw new ArgumentException($"Tags não encontradas: {string.Join(",", tagsInexistentes)}");
                }

                var tags = (await _repository.FindBySql<TagsModel>($"Select t.Id From Tags t Where t.Id in ({string.Join(",", listTags)}) and Not Exists(Select dc.Tags From DocumentoTags dc Where dc.Documento = {documento.Id} and dc.Tags = t.Id and dc.UsuarioRemocao is null and dc.DataHoraRemocao is null)")).AsList();

                foreach (var t in tags)
                {
                    var documentoTags = new DocumentoTags()
                    {
                        Documento = documento,
                        Tags = new Tags() { Id = t.Id.GetValueOrDefault(0) }
                    };

                    await _repository.Save(documentoTags);
                }
            }

        }

        public async Task<IEnumerable<DocumentoHistoricoModel>?> History(int id)
        {
            var documentHistory = (await _repository.FindBySql<DocumentoHistoricoModel>(@$"Select 
                gd.Id as GrupoDocumentoId,
                gd.Nome as GrupoDocumentoNome,
                d.Id as DocumentoId,
                d.Nome as DocumentoNome,
                hd.Acao as AcaoRealizada,
                hd.UsuarioCriacao as UsuarioId,
                hd.DataHoraCriacao as DataOperacao,
                p.Nome as NomeUsuario
                From 
                HistoricoDocumento hd 
                Inner Join Documento d on hd.Documento = d.Id
                Inner Join GrupoDocumento gd on d.GrupoDocumento = gd.Id
                Inner Join Usuario u on hd.UsuarioCriacao = u.Id
                Inner Join Pessoa p on u.Pessoa = p.Id
                Where 
                d.Id = {id} and hd.UsuarioRemocao is null and hd.DataHoraRemocao is null and d.UsuarioRemocao is null and d.DataHoraRemocao is null")).AsList();

            return documentHistory;
        }
    }

    public class MemoryFormFile : IFormFile
    {
        private readonly byte[] _content;
        private readonly string _fileName;

        public MemoryFormFile(byte[] content, string fileName)
        {
            _content = content;
            _fileName = fileName;
        }

        public string ContentType => "application/octet-stream";
        public string ContentDisposition => $"inline; filename={_fileName}";
        public IHeaderDictionary Headers => new HeaderDictionary();
        public long Length => _content.Length;
        public string Name => "Arquivo";
        public string FileName => _fileName;

        public Stream OpenReadStream() => new MemoryStream(_content);
        public void CopyTo(Stream target) => new MemoryStream(_content).CopyTo(target);
        public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default) =>
            new MemoryStream(_content).CopyToAsync(target, cancellationToken);
    }

    // If you do not have a HeaderDictionary implementation, you can use this minimal stub:
    public class HeaderDictionary : Dictionary<string, StringValues>, IHeaderDictionary
    {
        public long? ContentLength { get; set; }
    }
}
