using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class FrTipoBaixaPontoMap : ClassMap<FrTipoBaixaPonto>
    {
        public FrTipoBaixaPontoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FRTIPOBAIXAPONTO_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.GrupoEmpresa);
            Map(b => b.Empresa);
            Map(b => b.Filial);
            Map(b => b.Codigo);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(b => b.Status);
            Map(b => b.Reserva);
            Map(b => b.DebitoCredito);
            Map(b => b.LancamentoFuturo);
            Map(b => b.LimiteUtilizacao);
            Map(b => b.LancamentoManual);
            Map(b => b.Dpnu);
            Map(b => b.ComporTotalPontosContrato);

            Table("FrTipoBaixaPonto");
        }
    }
}
