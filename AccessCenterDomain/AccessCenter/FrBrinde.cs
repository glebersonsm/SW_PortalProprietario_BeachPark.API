namespace AccessCenterDomain.AccessCenter
{
    public class FrBrinde : EntityBase
    {
        public virtual int? Filial { get; set; }
        public virtual string? Codigo { get; set; }
        public virtual string? Nome { get; set; }
        public virtual string? NomePesquisa { get; set; }
        public virtual string? Status { get; set; } = "A";
        public virtual string? ControlaEstoque { get; set; } = "N";
        public virtual int? ProdutoItem { get; set; }
        public virtual string? Terceiro { get; set; } = "N";
        public virtual int? ClienteTerceiro { get; set; }
        public virtual decimal? Valor { get; set; }
        public virtual decimal? ValorTerceiro { get; set; }
        public virtual string? Descricao { get; set; }
        public virtual string? EspecificacaoUso { get; set; }
        public virtual int? FrTipoDocumentoImpressao { get; set; }
        public virtual int? QuantidadeMaximaAbordagem { get; set; }
        public virtual int? QuantidadeMaximaSala { get; set; }
        public virtual string? ParticipaIntegracao { get; set; } = "N";
        public virtual string? EnviarSmsConfirmacao { get; set; } = "N";
        public virtual string? EnviarSmsLinkVoucher { get; set; } = "N";
        public virtual string? VoucherUnico { get; set; } = "N";
        public virtual int? ProvedorSms { get; set; }
        public virtual int? SmsModelo { get; set; }
        public virtual string? EnviarEmailLinkVoucher { get; set; } = "N";
        public virtual int? EmailModelo { get; set; }
        public virtual int? EmailRemetente { get; set; }
        public virtual int? TipoDocumento { get; set; }
        public virtual string? Vencimento { get; set; }
        public virtual string? UtilizaMensagemWhatsApp { get; set; } = "N";
        public virtual string? MensagemWhatsapp { get; set; }

    }
}
