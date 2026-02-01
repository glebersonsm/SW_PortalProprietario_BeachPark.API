using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class RegimeCasamentoMap : ClassMap<RegimeCasamento>
    {
        public RegimeCasamentoMap()
        {
            Id(x => x.Id).GeneratedBy.Sequence("REGIMECASAMENTO_");
            Map(b => b.Tag);
            Map(b => b.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.UsuarioAlteracao);
            Map(b => b.DataHoraAlteracao);

            Map(b => b.Codigo);
            Map(b => b.Nome);

            Table("RegimeCasamento");
        }
    }
}
