using FluentNHibernate.Mapping;


namespace EsolutionPortalDomain.Portal
{
    public class GrupoCotasMap : ClassMap<GrupoCotas>
    {
        public GrupoCotasMap()
        {
            Id(x => x.Id)
            .GeneratedBy.SequenceIdentity();

            Map(p => p.Nome);
            Map(b => b.Empresa);
            Map(b => b.DataHoraInclusao);
            Map(b => b.Usuario);
            Map(b => b.GrupoTipoSemana);

            Table("GrupoCotas");
        }
    }
}
