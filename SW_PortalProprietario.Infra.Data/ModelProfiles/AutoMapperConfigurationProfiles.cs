using AutoMapper;
using SW_PortalProprietario.Application.Models.AuditModels;
using SW_PortalProprietario.Application.Models.DocumentTemplates;
using SW_PortalProprietario.Application.Models.FrameworkModels;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Models.PessoaModels;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Domain.Entities.Core.Auditoria;
using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;
using SW_PortalProprietario.Domain.Entities.Core.Framework;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;
using System.Linq;

namespace SW_PortalProprietario.Infra.Data.ModelProfiles
{
    public class AutoMapperConfigurationProfiles : Profile
    {
        public AutoMapperConfigurationProfiles()
        {
            ConfiguraModelToEntityMappings();
            ConfiguraEntityToModelMappings();
        }

        private void ConfiguraEntityToModelMappings()
        {
            #region Pais 

            CreateMap<Pais, PaisModel>();

            #endregion

            #region Estado 

            CreateMap<Estado, EstadoModel>()
                .ForPath(dest => dest.Pais,
                    opt => opt.MapFrom(src => src.Pais));

            #endregion

            #region Cidade 

            CreateMap<Cidade, CidadeModel>();

            #endregion

            #region Pessoa

            CreateMap<Pessoa, PessoaCompletaModel>();
            CreateMap<Pessoa, PessoaJuridicaModel>()
                .ForPath(dest => dest.RazaoSocial, opt => opt.MapFrom(src => src.Nome));

            #region Pessoa documento

            CreateMap<PessoaDocumento, PessoaDocumentoModel>()
                .ForPath(dest => dest.PessoaId, opt => opt.MapFrom(src => src.Pessoa.Id))
                .ForPath(dest => dest.TipoDocumentoId, opt => opt.MapFrom(src => src.TipoDocumento.Id))
                .ForPath(dest => dest.TipoDocumentoMascara, opt => opt.MapFrom(src => src.TipoDocumento.Mascara))
                .ForPath(dest => dest.TipoDocumentoNome, opt => opt.MapFrom(src => src.TipoDocumento.Nome));

            #endregion

            #region Pessoa telefone

            CreateMap<PessoaTelefone, PessoaTelefoneModel>()
                .ForPath(dest => dest.PessoaId, opt => opt.MapFrom(src => src.Pessoa.Id))
                .ForPath(dest => dest.TipoTelefoneId, opt => opt.MapFrom(src => src.TipoTelefone.Id))
                .ForPath(dest => dest.TipoTelefoneNome, opt => opt.MapFrom(src => src.TipoTelefone.Nome))
                .ForPath(dest => dest.TipoTelefoneMascara, opt => opt.MapFrom(src => src.TipoTelefone.Mascara));

            #endregion

            #region Pessoa endereço

            CreateMap<PessoaEndereco, PessoaEnderecoModel>()
                .ForPath(dest => dest.PessoaId, opt => opt.MapFrom(src => src.Pessoa.Id))
                .ForPath(dest => dest.TipoEnderecoId, opt => opt.MapFrom(src => src.TipoEndereco.Id))
                .ForPath(dest => dest.TipoEnderecoNome, opt => opt.MapFrom(src => src.TipoEndereco.Nome))
                .ForPath(dest => dest.CidadeId, opt => opt.MapFrom(src => src.Cidade.Id))
                .ForPath(dest => dest.EstadoId, opt => opt.MapFrom(src => src.Cidade.Estado.Id))
                .ForPath(dest => dest.CidadeNome, opt => opt.MapFrom(src => src.Cidade.Nome))
                .ForPath(dest => dest.EstadoSigla, opt => opt.MapFrom(src => src.Cidade.Estado.Sigla));

            #endregion

            #endregion

            #region Empresa

            CreateMap<Empresa, EmpresaModel>()
                .ForPath(dest => dest.PessoaEmpresa, opt => opt.MapFrom(src => src.Pessoa))
                .ForPath(dest => dest.Codigo, opt => opt.MapFrom(src => src.Codigo))
                .ForPath(dest => dest.PessoaGrupoEmpresa, opt => opt.MapFrom(src => src.GrupoEmpresa.Pessoa))
                .ForPath(dest => dest.GrupoEmpresaId, opt => opt.MapFrom(src => src.GrupoEmpresa.Id))
                .ForPath(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForPath(dest => dest.GrupoEmpresaCodigo, opt => opt.MapFrom(src => src.GrupoEmpresa.Codigo))
                .ForPath(dest => dest.PessoaEmpresa.RegimeTributario, opt => opt.MapFrom(src => src.Pessoa.RegimeTributario));


            #endregion

            #region Grupo Empresa 

            CreateMap<GrupoEmpresa, GrupoEmpresaModel>()
                .ForPath(dest => dest.Pessoa, opt => opt.MapFrom(src => src.Pessoa))
                .ForPath(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

            #endregion

            #region Grupo documento e documento e tags

            CreateMap<GrupoDocumento, GrupoDocumentoModel>()
                .ForPath(dest => dest.EmpresaId, opt => opt.MapFrom(src => src.Empresa.Id));

            CreateMap<Documento, DocumentoModel>()
                .ForPath(dest => dest.GrupoDocumento, opt => opt.MapFrom(src => src.GrupoDocumento))
                .ForMember(dest => dest.Arquivo, opt => opt.MapFrom(src => src.Arquivo))
                .ForMember(dest => dest.NomeArquivo, opt => opt.MapFrom(src => src.NomeArquivo))
                .ForMember(dest => dest.TipoMime, opt => opt.MapFrom(src => src.TipoMime))
                .ForMember(dest => dest.DataInicioVigencia, opt => opt.MapFrom(src => src.DataInicioVigencia))
                .ForMember(dest => dest.DataFimVigencia, opt => opt.MapFrom(src => src.DataFimVigencia));

            CreateMap<Documento, DocumentoModelSimplificado>()
                .ForPath(dest => dest.GrupoDocumentoId, opt => opt.MapFrom(src => src.GrupoDocumento.Id))
                .ForMember(dest => dest.Arquivo, opt => opt.MapFrom(src => src.Arquivo))
                .ForMember(dest => dest.NomeArquivo, opt => opt.MapFrom(src => src.NomeArquivo))
                .ForMember(dest => dest.TipoMime, opt => opt.MapFrom(src => src.TipoMime))
                .ForMember(dest => dest.DataInicioVigencia, opt => opt.MapFrom(src => src.DataInicioVigencia))
                .ForMember(dest => dest.DataFimVigencia, opt => opt.MapFrom(src => src.DataFimVigencia));

            CreateMap<DocumentoTags, DocumentoTagsModel>()
                .ForPath(dest => dest.DocumentoId, opt => opt.MapFrom(src => src.Documento.Id))
                .ForPath(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags));

            CreateMap<GrupoDocumentoTags, GrupoDocumentoTagsModel>()
                .ForPath(dest => dest.GrupoDocumentoId, opt => opt.MapFrom(src => src.GrupoDocumento.Id))
                .ForPath(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags));

            #endregion

            #region RegraPaxFree

            CreateMap<RegraPaxFree, RegraPaxFreeModel>()
                .ForMember(dest => dest.DataInicioVigencia, opt => opt.MapFrom(src => src.DataInicioVigencia))
                .ForMember(dest => dest.DataFimVigencia, opt => opt.MapFrom(src => src.DataFimVigencia));

            CreateMap<RegraPaxFreeConfiguracao, RegraPaxFreeConfiguracaoModel>()
                .ForPath(dest => dest.RegraPaxFreeId, opt => opt.MapFrom(src => src.RegraPaxFree.Id));

            #endregion

            #region Usuario tags usuario

            CreateMap<UsuarioTags, UsuarioTagsModel>()
                .ForPath(dest => dest.UsuarioId, opt => opt.MapFrom(src => src.Usuario.Id))
                .ForPath(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags));

