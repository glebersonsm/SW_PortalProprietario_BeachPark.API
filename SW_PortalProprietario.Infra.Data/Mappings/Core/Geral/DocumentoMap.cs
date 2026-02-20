using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Geral
{
    public class DocumentoMap : ClassMap<Documento>
    {
        public DocumentoMap()
        {
            Id(x => x.Id).GeneratedBy.Native("Documento_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            Map(p => p.UsuarioRemocao).Nullable();
            Map(p => p.DataHoraRemocao).Nullable();

            References(b => b.GrupoDocumento, "GrupoDocumento");
            Map(b => b.Nome).Length(100);
            Map(b => b.Arquivo).CustomType("BinaryBlob");
            Map(b => b.NomeArquivo).Length(255).Nullable();
            Map(b => b.TipoMime).Length(100).Nullable();
            Map(b => b.Path).Length(500).Nullable();
            Map(b => b.Disponivel).CustomType<EnumSimNao>();
            Map(b => b.DocumentoPublico).CustomType<EnumSimNao>();
            Map(b => b.Ordem).Nullable();
            Map(b => b.DataInicioVigencia).Nullable();
            Map(b => b.DataFimVigencia).Nullable();
            Map(b => b.Cor).Length(50).Nullable();
            Map(b => b.CorTexto).Length(50).Nullable();

            Schema("portalohana");
            Table("Documento");
        }
    }
}

