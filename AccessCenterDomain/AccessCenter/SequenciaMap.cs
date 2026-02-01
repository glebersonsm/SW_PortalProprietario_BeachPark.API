using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class SequenciaMap : ClassMap<Sequencia>
    {
        public SequenciaMap()
        {
            Id(x => x.Id).GeneratedBy.Sequence("SEQUENCIA_SEQUENCE");
            Map(b => b.Tag);
            Map(b => b.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.UsuarioAlteracao);
            Map(b => b.DataHoraAlteracao);

            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(b => b.Inicio);
            Map(b => b.Fim);
            Map(b => b.Proximo);

            Table("Sequencia");
        }
    }
}