            #endregion

            #region Grupo Faq e Faq

            CreateMap<GrupoFaq, GrupoFaqModel>()
                .ForPath(dest => dest.EmpresaId, opt => opt.MapFrom(src => src.Empresa.Id));

            CreateMap<GrupoFaq, GrupoFaqModelSimplificado>()
                .ForPath(dest => dest.EmpresaId, opt => opt.MapFrom(src => src.Empresa.Id)); ;

            CreateMap<GrupoFaqTags, GrupoFaqTagsModel>()
                .ForPath(dest => dest.GrupoFaqId, opt => opt.MapFrom(src => src.GrupoFaq.Id));

            CreateMap<Faq, FaqModel>()
                .ForPath(dest => dest.GrupoFaq, opt => opt.MapFrom(src => src.GrupoFaq));

            CreateMap<FaqTags, FaqTagsModel>()
                .ForPath(dest => dest.FaqId, opt => opt.MapFrom(src => src.Faq.Id));

            CreateMap<Faq, FaqModelSimplificado>()
                .ForPath(dest => dest.GrupoFaq, opt => opt.MapFrom(src => src.GrupoFaq))
                .ForPath(dest => dest.DataHoraCriacao, opt => opt.MapFrom(src => src.DataHoraCriacao))
                .ForPath(dest => dest.Pergunta, opt => opt.MapFrom(src => src.Pergunta))
                .ForPath(dest => dest.Resposta, opt => opt.MapFrom(src => src.Resposta))
                .ForPath(dest => dest.UsuarioCriacao, opt => opt.MapFrom(src => src.UsuarioCriacao))
                .ForPath(dest => dest.UsuarioAlteracao, opt => opt.MapFrom(src => src.UsuarioAlteracao))
                .ForPath(dest => dest.DataHoraAlteracao, opt => opt.MapFrom(src => src.DataHoraAlteracao));


