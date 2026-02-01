using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Geral;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Geral
{
    public class CidadeMap : ClassMap<Cidade>
    {
        public CidadeMap()
        {
            Id(x => x.Id).GeneratedBy.Native("Cidade_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            Map(b => b.CodigoIbge).Length(10);
            Map(b => b.Nome).Length(100);
            Map(b => b.NomePesquisa).Length(100);
            References(p => p.Estado, "Estado");

            Table("Cidade");
        }

    }
}
