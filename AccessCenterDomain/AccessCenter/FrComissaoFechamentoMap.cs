using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;

namespace AccessCenterDomain.AccessCenter
{
    public class FrComissaoFechamentoMap : ClassMap<FrComissaoFechamento>
    {
        public FrComissaoFechamentoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FRCOMISSAOFECHAMENTO_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.ContaPagar);
            Map(b => b.FrUsuario);
            Map(b => b.Total);
            Map(b => b.MesReferencia);
            Map(b => b.AnoReferencia);
            Map(b => b.DataInicial);
            Map(b => b.DataFinal);
            Map(b => b.Filial);
            Map(b => b.Observacao);
            Map(b => b.Liquidacao);
            Map(b => b.DesligamentoColaborador);
            Map(b => b.ConsiderarLancamentosAnt);

            Table("FrComissaoFechamento");
        }
    }
}