            #endregion

            #region tags

            CreateMap<Tags, TagsModel>()
                .ForMember(dest => dest.ParentId, opt =>
                {
                    opt.Condition(src => src.Parent != null);
                    opt.MapFrom(src => src.Parent.Id);
                });

            #endregion

            #region tags

            CreateMap<HtmlTemplate, HtmlTemplateModel>();

            #endregion

            #region Email

            CreateMap<Email, EmailModel>()
                .ForMember(dest => dest.EmpresaId, opt =>
                {
                    opt.Condition(src => src.Empresa != null);
                    opt.MapFrom(src => src.Empresa.Id);
                })
                .ForMember(dest => dest.Anexos, opt =>
                {
                    opt.Condition(src => src.Anexos != null && src.Anexos.Any());
                    opt.MapFrom(src => src.Anexos.Select(a => new EmailAnexoModel
                    {
                        Id = a.Id,
                        NomeArquivo = a.NomeArquivo,
                        TipoMime = a.TipoMime,
                        Arquivo = a.Arquivo
                    }).ToList());
                });

            CreateMap<EmailAnexo, EmailAnexoModel>();

            CreateMap<EmailAnexoInputModel, EmailAnexo>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ObjectGuid, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.Email, opt => opt.Ignore());

            #endregion

            #region Grupo de Imagem e Imagens

            CreateMap<GrupoImagem, ImageGroupModel>()
                .ForPath(dest => dest.CompanyId, opt => opt.MapFrom(src => src.Empresa.Id))
                .ForPath(dest => dest.Name, opt => opt.MapFrom(src => src.Nome));

            CreateMap<GrupoImagemTags, GrupoImagemTagsModel>()
            .ForPath(dest => dest.GrupoImagemId, opt => opt.MapFrom(src => src.GrupoImagem.Id));

            CreateMap<ImagemGrupoImagem, ImageGroupImageModel>()
                .ForPath(dest => dest.ImageGroupId, opt => opt.MapFrom(src => src.GrupoImagem != null ? src.GrupoImagem.Id : (int?)null))
                .ForPath(dest => dest.ImageGroupName, opt => opt.MapFrom(src => src.GrupoImagem != null ? src.GrupoImagem.Nome : null))
                .ForPath(dest => dest.Name, opt => opt.MapFrom(src => src.Nome))
                .ForMember(dest => dest.Imagem, opt => opt.MapFrom(src => src.Imagem))
                .ForMember(dest => dest.NomeBotao, opt => opt.MapFrom(src => src.NomeBotao))
                .ForMember(dest => dest.LinkBotao, opt => opt.MapFrom(src => src.LinkBotao))
                .ForMember(dest => dest.DataInicioVigencia, opt => opt.MapFrom(src => src.DataInicioVigencia))
                .ForMember(dest => dest.DataFimVigencia, opt => opt.MapFrom(src => src.DataFimVigencia));

            CreateMap<ImagemGrupoImagemTags, ImagemGrupoImagemTagsModel>()
            .ForPath(dest => dest.ImagemGrupoImagemId, opt => opt.MapFrom(src => src.ImagemGrupoImagem.Id));

            #endregion

            #region Grupo de Imagem Home e Imagens Home

            CreateMap<GrupoImagemHome, GrupoImagemHomeModel>()
                .ForPath(dest => dest.CompanyId, opt => opt.MapFrom(src => src.Empresa.Id))
                .ForPath(dest => dest.Name, opt => opt.MapFrom(src => src.Nome));

