using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Geral;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Geral
{
    public class PaisMap : ClassMap<Pais>
    {
        public PaisMap()
        {
            Id(x => x.Id).GeneratedBy.Native("Pais_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            Map(b => b.CodigoIbge).Length(10);
            Map(b => b.Nome).Length(100);
            Map(b => b.Ddi).Length(3);
            Map(b => b.MascaraTelefoneCelular).Length(15);
            Map(b => b.MascaraTelefoneFixo).Length(15);
            Table("Pais");
        }
    }
}

