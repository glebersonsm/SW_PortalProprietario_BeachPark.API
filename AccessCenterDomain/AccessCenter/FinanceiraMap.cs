using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class FinanceiraMap : ClassMap<Financeira>
    {
        public FinanceiraMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FINANCEIRA_SEQUENCE");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.Nome);
            Map(b => b.NomePesquisa);

            Table("Financeira");
        }
    }
}
