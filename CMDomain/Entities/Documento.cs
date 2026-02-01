namespace CMDomain.Entities
{
    public class Documento : CMEntityBase
    {
        public virtual int? CodDocumento { get; set; }
        public virtual int? IdPessoa { get; set; }
        public virtual int? IdHotel { get; set; }
        public virtual int? Plano { get; set; }
        public virtual string? Placonta { get; set; }
        public virtual int? IdEmpresa { get; set; }
        public virtual string? CodCentroCusto { get; set; }
        public virtual int? IdForCli { get; set; }
        public virtual int? IdModulo { get; set; }
        public virtual int? CodTipDoc { get; set; }
        public virtual string? RecPag { get; set; }
        public virtual string? NoDocumento { get; set; }
        public virtual int? CodSubConta { get; set; }
        public virtual string? ComplDocumento { get; set; }
        public virtual DateTime? DataEmissao { get; set; }
        public virtual DateTime? DataVencto { get; set; }
        public virtual DateTime? DataProgramada { get; set; }
        public virtual string? Status { get; set; } = "0";
        public virtual int? NumFatura { get; set; }
        public virtual string? Operacao { get; set; }
        public virtual string? NumSlip { get; set; }
        public virtual string? EmisBloq { get; set; }
        public virtual string? FlgAprovados { get; set; } = "S";
        public virtual string? FlgContabTipReceb { get; set; } = "N";
        public virtual string? FlgContabRecDes { get; set; } = "N";
        public virtual string? FlgNaoIntegfflex { get; set; } = "S";
        public virtual int? FlgNaoConciliado { get; set; } = 1;
        public virtual string? FlgOc { get; set; } = "N";
        public virtual string? EmitiuComprovante { get; set; } = "N";
        public virtual int? IdUsuarioInclusao { get; set; }
        public virtual int? CodForma { get; set; }
        public virtual int? CodPortForma { get; set; }
        public virtual int? ControleRemessa { get; set; }
        public virtual DateTime? DataRemessa { get; set; }
        public virtual string? ChavePix { get; set; }
        public virtual string? NumLeitCodBarras { get; set; }
        public virtual string? NumDigCodBarras { get; set; }
        public virtual string? Obs { get; set; }
        public virtual string? CodFiscal { get; set; }
        public virtual int? IdModeloNfFlex { get; set; }
        public virtual string? CodSituacaoFFlex { get; set; } = "00";
        public virtual string? ClasConsumoFFlex { get; set; }
        public virtual string? CodTpLigacaoFFlex { get; set; }
        public virtual string? GrupoTensaoFFlex { get; set; }
        public virtual int? IdArquivo { get; set; }
        public virtual int? IdCBancaria { get; set; }
        public virtual int? IdContaXChave { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
        public virtual DateTime? TrgDtAlteracao { get; set; }
        public virtual string? TrgUserAlteracao { get; set; }

    }
}
