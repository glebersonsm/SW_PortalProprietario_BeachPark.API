using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class FrAtendimentoVendaContaRecMap : ClassMap<FrAtendimentoVendaContaRec>
    {
        public FrAtendimentoVendaContaRecMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FRATENDIMENTOVENDACONTAREC_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.FrAtendimentoVenda);
            Map(b => b.ContaReceber);
            Map(b => b.Origem);
            Map(b => b.FrProdutoParticipante);
            Map(b => b.Empresa);

            Table("FrAtendimentoVendaContaRec");
        }
    }
}
