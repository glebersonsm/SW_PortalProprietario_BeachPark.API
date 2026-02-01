using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class FrCrcTipoAtendimentoMap : ClassMap<FrCrcTipoAtendimento>
    {
        public FrCrcTipoAtendimentoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FRCRCTIPOATENDIMENTO_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.Codigo);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(b => b.Status);

            Table("FrCrcTipoAtendimento");
        }
    }
}
