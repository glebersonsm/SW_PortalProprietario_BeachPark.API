namespace CMDomain.Entities
{
    public class SoliComp : CMEntityBase
    {
        public virtual int? NumSolCompra { get; set; }
        public virtual int? IdPessoa { get; set; }
        public virtual int? IdUsuario { get; set; }
        public virtual int? NumRequisicao { get; set; }
        public virtual int? IdProcesso { get; set; }
        public virtual int? IdReservaOrcamen { get; set; }
        public virtual int? CodAlmoxarifado { get; set; }
        public virtual int? UnidNegoc { get; set; } = -1;
        public virtual string? CodCentroRespon { get; set; }
        public virtual DateTime? DataEntrega { get; set; }
        public virtual int? IdEmpresa { get; set; }
        public virtual string? CodCentroCusto { get; set; }
        public virtual string? AlgumParaEstoque { get; set; }
        public virtual DateTime? DataEmissao { get; set; }
        public virtual string? SoliciAtendida { get; set; } = "F";
        public virtual string? SoliciAceita { get; set; } = "F";
        public virtual string? CustoEstoque { get; set; } = "E";
        public virtual string? Impresso { get; set; } = "F";
        public virtual string? FlgPrePronta { get; set; } = "N";
        public virtual int? IdContPermuta { get; set; }
        public virtual string? Status { get; set; } = "PE";
        public virtual int? IdArquivo { get; set; }
        public virtual int? IdProcessoSecundario { get; set; }
        public virtual int? IdProcessoMaster { get; set; }
        public virtual string? FlgUrgente { get; set; } = "N";
        public virtual string? FlgWs { get; set; } = "N";
        public virtual string? FlgStatusWs { get; set; } = "N";
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
    }
}