            CreateMap<GrupoImagemHomeTags, GrupoImagemHomeTagsModel>()
                .ForPath(dest => dest.GrupoImagemHomeId, opt => opt.MapFrom(src => src.GrupoImagemHome.Id))
                .ForPath(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags));

            CreateMap<ImagemGrupoImagemHome, ImagemGrupoImagemHomeModel>()
                .ForPath(dest => dest.GrupoImagemHomeId, opt => opt.MapFrom(src => src.GrupoImagemHome.Id))
                .ForPath(dest => dest.GrupoImagemHomeName, opt => opt.MapFrom(src => src.GrupoImagemHome.Nome))
                .ForPath(dest => dest.Name, opt => opt.MapFrom(src => src.Nome))
                .ForPath(dest => dest.ImagemBase64, opt => opt.MapFrom(src => src.Imagem != null ? Convert.ToBase64String(src.Imagem) : null));

            CreateMap<ImagemGrupoImagemHomeTags, ImagemGrupoImagemHomeTagsModel>()
                .ForPath(dest => dest.ImagemGrupoImagemHomeId, opt => opt.MapFrom(src => src.ImagemGrupoImagemHome.Id))
                .ForPath(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags));

            #endregion

            #region Parametro Sistema

            CreateMap<ParametroSistema, ParametroSistemaViewModel>()
                .ForPath(dest => dest.EmpresaId, opt => opt.MapFrom(src => src.Empresa.Id))
                .ForMember(dest => dest.ExigeEnderecoHospedeConvidado, opt => opt.MapFrom(src => src.ExigeEnderecoHospedeConvidado))
                .ForMember(dest => dest.ExigeTelefoneHospedeConvidado, opt => opt.MapFrom(src => src.ExigeTelefoneHospedeConvidado))
                .ForMember(dest => dest.ExigeDocumentoHospedeConvidado, opt => opt.MapFrom(src => src.ExigeDocumentoHospedeConvidado))
                .ForMember(dest => dest.PermiteReservaRciApenasClientesComContratoRci, opt => opt.MapFrom(src => src.PermiteReservaRciApenasClientesComContratoRci))
                .ForMember(dest => dest.Habilitar2FAPorEmail, opt => opt.MapFrom(src => src.Habilitar2FAPorEmail))
                .ForMember(dest => dest.Habilitar2FAPorSms, opt => opt.MapFrom(src => src.Habilitar2FAPorSms))
                .ForMember(dest => dest.Habilitar2FAParaCliente, opt => opt.MapFrom(src => src.Habilitar2FAParaCliente))
                .ForMember(dest => dest.Habilitar2FAParaAdministrador, opt => opt.MapFrom(src => src.Habilitar2FAParaAdministrador));


            #endregion

            #region RabbitMQ Queue

            CreateMap<RabbitMQQueue, RabbitMQQueueViewModel>();

            #endregion

            #region DocumentTemplate

            CreateMap<DocumentTemplate, DocumentTemplateModel>();
            CreateMap<DocumentTemplate, DocumentTemplateSummaryModel>();

            #endregion

            #region AutomaticCommunicationConfig

            CreateMap<AutomaticCommunicationConfig, AutomaticCommunicationConfigModel>();

            #endregion

            #region AuditLog

            CreateMap<AuditLog, AuditLogModel>()
                .ForMember(dest => dest.Changes, opt => opt.Ignore()); // Changes são deserializados manualmente no service

            #endregion

        }

        private void ConfiguraModelToEntityMappings()
        {
            #region Pais 

            CreateMap<RegistroPaisInputModel, Pais>()
               .ForMember(dest => dest.Id, opt => opt.Ignore())
               .ForMember(dest => dest.ObjectGuid, opt => opt.Ignore())
               .ForMember(dest => dest.DataHoraCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAlteracao, opt => opt.Ignore())
                .ForAllMembers(opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<AlteracaoPaisInputModel, Pais>()
               .ForMember(dest => dest.Id, opt => opt.Ignore())
               .ForMember(dest => dest.ObjectGuid, opt => opt.Ignore())
               .ForMember(dest => dest.DataHoraCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAlteracao, opt => opt.Ignore())
                .ForAllMembers(opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));

            #endregion

            #region Estado

            CreateMap<RegistroEstadoInputModel, Estado>()
               .ForMember(dest => dest.Id, opt => opt.Ignore())
               .ForMember(dest => dest.ObjectGuid, opt => opt.Ignore())
               .ForMember(dest => dest.DataHoraCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.Pais, opt =>
                {
                    opt.Condition(src => src.PaisId.GetValueOrDefault(0) > 0);
                    opt.MapFrom(src => new Pais() { Id = src.PaisId.GetValueOrDefault() });
                })
                .ForAllMembers(opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<AlteracaoEstadoInputModel, Estado>()
                .ForMember(dest => dest.Pais, opt =>
                {
                    opt.Condition(src => src.PaisId.GetValueOrDefault(0) > 0);
                    opt.MapFrom(src => new Pais() { Id = src.PaisId.GetValueOrDefault() });
                })
                .ForAllMembers(opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));

            #endregion

            #region Cidade

            CreateMap<RegistroCidadeInputModel, Cidade>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ObjectGuid, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.Estado, opt =>
                {
                    opt.Condition(src => src.EstadoId.GetValueOrDefault(0) > 0);
                    opt.MapFrom(src => new Estado() { Id = src.EstadoId.GetValueOrDefault() });
                })
                .ForAllMembers(opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<AlteracaoCidadeInputModel, Cidade>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ObjectGuid, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.Estado, opt =>
                {
                    opt.Condition(src => src.EstadoId.GetValueOrDefault(0) > 0);
                    opt.MapFrom(src => new Estado() { Id = src.EstadoId.GetValueOrDefault() });
                })
                .ForAllMembers(opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));

            #endregion

            #region Pessoa

            #region Pessoa física

            CreateMap<PessoaFisicaInputModel, Pessoa>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ObjectGuid, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAlteracao, opt => opt.Ignore())
                .ForPath(dest => dest.EmailPreferencial, opt => opt.MapFrom(crc => crc.EmailPreferencial))
                .ForPath(dest => dest.EmailAlternativo, opt => opt.MapFrom(crc => crc.EmailAlternativo))
                .ForAllMembers(opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));

            #endregion

            #region Pessoa jurídica

            CreateMap<PessoaJuridicaInputModel, Pessoa>()
               .ForMember(dest => dest.Id, opt => opt.Ignore())
               .ForMember(dest => dest.ObjectGuid, opt => opt.Ignore())
               .ForMember(dest => dest.DataHoraCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAlteracao, opt => opt.Ignore())
                .ForPath(dest => dest.Nome, opt => opt.MapFrom(src => src.RazaoSocial))
                .ForPath(dest => dest.EmailPreferencial, opt => opt.MapFrom(crc => crc.EmailPreferencial))
                .ForPath(dest => dest.EmailAlternativo, opt => opt.MapFrom(crc => crc.EmailAlternativo))
               .ForAllMembers(opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));


            #endregion

            #region Pessoa telefone

            CreateMap<PessoaTelefoneInputModel, PessoaTelefone>()
               .ForMember(dest => dest.Id, opt => opt.Ignore())
               .ForMember(dest => dest.ObjectGuid, opt => opt.Ignore())
               .ForMember(dest => dest.DataHoraCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAlteracao, opt => opt.Ignore())
               .ForMember(dest => dest.Pessoa, opt =>
               {
                   opt.Condition(src => src.PessoaId.GetValueOrDefault(0) > 0);
                   opt.MapFrom(src => new Pessoa() { Id = src.PessoaId.GetValueOrDefault() });
               })
               .ForAllMembers(opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));



            #endregion

            #region Pessoa endereço

            CreateMap<PessoaEnderecoInputModel, PessoaEndereco>()
               .ForMember(dest => dest.Id, opt => opt.Ignore())
               .ForMember(dest => dest.ObjectGuid, opt => opt.Ignore())
               .ForMember(dest => dest.DataHoraCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAlteracao, opt => opt.Ignore())
               .ForMember(dest => dest.Pessoa, opt =>
               {
                   opt.Condition(src => src.PessoaId.GetValueOrDefault(0) > 0);
                   opt.MapFrom(src => new Pessoa() { Id = src.PessoaId.GetValueOrDefault() });
               })
               .ForMember(dest => dest.TipoEndereco, opt =>
               {
                   opt.Condition(src => src.TipoEnderecoId.GetValueOrDefault(0) > 0);
                   opt.MapFrom(src => new TipoEndereco() { Id = src.TipoEnderecoId.GetValueOrDefault() });
               })
               .ForMember(dest => dest.Cidade, opt =>
               {
                   opt.Condition(src => src.CidadeId.GetValueOrDefault(0) > 0);
                   opt.MapFrom(src => new Cidade() { Id = src.CidadeId.GetValueOrDefault() });
               })
               .ForAllMembers(opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));

            #endregion

            #region Pessoa endereço

            CreateMap<PessoaTelefoneInputModel, PessoaTelefone>()
              .ForMember(dest => dest.Id, opt => opt.Ignore())
              .ForMember(dest => dest.ObjectGuid, opt => opt.Ignore())
               .ForMember(dest => dest.DataHoraCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAlteracao, opt => opt.Ignore())
               .ForMember(dest => dest.Pessoa, opt =>
               {
                   opt.Condition(src => src.PessoaId.GetValueOrDefault(0) > 0);
                   opt.MapFrom(src => new Pessoa() { Id = src.PessoaId.GetValueOrDefault() });
               })
               .ForMember(dest => dest.TipoTelefone, opt =>
               {
                   opt.Condition(src => src.TipoTelefoneId.GetValueOrDefault(0) > 0);
                   opt.MapFrom(src => new TipoTelefone() { Id = src.TipoTelefoneId.GetValueOrDefault() });
               })
               .ForAllMembers(opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));

            #endregion

            #region Pessoa documento

            CreateMap<PessoaDocumentoInputModel, PessoaDocumento>()
               .ForMember(dest => dest.Id, opt => opt.Ignore())
               .ForMember(dest => dest.ObjectGuid, opt => opt.Ignore())
               .ForMember(dest => dest.DataHoraCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAlteracao, opt => opt.Ignore())
               .ForMember(dest => dest.Pessoa, opt =>
               {
                   opt.Condition(src => src.PessoaId.GetValueOrDefault(0) > 0);
                   opt.MapFrom(src => new Pessoa() { Id = src.PessoaId.GetValueOrDefault() });
               })
               .ForMember(dest => dest.TipoDocumento, opt =>
               {
                   opt.Condition(src => src.TipoDocumentoId.GetValueOrDefault(0) > 0);
                   opt.MapFrom(src => new TipoDocumentoPessoa() { Id = src.TipoDocumentoId.GetValueOrDefault() });
               })
               .ForAllMembers(opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));

            #endregion

            #endregion

            #region Grupo Empresa 


            CreateMap<RegistroGrupoEmpresaInputModel, GrupoEmpresa>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ObjectGuid, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.Pessoa, opt =>
                {
                    opt.MapFrom(src => src.Pessoa);
                })
                .ForPath(dest => dest.Pessoa.Nome, opt => opt.MapFrom(src => src.Pessoa.RazaoSocial))
                .ForAllMembers(opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<AlteracaoGrupoEmpresaInputModel, GrupoEmpresa>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ObjectGuid, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.Pessoa, opt =>
                {
                    opt.MapFrom(src => src.Pessoa);
                })
                .ForPath(dest => dest.Pessoa.Nome, opt => opt.MapFrom(src => src.Pessoa.RazaoSocial))
                .ForAllMembers(opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));


            #endregion

            #region Empresa

            CreateMap<RegistroEmpresaInputModel, Empresa>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ObjectGuid, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.GrupoEmpresa, opt =>
                {
                    opt.MapFrom(src => src.GrupoEmpresaId.GetValueOrDefault(0) > 0 ? new GrupoEmpresa() { Id = src.GrupoEmpresaId.GetValueOrDefault() } : null);
                })
                .ForMember(dest => dest.Pessoa, opt =>
                {
                    opt.MapFrom(src => src.Pessoa);
                })
                .ForAllMembers(opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<AlteracaoEmpresaInputModel, Empresa>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ObjectGuid, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.GrupoEmpresa, opt =>
                {
                    opt.MapFrom(src => src.GrupoEmpresaId.GetValueOrDefault(0) > 0 ? new GrupoEmpresa() { Id = src.GrupoEmpresaId.GetValueOrDefault() } : null);
                })
                .ForMember(dest => dest.Pessoa, opt =>
                {
                    opt.MapFrom(src => src.Pessoa);
                })
                .ForAllMembers(opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));


            #endregion

            #region Grupo Faq e Faq

            CreateMap<FaqGroupInputModel, GrupoFaq>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ObjectGuid, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAlteracao, opt => opt.Ignore())
                .ForAllMembers(opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<FaqInputModel, Faq>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ObjectGuid, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.GrupoFaq, opt =>
                {
                    opt.MapFrom(src => src.GrupoFaqId.GetValueOrDefault(0) > 0 ?
                    new GrupoFaq() { Id = src.GrupoFaqId.GetValueOrDefault() } : null);
                })
                .ForAllMembers(opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));


            #endregion

            #region Grupo Documento e Documento

            CreateMap<DocumentGroupInputModel, GrupoDocumento>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ObjectGuid, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAlteracao, opt => opt.Ignore())
                .ForAllMembers(opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<DocumentInputModel, Documento>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ObjectGuid, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.Arquivo, opt => opt.Ignore())
                .ForMember(dest => dest.NomeArquivo, opt => opt.Ignore())
                .ForMember(dest => dest.TipoMime, opt => opt.Ignore())
                .ForMember(dest => dest.GrupoDocumento, opt =>
                {
                    opt.MapFrom(src => src.GrupoDocumentoId.GetValueOrDefault(0) > 0 ?
                    new GrupoDocumento() { Id = src.GrupoDocumentoId.GetValueOrDefault() } : null);
                })
                .ForMember(dest => dest.Nome, opt => opt.MapFrom(src => src.Nome))
                .ForMember(dest => dest.DocumentoPublico, opt => opt.MapFrom(src => src.DocumentoPublico))
                .ForMember(dest => dest.Disponivel, opt => opt.MapFrom(src => src.Disponivel))
                .ForMember(dest => dest.DataInicioVigencia, opt => opt.Ignore()) // Handled manually in service
                .ForMember(dest => dest.DataFimVigencia, opt => opt.Ignore()) // Handled manually in service
                .ForAllMembers(opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<AlteracaoDocumentInputModel, Documento>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ObjectGuid, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.DataInicioVigencia, opt => opt.Ignore()) // Handled manually in service
                .ForMember(dest => dest.DataFimVigencia, opt => opt.Ignore()) // Handled manually in service
                .ForAllMembers(opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));

            #endregion

            #region RegraPaxFree

            CreateMap<RegraPaxFreeInputModel, RegraPaxFree>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ObjectGuid, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.DataInicioVigencia, opt => opt.MapFrom(src => src.DataInicioVigencia))
                .ForMember(dest => dest.DataFimVigencia, opt => opt.MapFrom(src => src.DataFimVigencia))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<AlteracaoRegraPaxFreeInputModel, RegraPaxFree>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ObjectGuid, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.DataInicioVigencia, opt => opt.MapFrom(src => src.DataInicioVigencia))
                .ForMember(dest => dest.DataFimVigencia, opt => opt.MapFrom(src => src.DataFimVigencia))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            #endregion

            #region Tags 

            CreateMap<TagsInputModel, Tags>()
               .ForMember(dest => dest.Id, opt => opt.Ignore())
               .ForMember(dest => dest.ObjectGuid, opt => opt.Ignore())
               .ForMember(dest => dest.DataHoraCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.Parent, opt =>
                {
                    opt.MapFrom(src => src.TagsParentId.GetValueOrDefault(0) > 0 ?
                    new Tags() { Id = src.TagsParentId.GetValueOrDefault() } : null);
                })
                .ForAllMembers(opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));


            #endregion

            #region HtmlTemplate 

            CreateMap<HtmlTemplateInputModel, HtmlTemplate>()
               .ForMember(dest => dest.Id, opt => opt.Ignore())
               .ForMember(dest => dest.ObjectGuid, opt => opt.Ignore())
               .ForMember(dest => dest.DataHoraCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAlteracao, opt => opt.Ignore())
                .ForAllMembers(opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));


            #endregion

            #region Email

            CreateMap<EmailModel, Email>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ObjectGuid, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.Empresa, opt =>
                {
                    opt.MapFrom(src => src.EmpresaId.GetValueOrDefault(0) > 0 ?
                    new Empresa() { Id = src.EmpresaId.GetValueOrDefault() } : null);
                })
                .ForAllMembers(opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<EmailInputInternalModel, Email>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ObjectGuid, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.Empresa, opt =>
                {
                    opt.MapFrom(src => src.EmpresaId.GetValueOrDefault(0) > 0 ?
                    new Empresa() { Id = src.EmpresaId.GetValueOrDefault() } : null);
                })
                .ForAllMembers(opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<EmailInputModel, Email>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ObjectGuid, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.Empresa, opt =>
                {
                    opt.MapFrom(src => src.EmpresaId.GetValueOrDefault(0) > 0 ?
                    new Empresa() { Id = src.EmpresaId.GetValueOrDefault() } : null);
                })
                .ForAllMembers(opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<AlteracaoEmailInputModel, Email>()
                .ForMember(dest => dest.ObjectGuid, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.Empresa, opt =>
                {
                    opt.MapFrom(src => src.EmpresaId.GetValueOrDefault(0) > 0 ?
                    new Empresa() { Id = src.EmpresaId.GetValueOrDefault() } : null);
                })
                .ForAllMembers(opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));

            #endregion

            #region Grupo Documento e Documento

            CreateMap<ImageGroupInputModel, GrupoImagem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ObjectGuid, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.Empresa, opt =>
                {
                    opt.MapFrom(src => src.CompanyId.GetValueOrDefault(0) > 0 ? new Empresa() { Id = src.CompanyId.GetValueOrDefault() } : null);
                })
                .ForMember(dest => dest.Nome, opt =>
                {
                    opt.MapFrom(src => src.Name);
                })
                .ForAllMembers(opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<ImageGroupImageInputModel, ImagemGrupoImagem>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ObjectGuid, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.Nome, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Imagem, opt => opt.Ignore()) // Será preenchido manualmente no service
                .ForMember(dest => dest.GrupoImagem, opt =>
                {
                    opt.MapFrom(src => src.ImageGroupId.GetValueOrDefault(0) > 0 ?
                    new GrupoImagem() { Id = src.ImageGroupId.GetValueOrDefault() } : null);
                })
                .ForAllMembers(opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));

            #endregion

            #region Grupo de Imagem Home e Imagens Home

            CreateMap<GrupoImagemHomeInputModel, GrupoImagemHome>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ObjectGuid, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.Empresa, opt =>
                {
                    opt.MapFrom(src => src.CompanyId.GetValueOrDefault(0) > 0 ? new Empresa() { Id = src.CompanyId.GetValueOrDefault() } : null);
                })
                .ForMember(dest => dest.Nome, opt =>
                {
                    opt.MapFrom(src => src.Name);
                })
                .ForAllMembers(opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<ImagemGrupoImagemHomeInputModel, ImagemGrupoImagemHome>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ObjectGuid, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.GrupoImagemHome, opt =>
                {
                    opt.MapFrom(src => src.GrupoImagemHomeId.GetValueOrDefault(0) > 0 ?
                    new GrupoImagemHome() { Id = src.GrupoImagemHomeId.GetValueOrDefault() } : null);
                })
                .ForMember(dest => dest.Nome, opt =>
                {
                    opt.MapFrom(src => src.Name);
                })
                .ForMember(dest => dest.Imagem, opt => opt.Ignore()) // Será tratado manualmente no service
                .ForAllMembers(opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));

            #endregion

            #region Parametro Sistema

            CreateMap<ParametroSistemaInputUpdateModel, ParametroSistema>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ObjectGuid, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.EmitirCertidaoPorUnidCliente, opt => opt.MapFrom(a => a.EmitirCertidaoPorUnidCliente))
                .ForMember(dest => dest.EmitirCertidaoPorUnidCliente, opt => opt.MapFrom(a => a.EmitirCertidaoPorUnidCliente))
                .ForMember(dest => dest.IntegradoComMultiPropriedade, opt => opt.MapFrom(a => a.IntegradoComMultiPropriedade))
                .ForMember(dest => dest.IntegradoComTimeSharing, opt => opt.MapFrom(a => a.IntegradoComTimeSharing))
                .ForMember(dest => dest.NomeCondominio, opt => opt.MapFrom(a => a.NomeCondominio))
                .ForMember(dest => dest.CnpjCondominio, opt => opt.MapFrom(a => a.CnpjCondominio))
                .ForMember(dest => dest.EnderecoCondominio, opt => opt.MapFrom(a => a.EnderecoCondominio))
                .ForMember(dest => dest.NomeAdministradoraCondominio, opt => opt.MapFrom(a => a.NomeAdministradoraCondominio))
                .ForMember(dest => dest.CnpjAdministradoraCondominio, opt => opt.MapFrom(a => a.CnpjAdministradoraCondominio))
                .ForMember(dest => dest.EnderecoAdministradoraCondominio, opt => opt.MapFrom(a => a.EnderecoAdministradoraCondominio))
                .ForMember(dest => dest.ExibirFinanceirosDasEmpresaIds, opt => opt.MapFrom(a => a.ExibirFinanceirosDasEmpresaIds))
                .ForMember(dest => dest.ExigeEnderecoHospedeConvidado, opt => opt.MapFrom(a => a.ExigeEnderecoHospedeConvidado))
                .ForMember(dest => dest.ExigeTelefoneHospedeConvidado, opt => opt.MapFrom(a => a.ExigeTelefoneHospedeConvidado))
                .ForMember(dest => dest.ExigeDocumentoHospedeConvidado, opt => opt.MapFrom(a => a.ExigeDocumentoHospedeConvidado))
                .ForMember(dest => dest.PermiteReservaRciApenasClientesComContratoRci, opt => opt.MapFrom(a => a.PermiteReservaRciApenasClientesComContratoRci))
                .ForMember(dest => dest.Habilitar2FAPorEmail, opt => opt.MapFrom(a => a.Habilitar2FAPorEmail))
                .ForMember(dest => dest.Habilitar2FAPorSms, opt => opt.MapFrom(a => a.Habilitar2FAPorSms))
                .ForMember(dest => dest.Habilitar2FAParaCliente, opt => opt.MapFrom(a => a.Habilitar2FAParaCliente))
                .ForMember(dest => dest.Habilitar2FAParaAdministrador, opt => opt.MapFrom(a => a.Habilitar2FAParaAdministrador))
                .ForAllMembers(opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));

            #endregion

            #region RabbitMQ Queue

            CreateMap<RabbitMQQueueInputModel, RabbitMQQueue>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ObjectGuid, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataHoraAlteracao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAlteracao, opt => opt.Ignore())
                .ForAllMembers(opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));

            #endregion
        }
    }
}
