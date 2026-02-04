using Dapper;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_Utils.Auxiliar;
using System.Text;

namespace SW_PortalProprietario.Application.Services.Core
{
    public class TagsService : ITagsService
    {
        private readonly IRepositoryNH _repository;
        private readonly ILogger<TagsService> _logger;
        private readonly IProjectObjectMapper _mapper;
        private readonly IServiceBase _serviceBase;

        public TagsService(IRepositoryNH repository,
            ILogger<TagsService> logger,
            IProjectObjectMapper mapper,
            IServiceBase serviceBase)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _serviceBase = serviceBase;
        }

        public async Task<DeleteResultModel> DeleteTags(int id)
        {
            var result = new DeleteResultModel();
            result.Id = id;

            try
            {

                var tags = await _repository.FindById<Pais>(id);
                if (tags is null)
                {
                    throw new Exception($"Não foi encontrada a tag com Id: {id}!");
                }


                _repository.BeginTransaction();
                _repository.Remove(tags);

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
                _logger.LogError(err, $"Não foi possível deletar a tag: {id}");
                throw;

            }
        }

        public async Task<TagsModel> SaveTags(TagsInputModel tags)
        {
            try
            {
                _repository.BeginTransaction();

                var exist = (await _repository.FindByHql<Tags>($"From Tags t Left Join Fetch t.Parent p Where Lower(t.Nome) = '{tags.Nome?.TrimEnd().ToLower()}' or t.Id = {tags.Id.GetValueOrDefault(0)}")).FirstOrDefault();
                if (exist != null)
                {
                    var tag = _mapper.Map(tags, exist);
                }
                else
                    exist = _mapper.Map(tags, new Tags());

                if (tags.TagsParentId.GetValueOrDefault(0) > 0)
                {
                    var tagParent = (await _repository.FindBySql<TagsModel>($"Select t.Id From Tags t Where t.Id = {tags.TagsParentId.GetValueOrDefault()}")).FirstOrDefault();
                    if (tagParent == null)
                        throw new ArgumentException($"Não foi encontrada uma Tag com o Id indicado no campo TagsParentId: {tags.TagsParentId.GetValueOrDefault()}");
                }

                var result = await _repository.Save(exist);
                if (string.IsNullOrEmpty(exist.Path))
                {
                    exist.Path = $"tags/{exist.Id}";
                    await _repository.Save(exist);
                }

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"País: ({result.Id} - {exist.Id} - {exist.Nome}) salvo com sucesso!");

                    if (result != null)
                        return _mapper.Map(result, new TagsModel());

                }

                throw exception ?? new Exception($"Não foi possível salvar a Tag: ({tags.Nome})");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível salvar a Tag: ({tags.Nome})");
                _repository.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<TagsModel>?> SearchTags(SearchPadraoModel searchModel)
        {
            List<Parameter> parameters = new();
            List<Parameter> parameters1 = new();
            StringBuilder sb = new("From Tags t Left Join Fetch t.Parent p Where 1 = 1");
            if (!string.IsNullOrEmpty(searchModel.Nome))
                sb.AppendLine($" and Lower(t.Nome) like '%{searchModel.Nome.ToLower()}%'");

            if (searchModel.Id.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" and t.Id = :id");
                parameters.Add(new Parameter("id", searchModel.Id));
            }

            if (searchModel.Ids != null && searchModel.Ids.Any())
            {
                sb.AppendLine($" and t.Id in ({string.Join(",", searchModel.Ids.AsList())})");
            }

            if (searchModel.UsuarioCriacao.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" and t.UsuarioCriacao = {searchModel.UsuarioCriacao.GetValueOrDefault()}");
            }

            var tags = await _repository.FindByHql<Tags>(sb.ToString(), session: null, parameters.ToArray());

            if (tags.Any())
                return await _serviceBase.SetUserName(tags.Select(a => _mapper.Map<TagsModel>(a)).AsList());

            return default;
        }

    }
}
