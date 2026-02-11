using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Geral
{
    public class GrupoDocumentoMap : ClassMap<GrupoDocumento>
    {
        public GrupoDocumentoMap()
        {
            Id(x => x.Id).GeneratedBy.Native("GrupoDocumento_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            Map(p => p.UsuarioRemocao).Nullable();
            Map(p => p.DataHoraRemocao).Nullable();

            References(b => b.Empresa, "Empresa");
            References(b => b.GrupoDocumentoPai, "IdGrupoDocumentoPai").Nullable();
            Map(b => b.Nome).Length(100);
            Map(b => b.Disponivel).CustomType<EnumSimNao>();
            Map(b => b.Ordem).Nullable();
            Map(b => b.Cor).Length(50).Nullable();
            Map(b => b.CorTexto).Length(50).Nullable();

            Schema("portalohana");
            Table("GrupoDocumento");
        }
    }
}

