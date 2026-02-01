using FluentNHibernate.Mapping;


namespace EsolutionPortalDomain.Portal
{
    public class GrupoTipoSemanaMap : ClassMap<GrupoTipoSemana>
    {
        public GrupoTipoSemanaMap()
        {
            Id(x => x.Id)
            .GeneratedBy.SequenceIdentity();

            Map(p => p.Nome);
            Map(b => b.Empresa);
            Map(b => b.DataHoraCriacao);
            Map(b => b.UsuarioCriacao);
            Map(b => b.DataHoraExclusao);
            Map(b => b.UsuarioExlcusao);

            Table("GrupoTipoSemana");
        }
    }
}
