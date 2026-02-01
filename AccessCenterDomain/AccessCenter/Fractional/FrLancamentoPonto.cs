namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrLancamentoPonto : EntityBase
    {
        public virtual int? GrupoEmpresa { get; set; } = 1;
        public virtual int? Empresa { get; set; } = 1;
        public virtual int? Filial { get; set; } = 1;
        public virtual int? FrAtendimentoVenda { get; set; }
        public virtual int? FrTipoBaixaPonto { get; set; }
        public virtual int? FrLancamentoDpnu { get; set; }
        public virtual string Reserva { get; set; }
        public virtual DateTime? DataSolicitacao { get; set; }
        public virtual decimal? Pontos { get; set; }
        public virtual decimal? PontosFracionamento { get; set; }
        public virtual decimal? PontosNormal { get; set; }
        public virtual decimal? PontosPensao { get; set; }
        public virtual DateTime? DataHoraConfirmacao { get; set; }
        public virtual int? UsuarioConfirmacao { get; set; }
        public virtual string ObservacaoConfirmacao { get; set; }
        public virtual string Confirmado { get; set; } = "N";
        public virtual int? FrHotel { get; set; }
        public virtual int? FrHotelTipoUh { get; set; }
        public virtual DateTime? Checkin { get; set; }
        public virtual DateTime? Checkout { get; set; }
        public virtual int? QuantidadeAdulto { get; set; }
        public virtual int? QuantidadeCrianca1 { get; set; }
        public virtual int? QuantidadeCrianca2 { get; set; }
        public virtual string Observacao { get; set; }
        public virtual string HospedeNome { get; set; }
        public virtual string HospedeCpf { get; set; }
        public virtual string Estornado { get; set; } = "N";
        public virtual string PontosDebitoCredito { get; set; } = "D";
        public virtual string PontosPensaoDebitoCredito { get; set; } = "D";
        public virtual string LancamentoEstorno { get; set; } = "N";
        public virtual int? FrLancamentoPontoEstorno { get; set; }
        public virtual int? Cidade { get; set; }
        public virtual decimal? ValorTaxaUtilizacao { get; set; }
        public virtual string TipoOperacaoLancamentoPonto { get; set; } = "N";
        public virtual string ContaReceberTaxaUtiGer { get; set; } = "N";
        public virtual int? FrLancamentoPontoReserva { get; set; }
        public virtual string IntegracaoId { get; set; }

    }
}
