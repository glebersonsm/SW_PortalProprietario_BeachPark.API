using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models.DocumentTemplates;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Enumns;
using SW_Utils.Auxiliar;

namespace SW_PortalProprietario.Application.Services.Core;

public class DocumentTemplateService : IDocumentTemplateService
{
    private readonly IRepositoryNH _repository;
    private readonly IProjectObjectMapper _mapper;
    private readonly IServiceBase _serviceBase;
    private readonly ILogger<DocumentTemplateService> _logger;

    public DocumentTemplateService(
        IRepositoryNH repository,
        IProjectObjectMapper mapper,
        IServiceBase serviceBase,
        ILogger<DocumentTemplateService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _serviceBase = serviceBase ?? throw new ArgumentNullException(nameof(serviceBase));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<DocumentTemplateModel> CreateAsync(DocumentTemplateUploadInputModel model, int usuarioId)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        if (string.IsNullOrWhiteSpace(model.ContentHtml))
            throw new ArgumentException("O conteúdo HTML do template deve ser informado.", nameof(model.ContentHtml));

        if (model.TemplateId.HasValue && model.TemplateId.Value > 0)
            throw new ArgumentException("TemplateId não deve ser informado ao criar um novo template.", nameof(model.TemplateId));

        try
        {
            _repository.BeginTransaction();

           
            var entity = new DocumentTemplate
            {
                TemplateType = model.TemplateType,
                Name = string.IsNullOrWhiteSpace(model.Name)
                    ? $"Template {model.TemplateType}"
                    : model.Name!.Trim(),
                Version = 1,
                ContentHtml = model.ContentHtml,
                Active = true,
                ObjectGuid = Guid.NewGuid().ToString("N")
            };

            await _repository.Save(entity);

            // Salvar tags se fornecidas
            if (model.TagsIds != null && model.TagsIds.Any())
            {
                await SincronizarTagsRequeridas(entity, model.TagsIds);
            }

            var (executed, exception) = await _repository.CommitAsync();
            if (!executed)
            {
                throw exception ?? new Exception("Falha ao criar o template de documento.");
            }

            var mapped = _mapper.Map<DocumentTemplateModel>(entity);
            mapped = await _serviceBase.SetUserName(mapped);
            
            // Carregar tags do template
            mapped.Tags = await CarregarTagsDoTemplate(entity.Id);
            
            return mapped;
        }
        catch (Exception ex)
        {
            _repository.Rollback();
            _logger.LogError(ex, "Erro ao criar template de documento {TemplateType}", model.TemplateType);
            throw;
        }
    }

    public async Task<DocumentTemplateModel> UpdateAsync(DocumentTemplateUploadInputModel model, int usuarioId)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        if (string.IsNullOrWhiteSpace(model.ContentHtml))
            throw new ArgumentException("O conteúdo HTML do template deve ser informado.", nameof(model.ContentHtml));

        if (!model.TemplateId.HasValue || model.TemplateId.Value <= 0)
            throw new ArgumentException("TemplateId é obrigatório para atualizar um template.", nameof(model.TemplateId));

