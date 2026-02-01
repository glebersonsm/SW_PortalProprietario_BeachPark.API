using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class EstadoMap : ClassMap<Estado>
    {
        public EstadoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("ESTADO_SEQUENCE");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(p => p.Pais);
            Map(p => p.CodigoIbge);
            Map(p => p.Uf);

            Table("Estado");
        }
    }
}
