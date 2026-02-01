namespace CMDomain.Entities
{
    public class ProdutoXEmpresa : CMEntityBase
    {
        public virtual int? IdProdutoXEmpresa { get; set; }
        public virtual string? SituacaoTrib { get; set; }
        public virtual int? IdPessoa { get; set; }
        public virtual string? CodProduto { get; set; }
        public virtual string? IsentoOutros { get; set; } = "O";
        public virtual string? CodFiscalPadrao { get; set; } = "102";
        public virtual string? ConsumoRevenda { get; set; } = "R";
        public virtual string? SituacaoTribA { get; set; } = "0";
        public virtual string? CodStCofins { get; set; } = "50";
        public virtual string? CodStPis { get; set; } = "50";
        public virtual string? SituacaoTribSaida { get; set; } = "41";
        public virtual string? CodStPisSaida { get; set; } = "01";
        public virtual string? CodStCofinsSaida { get; set; } = "01";
        public virtual string? CodStIpi { get; set; } = "03";
        public virtual string? CodStIpiSaida { get; set; } = "99";
        public virtual string? FlgRegraCalc { get; set; } = "P";
        public virtual string? RegimeApuracao { get; set; } = "N";
        public virtual string? FlgIntTHex { get; set; } = "N";
        public virtual string? TipoEstoque { get; set; }
        public virtual string? FlgIndicadorProp { get; set; } = "0"; //0 = Propriedade do informante ou em seu poder, 1 = Conselheiro de Administração do Informante e em posso de Terceiros, 2 = Propriedade de terceiros em posse do informante
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }

    }
}
