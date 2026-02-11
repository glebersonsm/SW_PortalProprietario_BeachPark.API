using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Geral;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Geral
{
    public class EmailAnexoMap : ClassMap<EmailAnexo>
    {
        public EmailAnexoMap()
        {
            Id(x => x.Id).GeneratedBy.Native("EmailAnexo_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();

            References(x => x.Email, "Email").Not.Nullable();
            Map(b => b.NomeArquivo).Length(500).Not.Nullable();
            Map(b => b.TipoMime).Length(100).Not.Nullable();
            Map(b => b.Arquivo).CustomType("BinaryBlob");

            Schema("portalohana");
            Table("EmailAnexo");
        }
    }
}

