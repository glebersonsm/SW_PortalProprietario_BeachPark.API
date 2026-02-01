using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class FrLancamentoPontoMap : ClassMap<FrLancamentoPonto>
    {
        public FrLancamentoPontoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FRLANCAMENTOPONTO_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.GrupoEmpresa);
            Map(b => b.Empresa);
            Map(b => b.Filial);
            Map(b => b.FrAtendimentoVenda);
            Map(b => b.FrTipoBaixaPonto);
            Map(b => b.FrLancamentoDpnu);
            Map(b => b.Reserva);
            Map(b => b.DataSolicitacao);
            Map(b => b.Pontos);
            Map(b => b.PontosFracionamento);
            Map(b => b.PontosNormal);
            Map(b => b.PontosPensao);
            Map(b => b.DataHoraConfirmacao);
            Map(b => b.UsuarioConfirmacao);
            Map(b => b.ObservacaoConfirmacao);
            Map(b => b.Confirmado);
            Map(b => b.FrHotel);
            Map(b => b.FrHotelTipoUh);
            Map(b => b.Checkin);
            Map(b => b.Checkout);
            Map(b => b.QuantidadeAdulto);
            Map(b => b.QuantidadeCrianca1);
            Map(b => b.QuantidadeCrianca2);
            Map(b => b.Observacao);
            Map(b => b.HospedeNome);
            Map(b => b.HospedeCpf);
            Map(b => b.Estornado);
            Map(b => b.PontosDebitoCredito);
            Map(b => b.PontosPensaoDebitoCredito);
            Map(b => b.LancamentoEstorno);
            Map(b => b.FrLancamentoPontoEstorno);
            Map(b => b.Cidade);
            Map(b => b.ValorTaxaUtilizacao);
            Map(b => b.TipoOperacaoLancamentoPonto);
            Map(b => b.ContaReceberTaxaUtiGer);
            Map(b => b.FrLancamentoPontoReserva);
            Map(b => b.IntegracaoId);

            Table("FrLancamentoPonto");
        }
    }
}
