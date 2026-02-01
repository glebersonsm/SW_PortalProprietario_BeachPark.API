namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrBrinde : EntityBaseEsol
    {

        public string? Codigo { get; set; }
        public string? Nome { get; set; }
        public string? NomePesquisa { get; set; }
        public string? Status { get; set; } = "A";
        public string? ControlaEstoque { get; set; } = "N";
        public int? ProdutoItem { get; set; }
        public string? Terceiro { get; set; } = "N";
        public int? ClienteTerceiro { get; set; }
        public decimal? Valor { get; set; }
        public decimal? ValorTerceiro { get; set; }
        public string? Descricao { get; set; }
        public string? EspecificacaoUso { get; set; }
        public int? FrTipoDocumentoImpressao { get; set; }
        public int? QuantidadeMaximaAbordagem { get; set; }
        public int? QuantidadeMaximaSala { get; set; }
        public string? ParticipaIntegracao { get; set; } = "N";
        public string? EnviarSmsConfirmacao { get; set; } = "N";
        public string? EnviarSmsLinkVoucher { get; set; } = "N";
        public string? VoucherUnico { get; set; } = "N";
        public int? ProvedorSms { get; set; }
        public int? SmsModelo { get; set; }
        public string? EnviarEmailLinkVoucher { get; set; } = "N";
        public int? EmailModelo { get; set; }
        public int? EmailRemetente { get; set; }
        public int? TipoDocumento { get; set; }
        public string? Vencimento { get; set; }
        public string? UtilizaMensagemWhatsApp { get; set; } = "N";
        public string? MensagemWhatsapp { get; set; }
        public int? Filial { get; set; }

    }
}
