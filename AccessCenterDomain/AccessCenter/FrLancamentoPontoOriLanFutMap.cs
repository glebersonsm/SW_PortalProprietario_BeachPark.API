using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class FrLancamentoPontoOriLanFutMap : ClassMap<FrLancamentoPontoOriLanFut>
    {
        public FrLancamentoPontoOriLanFutMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FRLANCAMENTOPONTOORILANFUT_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.GrupoEmpresa);
            Map(b => b.Empresa);
            Map(b => b.Filial);
            Map(b => b.PontoUtilizado);
            Map(b => b.FrLancamentoPontoVinculado);
            Map(b => b.FrLancamentoPontoFuturo);
            Map(b => b.LancamentoEstorno);

            Table("FrLancamentoPontoOriLanFut");
        }
    }
}
