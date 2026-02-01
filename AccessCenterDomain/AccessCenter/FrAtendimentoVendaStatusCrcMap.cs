using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class FrAtendimentoVendaStatusCrcMap : ClassMap<FrAtendimentoVendaStatusCrc>
    {
        public FrAtendimentoVendaStatusCrcMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FRATENDIMENTOVENDASTATUSCRC_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.FrAtendimentoVenda);
            Map(b => b.IntegracaoId);
            Map(b => b.Status);
            Map(b => b.DataHoraInativacao);
            Map(b => b.Observacao);
            Map(b => b.FrStatusCrc);

            Table("FrAtendimentoVendaStatusCrc");
        }
    }
}