        try
        {
            _repository.BeginTransaction();

            var templateId = model.TemplateId.Value;
            var parameters = new[]
            {
                new Parameter("templateType", (int)model.TemplateType),
                new Parameter("templateId", templateId)
            };

            // Buscar template existente pelo ID dele
            var itemToUpdate = (await _repository.FindByHql<DocumentTemplate>(
                "from DocumentTemplate dt where dt.TemplateType = :templateType and dt.Id = :templateId",
                session: null, parameters)).FirstOrDefault();

            if (itemToUpdate == null)
                throw new ArgumentException($"Não foi encontrado o template de Id: {model.TemplateId}");

            itemToUpdate.TemplateType = model.TemplateType;
            itemToUpdate.Name = string.IsNullOrWhiteSpace(model.Name)
                    ? $"Template {model.TemplateType}"
                    : model.Name!.Trim();
            itemToUpdate.Version = 1;
            itemToUpdate.ContentHtml = model.ContentHtml;
            itemToUpdate.Active = true;
            itemToUpdate.ObjectGuid = Guid.NewGuid().ToString("N");

            await _repository.Save(itemToUpdate);


            // Salvar tags se fornecidas
            if (model.TagsIds != null && model.TagsIds.Any())
            {
                await SincronizarTagsRequeridas(itemToUpdate, model.TagsIds);
            }

            var (executed, exception) = await _repository.CommitAsync();
            if (!executed)
            {
                throw exception ?? new Exception("Falha ao atualizar o template de documento.");
            }

            var mapped = _mapper.Map<DocumentTemplateModel>(itemToUpdate);
            mapped = await _serviceBase.SetUserName(mapped);
            
            // Carregar tags do template
            mapped.Tags = await CarregarTagsDoTemplate(itemToUpdate.Id);
            
            return mapped;
        }
        catch (Exception ex)
        {
            _repository.Rollback();
            _logger.LogError(ex, "Erro ao atualizar template de documento {TemplateType} id {TemplateId}", model.TemplateType, model.TemplateId);
            throw;
        }
    }

    [Obsolete("Use CreateAsync ou UpdateAsync")]
    public async Task<DocumentTemplateModel> UploadAsync(DocumentTemplateUploadInputModel model, int usuarioId)
    {
        // Se tem templateId, usar Update, senão usar Create
        if (model.TemplateId.HasValue && model.TemplateId.Value > 0)
        {
            return await UpdateAsync(model, usuarioId);
        }
        else
        {
            return await CreateAsync(model, usuarioId);
        }
    }

    public async Task<DocumentTemplateModel?> GetActiveTemplateAsync(EnumDocumentTemplateType? templateType, int? templateId = null)
    {
        if (templateId.GetValueOrDefault(0) < 0)
            throw new ArgumentException("templateId deve ser maior que zero, se fornecido.", nameof(templateId));

        var parameters = new List<Parameter>()
        {
            new Parameter("active", true)
        };

        if (templateType.HasValue)
            parameters.Add(new Parameter("templateType", (int)templateType));

        // Se templateId foi fornecido, buscar diretamente pelo ID
        if (templateId.HasValue && templateId.Value > 0)
        {
            var templateById = await _repository.FindByHql<DocumentTemplate>(
                "from DocumentTemplate dt where dt.Id = :templateId",
                session: null, new[] { new Parameter("templateId", templateId.Value) });
            
            var entity = templateById.FirstOrDefault();
            if (entity != null && entity.Active)
            {
                var model = _mapper.Map<DocumentTemplateModel>(entity);
                model = await _serviceBase.SetUserName(model);
                
                // IMPORTANTE: Garantir que sempre retornamos contentHtml, mesmo que seja string vazia
                if (model.ContentHtml == null)
                    model.ContentHtml = string.Empty;
                
                // Carregar tags do template
                model.Tags = await CarregarTagsDoTemplate(entity.Id);
                
                return model;
            }
        }

        // Buscar template ativo por tipo (sem templateId ou se não encontrou pelo ID)
        var resultado = await _repository.FindByHql<DocumentTemplate>(
            "from DocumentTemplate dt where dt.TemplateType = :templateType and dt.Active = :active order by dt.Version desc",
            session: null, parameters.ToArray());

        var entityByType = resultado.FirstOrDefault();
        if (entityByType == null)
            return null;

        var modelByType = _mapper.Map<DocumentTemplateModel>(entityByType);
        modelByType = await _serviceBase.SetUserName(modelByType);
        
        // IMPORTANTE: Garantir que sempre retornamos contentHtml, mesmo que seja string vazia
        if (modelByType.ContentHtml == null)
            modelByType.ContentHtml = string.Empty;
        
        // Carregar tags do template
        modelByType.Tags = await CarregarTagsDoTemplate(entityByType.Id);
        
        return modelByType;
    }

    public async Task<IReadOnlyCollection<DocumentTemplateSummaryModel>> ListAsync(EnumDocumentTemplateType? templateType = null)
    {
        var sb = new StringBuilder("from DocumentTemplate dt where 1 = 1 and dt.Active = :active");
        var parameters = new List<Parameter>() { new Parameter("active", true) };

        if (templateType.HasValue)
        {
            sb.Append(" and dt.TemplateType = :templateType");
            parameters.Add(new Parameter("templateType", (int)templateType.Value));
        }

        sb.Append(" order by dt.Id");

        var entidades = await _repository.FindByHql<DocumentTemplate>(sb.ToString(), session: null, parameters.ToArray());

        // Retornar todos os templates sem filtragem por grupo ou por active
        var models = entidades.Select(e => _mapper.Map<DocumentTemplateSummaryModel>(e)).ToList();
        return models;
    }

    public async Task<string?> GetTemplateContentHtmlAsync(EnumDocumentTemplateType? templateType, int? templateId = null)
    {
        var parameters = new List<Parameter>()
        {
            new Parameter("active", true)
        };

        if (templateType.HasValue)
            parameters.Add(new Parameter("templateType", (int)templateType));

        // Se templateId foi fornecido, buscar diretamente pelo ID
        if (templateId.HasValue && templateId.Value > 0)
        {
            var templateById = await _repository.FindByHql<DocumentTemplate>(
                "from DocumentTemplate dt where dt.Id = :templateId",
                session: null, new[] { new Parameter("templateId", templateId.Value) });
            
            var entity = templateById.FirstOrDefault();
            if (entity != null && entity.Active)
            {
                return entity.ContentHtml ?? string.Empty;
            }
        }

        // Buscar template ativo por tipo (sem templateId ou se não encontrou pelo ID)
        var resultado = await _repository.FindByHql<DocumentTemplate>(
            "from DocumentTemplate dt where dt.TemplateType = :templateType and dt.Active = :active order by dt.Version desc",
            session: null, parameters.ToArray());

        return resultado.FirstOrDefault()?.ContentHtml ?? string.Empty;
    }

    private async Task SincronizarTagsRequeridas(DocumentTemplate documentTemplate, List<int> listTags)
    {
        if (listTags == null || !listTags.Any())
        {
            // Se não há tags na lista, remover todas as tags existentes
            var tagsVinculadas = (await _repository.FindByHql<DocumentTemplateTags>(
                $"From DocumentTemplateTags dtt Inner Join Fetch dtt.DocumentTemplate dt Where dt.Id = {documentTemplate.Id} and dtt.UsuarioRemocao is null and dtt.DataHoraRemocao is null")).AsList();

            foreach (var tagParaRemover in tagsVinculadas)
            {
                tagParaRemover.UsuarioRemocao = documentTemplate.UsuarioCriacao;
                tagParaRemover.DataHoraRemocao = DateTime.Now;
                await _repository.Save(tagParaRemover);
            }
            return;
        }

        // Validar se as tags existem
        var allTags = (await _repository.FindBySql<TagsModel>($"Select t.Id From Tags t Where t.Id in ({string.Join(",", listTags)})")).AsList();
        var tagsInexistentes = listTags.Where(c => !allTags.Any(b => b.Id == c)).AsList();
        if (tagsInexistentes.Count > 0)
        {
            throw new ArgumentException($"Tags não encontradas: {string.Join(",", tagsInexistentes)}");
        }

        // Buscar tags existentes do template
        var tagsExistentes = (await _repository.FindByHql<DocumentTemplateTags>(
            $"From DocumentTemplateTags dtt Inner Join Fetch dtt.DocumentTemplate dt Inner Join Fetch dtt.Tags t Where dt.Id = {documentTemplate.Id} and dtt.UsuarioRemocao is null and dtt.DataHoraRemocao is null")).AsList();

        // Remover tags existentes que não estão na lista
        var tagsParaRemover = tagsExistentes.Where(t => !listTags.Contains(t.Tags.Id)).AsList();
        foreach (var tagParaRemover in tagsParaRemover)
        {
            tagParaRemover.UsuarioRemocao = documentTemplate.UsuarioCriacao;
            tagParaRemover.DataHoraRemocao = DateTime.Now;
            await _repository.Save(tagParaRemover);
        }

        // Adicionar novas tags que não existem
        var tagsParaAdicionar = listTags.Where(tagId => !tagsExistentes.Any(te => te.Tags.Id == tagId)).AsList();
        foreach (var tagId in tagsParaAdicionar)
        {
            var documentTemplateTags = new DocumentTemplateTags()
            {
                DocumentTemplate = documentTemplate,
                Tags = new Tags() { Id = tagId },
                UsuarioCriacao = documentTemplate.UsuarioCriacao,
                DataHoraCriacao = DateTime.Now,
                ObjectGuid = Guid.NewGuid().ToString("N")
            };

            await _repository.Save(documentTemplateTags);
        }
    }

    private async Task<List<DocumentTemplateTagDto>> CarregarTagsDoTemplate(int templateId)
    {
        var tagsDoTemplate = (await _repository.FindByHql<DocumentTemplateTags>(
            $"From DocumentTemplateTags dtt Inner Join Fetch dtt.DocumentTemplate dt Inner Join Fetch dtt.Tags t Where dtt.UsuarioRemocao is null and dtt.DataHoraRemocao is null and dt.Id = {templateId}")).AsList();

        return tagsDoTemplate
            .Where(tag => tag.Tags != null)
            .Select(tag => new DocumentTemplateTagDto
            {
                Id = tag.Tags.Id,
                Name = tag.Tags.Nome ?? string.Empty
            }).ToList();
    }

    public async Task<bool> DeleteAsync(int templateId, int usuarioId)
    {
        if (templateId <= 0)
            throw new ArgumentException("ID do template deve ser maior que zero.", nameof(templateId));

        try
        {
            _repository.BeginTransaction();

            var template = (await _repository.FindByHql<DocumentTemplate>(
                "from DocumentTemplate dt where dt.Id = :templateId",
                session: null, new[] { new Parameter("templateId", templateId) })).FirstOrDefault();

            if (template == null)
                throw new ArgumentException($"Template com ID {templateId} não encontrado.");

            // Soft delete: desativar o template
            template.Active = false;
            template.UsuarioAlteracao = usuarioId;
            template.DataHoraAlteracao = DateTime.Now;

            _logger.LogInformation("Desativando template {TemplateId} pelo usuário {UsuarioId}", templateId, usuarioId);

            await _repository.Save(template);

            var (executed, exception) = await _repository.CommitAsync();
            if (!executed)
            {
                throw exception ?? new Exception("Falha ao desativar o template de documento.");
            }

            return true;
        }
        catch (Exception ex)
        {
            _repository.Rollback();
            _logger.LogError(ex, "Erro ao desativar template de documento {TemplateId}", templateId);
            throw;
        }
    }
}

