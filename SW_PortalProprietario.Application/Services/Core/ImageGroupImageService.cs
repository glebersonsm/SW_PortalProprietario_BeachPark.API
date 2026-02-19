using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
    public class ImageGroupImageService : IImageGroupImageService
    {
        private readonly IRepositoryNH _repository;
        private readonly ILogger<ImageGroupImageService> _logger;
        private readonly IProjectObjectMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IServiceBase _serviceBase;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICommunicationProvider _communicationProvider;

        public ImageGroupImageService(IRepositoryNH repository,
            ILogger<ImageGroupImageService> logger,
            IProjectObjectMapper mapper,
            IConfiguration configuration,
            IServiceBase serviceBase,
            IHttpContextAccessor httpContextAccessor,
            ICommunicationProvider communicationProvider)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _configuration = configuration;
            _serviceBase = serviceBase;
            _httpContextAccessor = httpContextAccessor;
            _communicationProvider = communicationProvider;
        }

        public async Task<ImageGroupImageModel> SaveImage(ImageGroupImageInputModel model)
        {
            try
            {
                _repository.BeginTransaction();

                if (model.Imagem == null && model.Id.GetValueOrDefault(0) == 0)
                    throw new ArgumentException("Deve ser enviada uma imagem.");

                if (model.ImageGroupId.GetValueOrDefault(0) == 0)
                    throw new ArgumentException("Deve ser informado o ImageGroupId.");

                if (string.IsNullOrEmpty(model.Name))
                    throw new ArgumentException("Deve ser informado o nome da imagem");

                ImagemGrupoImagem? imageGroupImage = null;
                if (model.Id.GetValueOrDefault(0) > 0)
                {
                    imageGroupImage = (await _repository.FindByHql<ImagemGrupoImagem>($"From ImagemGrupoImagem i Where i.Id = {model.Id}")).FirstOrDefault();
                }

                var imageGroup = (await _repository.FindByHql<GrupoImagem>($"From GrupoImagem gi Inner Join Fetch gi.Empresa e Where gi.Id = {model.ImageGroupId}")).FirstOrDefault();
                if (imageGroup == null)
                    throw new ArgumentException($"NÃ£o foi encontrado o Grupo de Imagem com o Id: {model.ImageGroupId}.");

                if (imageGroupImage == null)
                {
                    imageGroupImage = new ImagemGrupoImagem();
                }

                imageGroupImage.GrupoImagem = imageGroup;
                imageGroupImage.Nome = model.Name;
                imageGroupImage.NomeBotao = model.NomeBotao;
                imageGroupImage.LinkBotao = model.LinkBotao;
                imageGroupImage.DataInicioVigencia = model.DataInicioVigencia;
                imageGroupImage.DataFimVigencia = model.DataFimVigencia;

                // Definir Ordem padrÃ£o se for uma nova imagem e Ordem nÃ£o foi informada
                if (model.Id.GetValueOrDefault(0) == 0 && (model.Ordem == null || model.Ordem == 0))
                {
                    var maxOrdem = await _repository.CountTotalEntry($"Select Max(i.Ordem) as Ordem From ImagemGrupoImagem i Where i.GrupoImagem = {model.ImageGroupId}", session: null);
                    imageGroupImage.Ordem = Convert.ToInt32(maxOrdem) + 1;
                }

                // Converter IFormFile para byte[] apenas se uma nova imagem foi enviada
                if (model.Imagem != null && model.Imagem.Length > 0)
                {
                    // Validar tamanho mÃ¡ximo de 2MB
                    long maxSizeBytes = 2 * 1024 * 1024; // 2MB
                    if (model.Imagem.Length > maxSizeBytes)
                    {
                        throw new ArgumentException("A imagem deve ter no mÃ¡ximo 2MB.");
                    }

                    using (var memoryStream = new MemoryStream())
                    {
                        await model.Imagem.CopyToAsync(memoryStream);
                        imageGroupImage.Imagem = memoryStream.ToArray();
                    }

                    // Validar tipo de arquivo
                    var ext = Functions.FileUtils.ObterTipoMIMEImagePorExtensao(Path.GetExtension(model.Imagem.FileName));
                    if (string.IsNullOrEmpty(ext))
                        throw new Exception($"Tipo de arquivo: ({Path.GetExtension(model.Imagem.FileName)}) nÃ£o suportado.");
                }
                // Se for ediÃ§Ã£o e nÃ£o houver nova imagem, mantÃ©m a imagem existente

                var result = await _repository.Save(imageGroupImage);
                await SincronizarTagsRequeridas(imageGroupImage, model.TagsRequeridas, model.RemoverTagsNaoEnviadas ?? false);

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"Imagem: ({result.Id} - {imageGroupImage.Nome}) salva com sucesso!");

                    if (result != null)
                    {
                        var searchResult = (await Search(new SearchImageGroupImageModel() { Id = result.Id }));
                        if (searchResult != null && searchResult.Any())
                            return searchResult.First();
                    }

                }
                throw exception ?? new Exception($"NÃ£o foi possÃ­vel salvar a Imagem: ({imageGroupImage.Nome})");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"NÃ£o foi possÃ­vel salvar a Imagem: ({model.Name})");
                _repository.Rollback();
                throw;
            }
        }

        private async Task SincronizarTagsRequeridas(ImagemGrupoImagem imagemGrupoImagem, List<int> listTags, bool removerTagsNaoEnviadas = false)
        {
            if (removerTagsNaoEnviadas)
            {
                // Buscar tags existentes para fazer soft delete (gerar log de auditoria)
                var tagsParaRemover = new List<ImagemGrupoImagemTags>();

                if (listTags == null || listTags.Count == 0)
                {
                    // Remover todas as tags - usar Inner Join Fetch para carregar Tags e ImagemGrupoImagem com nomes
                    tagsParaRemover = (await _repository.FindByHql<ImagemGrupoImagemTags>(
                        @$"From ImagemGrupoImagemTags igita 
                                Inner Join Fetch igita.Tags t 
                                Inner Join Fetch igita.ImagemGrupoImagem igi
                                Inner Join Fetch igi.GrupoImagem gi
                                Inner Join Fetch gi.Empresa e
                           Where 
                                igi.Id = {imagemGrupoImagem.Id} and 
                                igita.UsuarioRemocao is null and 
                                igita.DataHoraRemocao is null")).ToList();
                }
                else
                {
                    // Remover apenas tags que nÃ£o estÃ£o na lista - usar Inner Join Fetch para carregar Tags e ImagemGrupoImagem com nomes
                    tagsParaRemover = (await _repository.FindByHql<ImagemGrupoImagemTags>(
                        @$"From ImagemGrupoImagemTags igita 
                                Inner Join Fetch igita.Tags t 
                                Inner Join Fetch igita.ImagemGrupoImagem igi
                                Inner Join Fetch igi.GrupoImagem gi
                                Inner Join Fetch gi.Empresa e
                           Where 
                                igi.Id = {imagemGrupoImagem.Id} and 
                                t.Id not in ({string.Join(",", listTags)}) and 
                                igita.UsuarioRemocao is null and 
                                igita.DataHoraRemocao is null")).ToList();
                }

                var usuario = await _repository.GetLoggedUser();
                foreach (var tagParaRemover in tagsParaRemover)
                {
                    // Log detalhado da remoÃ§Ã£o da tag
                    _logger.LogInformation($"[REMOÃ‡ÃƒO TAG] Removendo vÃ­nculo de tag da imagem da galeria | " +
                                          $"ImagemID: {tagParaRemover.ImagemGrupoImagem?.Id} | " +
                                          $"ImagemNome: {tagParaRemover.ImagemGrupoImagem?.Nome} | " +
                                          $"TagID: {tagParaRemover.Tags?.Id} | " +
                                          $"TagNome: {tagParaRemover.Tags?.Nome} | " +
                                          $"UsuarioID: {usuario?.userId ?? "Sistema"} | " +
                                          $"DataHora: {DateTime.Now:yyyy-MM-dd HH:mm:ss} | " +
                                          $"GrupoImagemID: {tagParaRemover.ImagemGrupoImagem?.GrupoImagem?.Id} | " +
                                          $"GrupoImagemNome: {tagParaRemover.ImagemGrupoImagem?.GrupoImagem?.Nome} | " +
                                          $"EmpresaID: {tagParaRemover.ImagemGrupoImagem?.GrupoImagem?.Empresa?.Id} | " +
                                          $"TipoRemocao: SincronizaÃ§Ã£o");

                    await _repository.Remove(tagParaRemover);
                }
            }

            if (listTags != null && listTags.Any())
            {
                var allTags = (await _repository.FindBySql<TagsModel>($"Select t.Id From Tags t Where t.Id in ({string.Join(",", listTags)})")).AsList();
                var tagsInexistentes = listTags.Where(c => !allTags.Any(b => b.Id == c)).AsList();
                if (tagsInexistentes.Count > 0)
                {
                    throw new ArgumentException($"Tags nÃ£o encontradas: {string.Join(",", tagsInexistentes)}");
                }

                // Buscar tags com seus nomes para garantir que estejam carregadas no log
                var tags = (await _repository.FindByHql<Tags>(
                    @$"From Tags t Where t.Id in ({string.Join(",", listTags)}) and 
                    Not Exists(Select dc.Tags From ImagemGrupoImagemTags dc Where dc.ImagemGrupoImagem = {imagemGrupoImagem.Id} and dc.Tags = t.Id and dc.UsuarioRemocao is null and dc.DataHoraRemocao is null)")).ToList();

                // Buscar imagem com nome carregado usando HQL para garantir que o nome esteja disponÃ­vel
                var imagemComNome = (await _repository.FindByHql<ImagemGrupoImagem>(
                    $"From ImagemGrupoImagem igi Where igi.Id = {imagemGrupoImagem.Id}")).FirstOrDefault() ?? imagemGrupoImagem;

                foreach (var tag in tags)
                {
                    var imagemGrupoImagemTags = new ImagemGrupoImagemTags()
                    {
                        ImagemGrupoImagem = imagemComNome,
                        Tags = tag
                    };

                    await _repository.Save(imagemGrupoImagemTags);
                }
            }
        }

        private async Task<string> GetPathToSaveImage(string? configPath, string fisicalDirectoryImages, int imageGroupId, int empresaId)
        {
            var wwrootpath = fisicalDirectoryImages;
            List<(string key, string path)> listItensPathTranslate = new List<(string key, string path)>()
            {
                new ("[WwwRootGrupoImagePath]",$"{wwrootpath}"),
                new ("[imageGroupId]",$"ImageGroupId_{imageGroupId}"),

             };


            if (string.IsNullOrEmpty(configPath))
                configPath = "[WwwRootGrupoImagePath]|[imageGroupId]";


            var itensToTranslate = configPath.Split('|');

            var pathReturn = string.Empty;
            List<string> paths = new List<string>();

            foreach (var item in itensToTranslate)
            {
                var pathConfigAtual = listItensPathTranslate.FirstOrDefault(b => b.key.Equals($"{item}", StringComparison.CurrentCultureIgnoreCase));
                if (string.IsNullOrEmpty(pathConfigAtual.path))
                    throw new ArgumentException($"NÃ£o foi encontrada a configuraÃ§Ã£o de direcionamento de path com a key: '{item}'");

                paths.Add(pathConfigAtual.path);
            }

            pathReturn = Path.Combine(paths.ToArray());

            return await Task.FromResult(pathReturn);

        }

        public async Task<DeleteResultModel> DeleteImage(int id)
        {
            var result = new DeleteResultModel();
            result.Id = id;

            try
            {

                var image = await _repository.FindById<ImagemGrupoImagem>(id);
                if (image is null)
                {
                    throw new FileNotFoundException($"NÃ£o foi encontrada a imagem com Id: {id}!");
                }

                _repository.BeginTransaction();

                // Fazer soft delete das tags relacionadas
                var tagsRelacionadas = await _repository.FindByHql<ImagemGrupoImagemTags>(
                    @$"From ImagemGrupoImagemTags igita 
                            Inner Join Fetch igita.Tags t 
                            Inner Join Fetch igita.ImagemGrupoImagem igi
                            Inner Join Fetch igi.GrupoImagem gi
                            Inner Join Fetch gi.Empresa e
                       Where 
                            igi.Id = {id} and 
                            igita.UsuarioRemocao is null and 
                            igita.DataHoraRemocao is null");

                var usuario = await _repository.GetLoggedUser();
                foreach (var tagRelacionada in tagsRelacionadas)
                {
                    // Log detalhado da remoÃ§Ã£o da tag durante exclusÃ£o da imagem
                    _logger.LogInformation($"[REMOÃ‡ÃƒO TAG - EXCLUSÃƒO IMAGEM] Removendo vÃ­nculo de tag por exclusÃ£o da imagem da galeria | " +
                                          $"ImagemID: {tagRelacionada.ImagemGrupoImagem?.Id} | " +
                                          $"ImagemNome: {tagRelacionada.ImagemGrupoImagem?.Nome} | " +
                                          $"TagID: {tagRelacionada.Tags?.Id} | " +
                                          $"TagNome: {tagRelacionada.Tags?.Nome} | " +
                                          $"UsuarioID: {usuario?.userId ?? "Sistema"} | " +
                                          $"DataHora: {DateTime.Now:yyyy-MM-dd HH:mm:ss} | " +
                                          $"GrupoImagemID: {tagRelacionada.ImagemGrupoImagem?.GrupoImagem?.Id} | " +
                                          $"GrupoImagemNome: {tagRelacionada.ImagemGrupoImagem?.GrupoImagem?.Nome} | " +
                                          $"EmpresaID: {tagRelacionada.ImagemGrupoImagem?.GrupoImagem?.Empresa?.Id} | " +
                                          $"TipoRemocao: ExclusÃ£o da imagem");

                    tagRelacionada.DataHoraRemocao = DateTime.Now;
                    tagRelacionada.UsuarioRemocao = !string.IsNullOrEmpty(usuario?.userId) ? Convert.ToInt32(usuario.Value.userId) : null;
                    await _repository.Save(tagRelacionada);
                }

                await _repository.Remove(image);

                var resultCommit = await _repository.CommitAsync();
                if (resultCommit.executed)
                {
                    result.Result = "Removido com sucesso!";
                    // NÃ£o precisa mais deletar arquivo fÃ­sico, pois a imagem estÃ¡ no banco
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
                _logger.LogError(err, $"NÃ£o foi possÃ­vel deletar a imagem: {id}");
                throw;
            }
        }

        public async Task<IEnumerable<ImageGroupImageModel>?> Search(SearchImageGroupImageModel searchModel)
        {
            var loggedUser = await _repository.GetLoggedUser();
            if (!loggedUser.HasValue) throw new ArgumentException("NÃ£o foi possÃ­vel filtrar os grupos de imagens e imagens para a galeria");


            if (!loggedUser.Value.isAdm)
            {
                if (loggedUser == null || string.IsNullOrEmpty(loggedUser.Value.providerKeyUser) || !loggedUser.Value.providerKeyUser.Contains("PessoaId", StringComparison.InvariantCultureIgnoreCase))
                    throw new ArgumentNullException("NÃ£o foi possÃ­vel identificar o usuÃ¡rio para comunicaÃ§Ã£o com o eSolution!");

                if (string.IsNullOrEmpty(loggedUser.Value.userId) || !Helper.IsNumeric(loggedUser.Value.userId))
                    throw new ArgumentNullException("NÃ£o foi possÃ­vel identificar o id do usuÃ¡rio logado.");

                var pessoaProvider = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(Convert.ToInt32(loggedUser.Value.userId), _communicationProvider.CommunicationProviderName);
                if (pessoaProvider == null || !pessoaProvider.Any() || pessoaProvider.Any(a=> string.IsNullOrEmpty(a.PessoaProvider)))
                    throw new ArgumentNullException($"NÃ£o foi possÃ­vel encontrar a pessoa do provider: {_communicationProvider.CommunicationProviderName} vinculada a pessoa: {loggedUser.Value.providerKeyUser}");

            }


            List<Parameter> parameters = new();
            StringBuilder sb = new("From ImagemGrupoImagem i Inner Join Fetch i.GrupoImagem g Where 1 = 1 ");
            if (!string.IsNullOrEmpty(searchModel.Nome))
                sb.AppendLine($" and Lower(i.Nome) like '%{searchModel.Nome.ToLower()}%'");

            if (searchModel.Id.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" and i.Id = :id");
                parameters.Add(new Parameter("id", searchModel.Id.GetValueOrDefault()));
            }

            if (searchModel.Ids != null && searchModel.Ids.Any())
            {
                sb.AppendLine($" and i.Id in ({string.Join(",", searchModel.Ids.AsList())})");
            }

            if (searchModel.UsuarioCriacao.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" and i.UsuarioCriacao = {searchModel.UsuarioCriacao.GetValueOrDefault()}");
            }

            if (searchModel.GrupoImagemId.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" and i.GrupoImagem = :grupoImagemId");
                parameters.Add(new Parameter("grupoImagemId", searchModel.GrupoImagemId.GetValueOrDefault()));
            }

            if (!loggedUser.Value.isAdm)
            {
                sb.AppendLine(@$" and (
                    -- Caso 1: Grupo e imagem sem tags (devem aparecer)
                    (not exists(Select gdt.GrupoImagem From GrupoImagemTags gdt Where gdt.GrupoImagem = g.Id and gdt.UsuarioRemocao is null and gdt.DataHoraRemocao is null) 
                     and not exists(Select igt.ImagemGrupoImagem From ImagemGrupoImagemTags igt Where igt.ImagemGrupoImagem = i.Id and igt.UsuarioRemocao is null and igt.DataHoraRemocao is null))
                    
                    or
                    
                    -- Caso 2: Grupo com tags compatÃ­veis com o usuÃ¡rio (independente das tags da imagem)
                    Exists(Select git.GrupoImagem 
                           From GrupoImagemTags git 
                           Inner Join UsuarioTags ut on git.Tags = ut.Tags 
                           Where git.UsuarioRemocao is null and git.DataHoraRemocao is null and git.GrupoImagem = g.Id and ut.Usuario = {loggedUser.Value.userId})
                    
                    or
                    
                    -- Caso 3: Imagem com tags compatÃ­veis com o usuÃ¡rio (independente das tags do grupo)
                    Exists(Select igt.ImagemGrupoImagem 
                           From ImagemGrupoImagemTags igt 
                           Inner Join UsuarioTags ut on igt.Tags = ut.Tags 
                           Where igt.UsuarioRemocao is null and igt.DataHoraRemocao is null and igt.ImagemGrupoImagem = i.Id and ut.Usuario = {loggedUser.Value.userId})
                    
                    or
                    
                    -- Caso 4: Grupo sem tags, mas imagem com tags compatÃ­veis 
                    (not exists(Select gdt.GrupoImagem From GrupoImagemTags gdt Where gdt.GrupoImagem = g.Id and gdt.UsuarioRemocao is null and gdt.DataHoraRemocao is null)
                     and Exists(Select igt.ImagemGrupoImagem 
                                From ImagemGrupoImagemTags igt 
                                Inner Join UsuarioTags ut on igt.Tags = ut.Tags 
                                Where igt.UsuarioRemocao is null and igt.DataHoraRemocao is null and igt.ImagemGrupoImagem = i.Id and ut.Usuario = {loggedUser.Value.userId}))
                )");
            }

            sb.AppendLine(" Order by i.Ordem, i.Nome");

            var imagens = await _repository.FindByHql<ImagemGrupoImagem>(sb.ToString(), session: null, parameters.ToArray());
            var itensRetorno = imagens.Select(a => _mapper.Map<ImageGroupImageModel>(a)).ToList();

            if (itensRetorno.Any())
            {
                var imagemGrupoImagemTags = (await _repository.FindByHql<ImagemGrupoImagemTags>(@$"From
                    ImagemGrupoImagemTags igita 
                    Inner Join Fetch igita.ImagemGrupoImagem igi 
                    Inner Join Fetch igita.Tags t 
                    Where 
                    igi.Id in ({string.Join(",", imagens.Select(b => b.Id).AsList())}) 
                    and igita.UsuarioRemocao is null and igita.DataHoraRemocao is null")).AsList();

                foreach (var itemImagem in itensRetorno)
                {
                    var tagsDaImagem = imagemGrupoImagemTags.Where(a => a.ImagemGrupoImagem != null && a.ImagemGrupoImagem.Id == itemImagem.Id);
                    if (tagsDaImagem.Any())
                        itemImagem.TagsRequeridas = tagsDaImagem.Select(b => _mapper.Map<ImagemGrupoImagemTagsModel>(b)).AsList();
                }
            }

            return await _serviceBase.SetUserName(itensRetorno);
        }

        public async Task ReorderImages(List<ReorderImageModel> images)
        {
            _repository.BeginTransaction();
            foreach (var image in images)
            {
                var imagemGrupoImagem = await _repository.FindById<ImagemGrupoImagem>(image.Id);
                if (imagemGrupoImagem != null)
                {
                    imagemGrupoImagem.Ordem = image.Ordem;
                    await _repository.Save(imagemGrupoImagem);
                }
            }
            await _repository.CommitAsync();
        }
    }
}
