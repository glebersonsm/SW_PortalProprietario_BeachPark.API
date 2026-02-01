using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class FrAtendimentoVendaParcelaMap : ClassMap<FrAtendimentoVendaParcela>
    {
        public FrAtendimentoVendaParcelaMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FRATENDIMENTOVENDAPARCELA_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.FrAtendimentoVenda);
            Map(b => b.QuantidadeParcela);
            Map(b => b.ValorTotal);
            Map(b => b.ValorTotalAmortizado);
            Map(b => b.TipoContaReceber);
            Map(b => b.TipoParcela);
            Map(b => b.PrimeiroVencimento);
            Map(b => b.TipoBaseCalculoValor);
            Map(b => b.ValorParcela);
            Map(b => b.ClienteCartaoCredito);

            Table("FrAtendimentoVendaParcela");
        }
    }
}
