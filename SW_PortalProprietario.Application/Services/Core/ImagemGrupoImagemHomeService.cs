using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Functions;
using System.Text;
using SW_Utils.Auxiliar;
using Dapper;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;

namespace SW_PortalProprietario.Application.Services.Core
{
    public class ImagemGrupoImagemHomeService : IImagemGrupoImagemHomeService
    {
        private readonly IRepositoryNH _repository;
        private readonly ILogger<ImagemGrupoImagemHomeService> _logger;
        private readonly IProjectObjectMapper _mapper;
        private readonly IServiceBase _serviceBase;

        public ImagemGrupoImagemHomeService(
            IRepositoryNH repository,
            ILogger<ImagemGrupoImagemHomeService> logger,
            IProjectObjectMapper mapper,
            IServiceBase serviceBase)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _serviceBase = serviceBase;
        }

        public async Task<ImagemGrupoImagemHomeModel> SaveImagem(ImagemGrupoImagemHomeInputModel model)
        {
            try
            {
                _repository.BeginTransaction();

                if (model.Imagem == null && model.Id.GetValueOrDefault(0) == 0)
                    throw new ArgumentException("Deve ser enviada uma imagem.");

                if (model.GrupoImagemHomeId.GetValueOrDefault(0) == 0)
                    throw new ArgumentException("Deve ser informado o GrupoImagemHomeId.");

                if (string.IsNullOrEmpty(model.Name))
                    throw new ArgumentException("Deve ser informado o nome da imagem");

                ImagemGrupoImagemHome? imagemGrupoImagemHome = null;
                if (model.Id.GetValueOrDefault(0) > 0)
                {
                    imagemGrupoImagemHome = (await _repository.FindByHql<ImagemGrupoImagemHome>($"From ImagemGrupoImagemHome i Where i.Id = {model.Id}")).FirstOrDefault();
                }

                if (imagemGrupoImagemHome == null)
                {
                    imagemGrupoImagemHome = new ImagemGrupoImagemHome();
                }

                var grupoImagemHome = (await _repository.FindByHql<GrupoImagemHome>($"From GrupoImagemHome gi Where gi.Id = {model.GrupoImagemHomeId}")).FirstOrDefault();
                if (grupoImagemHome == null)
                    throw new ArgumentException($"Não foi encontrado o Grupo de Imagem Home com o Id: {model.GrupoImagemHomeId}.");

                imagemGrupoImagemHome.GrupoImagemHome = grupoImagemHome;
                imagemGrupoImagemHome.Nome = model.Name;
                imagemGrupoImagemHome.NomeBotao = model.NomeBotao;
                imagemGrupoImagemHome.LinkBotao = model.LinkBotao;
                imagemGrupoImagemHome.Ordem = model.Ordem;
                imagemGrupoImagemHome.DataInicioVigencia = model.DataInicioVigencia;
                imagemGrupoImagemHome.DataFimVigencia = model.DataFimVigencia;

                // Se for nova imagem e não tiver ordem definida, definir ordem padrão
                if (model.Id.GetValueOrDefault(0) == 0 && (model.Ordem == null || model.Ordem == 0))
                {
                    var maxOrdem = (await _repository.FindBySql<int?>($"Select Max(Ordem) From ImagemGrupoImagemHome Where GrupoImagemHome = {model.GrupoImagemHomeId}")).FirstOrDefault();
                    imagemGrupoImagemHome.Ordem = (maxOrdem ?? 0) + 1;
                }

                // Converter IFormFile para byte[] apenas se uma nova imagem foi enviada
                if (model.Imagem != null && model.Imagem.Length > 0)
                {
                    // Validar tamanho máximo de 2MB
                    long maxSizeBytes = 2 * 1024 * 1024; // 2MB
                    if (model.Imagem.Length > maxSizeBytes)
                    {
                        throw new ArgumentException("A imagem deve ter no máximo 2MB.");
                    }

                    using (var memoryStream = new MemoryStream())
                    {
                        await model.Imagem.CopyToAsync(memoryStream);
                        imagemGrupoImagemHome.Imagem = memoryStream.ToArray();
                    }

                    // Validar tipo de arquivo
                    var ext = Functions.FileUtils.ObterTipoMIMEImagePorExtensao(Path.GetExtension(model.Imagem.FileName));
                    if (string.IsNullOrEmpty(ext))
                        throw new Exception($"Tipo de arquivo: ({Path.GetExtension(model.Imagem.FileName)}) não suportado.");
                }
                // Se for edição e não houver nova imagem, mantém a imagem existente

                var result = await _repository.Save(imagemGrupoImagemHome);
                await SincronizarTagsRequeridas(imagemGrupoImagemHome, model.TagsRequeridas, model.RemoverTagsNaoEnviadas ?? false);

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"Imagem grupo imagem home: ({result.Id} - {imagemGrupoImagemHome.Nome}) salva com sucesso!");

                    if (result != null)
                    {
                        var itemRetornar = (await Search(new SearchImagemGrupoImagemHomeModel() { Id = result.Id })).AsList();
                        if (itemRetornar != null && itemRetornar.Any())
                            return itemRetornar.First();
                    }
                }
                throw exception ?? new Exception($"Não foi possível salvar a Imagem grupo imagem home: ({imagemGrupoImagemHome.Nome})");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível salvar a Imagem grupo imagem home: ({model.Name})");
                _repository.Rollback();
                throw;
            }
        }

        public async Task<DeleteResultModel> DeleteImagem(int id)
        {
            var result = new DeleteResultModel();
            result.Id = id;

            try
            {
                var imagemGrupoImagemHome = await _repository.FindById<ImagemGrupoImagemHome>(id);
                if (imagemGrupoImagemHome is null)
                {
                    throw new ArgumentException($"Não foi encontrada a imagem com Id: {id}!");
                }

                _repository.BeginTransaction();

                // Fazer logging detalhado das tags relacionadas antes da remoção
                var tagsRelacionadas = await _repository.FindByHql<ImagemGrupoImagemHomeTags>(
                    @$"From ImagemGrupoImagemHomeTags igita 
                            Inner Join Fetch igita.Tags t 
                            Inner Join Fetch igita.ImagemGrupoImagemHome igi
                            Inner Join Fetch igi.GrupoImagemHome gih
                       Where 
                            igi.Id = {id}");

                var usuario = await _repository.GetLoggedUser();
                foreach (var tagRelacionada in tagsRelacionadas)
                {
                    // Log detalhado da remoção da tag durante exclusão da imagem
                    _logger.LogInformation($"[REMOÇÃO TAG - EXCLUSÃO IMAGEM] Removendo vínculo de tag por exclusão da imagem da home | " +
                                          $"ImagemHomeID: {tagRelacionada.ImagemGrupoImagemHome?.Id} | " +
                                          $"ImagemHomeNome: {tagRelacionada.ImagemGrupoImagemHome?.Nome} | " +
                                          $"TagID: {tagRelacionada.Tags?.Id} | " +
                                          $"TagNome: {tagRelacionada.Tags?.Nome} | " +
                                          $"UsuarioID: {usuario?.userId ?? "Sistema"} | " +
                                          $"DataHora: {DateTime.Now:yyyy-MM-dd HH:mm:ss} | " +
                                          $"GrupoImagemHomeID: {tagRelacionada.ImagemGrupoImagemHome?.GrupoImagemHome?.Id} | " +
                                          $"GrupoImagemHomeNome: {tagRelacionada.ImagemGrupoImagemHome?.GrupoImagemHome?.Nome} | " +
                                          $"TipoRemocao: Exclusão da imagem");

                    await _repository.Remove(tagRelacionada);
                }

                await _repository.Remove(imagemGrupoImagemHome);

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
                _logger.LogError(err, $"Não foi possível deletar a imagem: {id}");
                throw;
            }
        }

        public async Task<IEnumerable<ImagemGrupoImagemHomeModel>?> Search(SearchImagemGrupoImagemHomeModel searchModel)
        {
            List<Parameter> parameters = new();
            StringBuilder sb = new("From ImagemGrupoImagemHome i Inner Join Fetch i.GrupoImagemHome g Where 1 = 1");

            if (searchModel.GrupoImagemHomeId.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" and i.GrupoImagemHome = :grupoImagemHomeId");
                parameters.Add(new Parameter("grupoImagemHomeId", searchModel.GrupoImagemHomeId.GetValueOrDefault()));
            }

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

            sb.AppendLine(" Order by i.Ordem, i.Id");

            var imagens = await _repository.FindByHql<ImagemGrupoImagemHome>(sb.ToString(), parameters.ToArray());
            var itensRetorno = imagens.Select(a => _mapper.Map<ImagemGrupoImagemHomeModel>(a)).ToList();

            if (itensRetorno.Any())
            {
                var imagemGrupoImagemHomeTags = (await _repository.FindByHql<ImagemGrupoImagemHomeTags>(@$"From
                    ImagemGrupoImagemHomeTags igita 
                    Inner Join Fetch igita.ImagemGrupoImagemHome igi 
                    Inner Join Fetch igita.Tags t 
                    Where 
                    igi.Id in ({string.Join(",", imagens.Select(b => b.Id).AsList())})")).AsList();

                foreach (var itemImagem in itensRetorno)
                {
                    var tagsDaImagem = imagemGrupoImagemHomeTags.Where(a => a.ImagemGrupoImagemHome != null && a.ImagemGrupoImagemHome.Id == itemImagem.Id);
                    if (tagsDaImagem.Any())
                        itemImagem.TagsRequeridas = tagsDaImagem.Select(b => _mapper.Map<ImagemGrupoImagemHomeTagsModel>(b)).AsList();
                }
            }

            return await _serviceBase.SetUserName(itensRetorno);
        }

        public async Task<IEnumerable<ImagemGrupoImagemHomeModel>?> SearchForHome()
        {
            var loggedUser = await _repository.GetLoggedUser();
            if (!loggedUser.HasValue)
                throw new ArgumentException("Falha na configuração de permissões de acesso.");

            var dataAtual = DateTime.Now;
            StringBuilder sb = new("From ImagemGrupoImagemHome i Inner Join Fetch i.GrupoImagemHome g Where 1 = 1");

            sb.AppendLine($" and ((i.DataInicioVigencia is null and i.DataFimVigencia is null) or ((i.DataInicioVigencia is null or i.DataInicioVigencia <= :dataAtual) and (i.DataFimVigencia is null or i.DataFimVigencia >= :dataAtual)))");
            List<Parameter> parameters = new();
            parameters.Add(new Parameter("dataAtual", dataAtual));

            var imagens = await _repository.FindByHql<ImagemGrupoImagemHome>(sb.ToString(), parameters.ToArray());

            if (!imagens.Any())
                return new List<ImagemGrupoImagemHomeModel>();

            // Buscar todas as tags das imagens
            var imagemGrupoImagemHomeTags = (await _repository.FindByHql<ImagemGrupoImagemHomeTags>(@$"From
                ImagemGrupoImagemHomeTags igita 
                Inner Join Fetch igita.ImagemGrupoImagemHome igi 
                Inner Join Fetch igita.Tags t 
                Where 
                igi.Id in ({string.Join(",", imagens.Select(b => b.Id).AsList())})")).AsList();

            // Buscar todas as tags dos grupos das imagens
            var grupoIds = imagens.Where(i => i.GrupoImagemHome != null).Select(i => i.GrupoImagemHome.Id).Distinct().ToList();
            var grupoImagemHomeTags = new List<GrupoImagemHomeTags>();
            if (grupoIds.Any())
            {
                grupoImagemHomeTags = (await _repository.FindByHql<GrupoImagemHomeTags>(@$"From
                    GrupoImagemHomeTags gita 
                    Inner Join Fetch gita.GrupoImagemHome gi 
                    Inner Join Fetch gita.Tags t 
                    Where 
                    gi.Id in ({string.Join(",", grupoIds)}) and gita.UsuarioRemocao is null and gita.DataHoraRemocao is null")).AsList();
            }

            // Se não for admin, filtrar por tags
            if (!loggedUser.Value.isAdm)
            {
                // Buscar tags do usuário
                var userTags = (await _repository.FindBySql<TagsModel>($"Select ut.Tags as Id From UsuarioTags ut Where ut.Usuario = {loggedUser.Value.userId}")).AsList();
                var userTagIds = userTags.Where(t => t.Id is not null).Select(b => b.Id).AsList();

                // Filtrar imagens: sem tag (nem na imagem nem no grupo) OU com tags compatíveis com as do usuário
                var imagensFiltradas = new List<ImagemGrupoImagemHome>();
                foreach (var imagem in imagens)
                {
                    var tagsDaImagem = imagemGrupoImagemHomeTags
                        .Where(a => a.ImagemGrupoImagemHome != null && a.ImagemGrupoImagemHome.Id == imagem.Id)
                        .ToList();

                    var tagsDoGrupo = new List<GrupoImagemHomeTags>();
                    if (imagem.GrupoImagemHome != null)
                    {
                        tagsDoGrupo = grupoImagemHomeTags
                            .Where(a => a.GrupoImagemHome != null && a.GrupoImagemHome.Id == imagem.GrupoImagemHome.Id)
                            .ToList();
                    }

                    // Se a imagem e o grupo não têm tags, incluir
                    if (!tagsDaImagem.Any() && !tagsDoGrupo.Any())
                    {
                        imagensFiltradas.Add(imagem);
                    }
                    else
                    {
                        // Verificar se alguma tag da imagem ou do grupo é compatível com as do usuário
                        var tagsDaImagemIds = tagsDaImagem
                            .Where(t => t.Tags != null)
                            .Select(t => t.Tags.Id)
                            .ToList();

                        var tagsDoGrupoIds = tagsDoGrupo
                            .Where(t => t.Tags != null)
                            .Select(t => t.Tags.Id)
                            .ToList();

                        var todasAsTags = tagsDaImagemIds.Union(tagsDoGrupoIds).ToList();

                        if (todasAsTags.Any(tagId => userTagIds.Contains(tagId)))
                        {
                            imagensFiltradas.Add(imagem);
                        }
                    }
                }
                imagens = imagensFiltradas;
            }

            var itensRetorno = imagens
                .OrderBy(i => i.Ordem ?? int.MaxValue)
                .ThenBy(i => i.Id)
                .Select(a => _mapper.Map<ImagemGrupoImagemHomeModel>(a))
                .ToList();

            if (itensRetorno.Any())
            {
                foreach (var itemImagem in itensRetorno)
                {
                    var tagsDaImagem = imagemGrupoImagemHomeTags.Where(a => a.ImagemGrupoImagemHome != null && a.ImagemGrupoImagemHome.Id == itemImagem.Id);
                    if (tagsDaImagem.Any())
                        itemImagem.TagsRequeridas = tagsDaImagem.Select(b => _mapper.Map<ImagemGrupoImagemHomeTagsModel>(b)).AsList();
                }
            }

            return await _serviceBase.SetUserName(itensRetorno);
        }

        private async Task SincronizarTagsRequeridas(ImagemGrupoImagemHome imagemGrupoImagemHome, List<int> listTags, bool removerTagsNaoEnviadas = false)
        {
            if (removerTagsNaoEnviadas)
            {
                // Buscar tags existentes para fazer log detalhado antes da remoção
                var tagsParaRemover = new List<ImagemGrupoImagemHomeTags>();

                if (listTags == null || listTags.Count == 0)
                {
                    // Remover todas as tags - usar Inner Join Fetch para carregar Tags e ImagemGrupoImagemHome com nomes
                    tagsParaRemover = (await _repository.FindByHql<ImagemGrupoImagemHomeTags>(
                        @$"From ImagemGrupoImagemHomeTags igita 
                                Inner Join Fetch igita.Tags t 
                                Inner Join Fetch igita.ImagemGrupoImagemHome igi
                                Inner Join Fetch igi.GrupoImagemHome gih
                           Where 
                                igi.Id = {imagemGrupoImagemHome.Id}")).ToList();
                }
                else
                {
                    // Remover apenas tags que não estão na lista - usar Inner Join Fetch para carregar Tags e ImagemGrupoImagemHome com nomes
                    tagsParaRemover = (await _repository.FindByHql<ImagemGrupoImagemHomeTags>(
                        @$"From ImagemGrupoImagemHomeTags igita 
                                Inner Join Fetch igita.Tags t 
                                Inner Join Fetch igita.ImagemGrupoImagemHome igi
                                Inner Join Fetch igi.GrupoImagemHome gih
                           Where 
                                igi.Id = {imagemGrupoImagemHome.Id} and 
                                t.Id not in ({string.Join(",", listTags)})")).ToList();
                }

                var usuario = await _repository.GetLoggedUser();
                foreach (var tagParaRemover in tagsParaRemover)
                {
                    // Log detalhado da remoção da tag
                    _logger.LogInformation($"[REMOÇÃO TAG] Removendo vínculo de tag da imagem da home | " +
                                          $"ImagemHomeID: {tagParaRemover.ImagemGrupoImagemHome?.Id} | " +
                                          $"ImagemHomeNome: {tagParaRemover.ImagemGrupoImagemHome?.Nome} | " +
                                          $"TagID: {tagParaRemover.Tags?.Id} | " +
                                          $"TagNome: {tagParaRemover.Tags?.Nome} | " +
                                          $"UsuarioID: {usuario?.userId ?? "Sistema"} | " +
                                          $"DataHora: {DateTime.Now:yyyy-MM-dd HH:mm:ss} | " +
                                          $"GrupoImagemHomeID: {tagParaRemover.ImagemGrupoImagemHome?.GrupoImagemHome?.Id} | " +
                                          $"GrupoImagemHomeNome: {tagParaRemover.ImagemGrupoImagemHome?.GrupoImagemHome?.Nome} | " +
                                          $"TipoRemocao: Sincronização");

                    await _repository.Remove(tagParaRemover);
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

                var tags = (await _repository.FindBySql<TagsModel>(@$"Select t.Id From Tags t Where t.Id in ({string.Join(",", listTags)}) and 
                Not Exists(Select dc.Tags From ImagemGrupoImagemHomeTags dc Where dc.ImagemGrupoImagemHome = {imagemGrupoImagemHome.Id} and dc.Tags = t.Id)")).AsList();

                foreach (var t in tags)
                {
                    var imagemGrupoImagemHomeTags = new ImagemGrupoImagemHomeTags()
                    {
                        ImagemGrupoImagemHome = imagemGrupoImagemHome,
                        Tags = new SW_PortalProprietario.Domain.Entities.Core.Geral.Tags { Id = t.Id.GetValueOrDefault(0) }
                    };

                    await _repository.Save(imagemGrupoImagemHomeTags);
                }
            }
        }

        public async Task ReorderImages(List<ReorderImageModel> images)
        {
            _repository.BeginTransaction();
            foreach (var image in images)
            {
                var imagemGrupoImagemHome = await _repository.FindById<ImagemGrupoImagemHome>(image.Id);
                if (imagemGrupoImagemHome != null)
                {
                    imagemGrupoImagemHome.Ordem = image.Ordem;
                    await _repository.Save(imagemGrupoImagemHome);
                }
            }
            await _repository.CommitAsync();
        }
    }
}

