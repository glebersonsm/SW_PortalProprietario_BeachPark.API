namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrAtendimentoBrinde : EntityBase
    {
        public virtual int? FrAtendimento { get; set; }
        public virtual int? FrBrindeFase { get; set; }
        public virtual string? Fase { get; set; } = "A"; //[EnumDescription("Abordagem")] Abordagem = 'A', [EnumDescription("Transporte")] Transporte = 'T', [EnumDescription("Recepção")] Recepcao = 'R',[EnumDescription("Sala")] Sala = 'S'
        public virtual int? FrBrinde { get; set; }
        public virtual int? FrComboBrinde { get; set; }
        public virtual int? Quantidade { get; set; } = 1;
        public virtual string? Status { get; set; } = "C"; //[EnumDescription("Reservado")] Reservado = 'R', [EnumDescription("Concedido")] Concedido = 'C',  [EnumDescription("Cancelado")]  Cancelado = 'X'
        public virtual int? Pessoa1 { get; set; }
        public virtual int? Pessoa2 { get; set; }
        public virtual int? FrSala { get; set; }
        public virtual int? FrSolicitacaoNegociacao { get; set; }
        public virtual DateTime? DataHoraCancelamento { get; set; }
        public virtual int? UsuarioCancelamento { get; set; }
        public virtual DateTime? DataHoraConcessao { get; set; }
        public virtual int? UsuarioConcessao { get; set; }
        public virtual string? VoucherIntegracao { get; set; }
        public virtual string? Utilizado { get; set; } = "N";
        public virtual DateTime? DataUtilizacao { get; set; }
        public virtual DateTime? DataHoraUtilizacao { get; set; }
        public virtual int? UsuarioUtilizacao { get; set; }
        public virtual int? FrBrindePagamento { get; set; }
        public virtual decimal? ValorPagamento { get; set; }

    }
}
