using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class FrLancamentoPontoFuturoMap : ClassMap<FrLancamentoPontoFuturo>
    {
        public FrLancamentoPontoFuturoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FRLANCAMENTOPONTOFUTURO_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.GrupoEmpresa);
            Map(b => b.Empresa);
            Map(b => b.Filial);
            Map(b => b.DataSolicitacao);
            Map(b => b.FrAtendimentoVenda);
            Map(b => b.FrTipoBaixaPonto);
            Map(b => b.FrLancamentoDpnu);
            Map(b => b.TotalPontos);
            Map(b => b.TotalPontoUtilizado);
            Map(b => b.QuantidadeDiasTotal);
            Map(b => b.QuantidadeDiasUtilizado);
            Map(b => b.DataLimiteUtilizacao);
            Map(b => b.Estornado);

            Table("FrLancamentoPontoFuturo");
        }
    }
}
