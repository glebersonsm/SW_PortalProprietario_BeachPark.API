using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class FrLancamentoPontoReservaMap : ClassMap<FrLancamentoPontoReserva>
    {
        public FrLancamentoPontoReservaMap()
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
            Map(b => b.FrLancamentoPonto);
            Map(b => b.Reserva);
            Map(b => b.DataHoraCancelamento);
            Map(b => b.Cancelado);
            Map(b => b.UsuarioCancelamento);

            Table("FrLancamentoPontoReserva");
        }
    }
}
