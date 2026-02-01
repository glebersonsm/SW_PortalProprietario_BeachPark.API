namespace CMDomain.Entities
{
    public class Moviment : CMEntityBase
    {
        public virtual int? IdMov { get; set; }
        public virtual string? CodArtigo { get; set; }
        public virtual TipoMov? TipoMov { get; set; }
        public virtual int? IdHotel { get; set; }
        public virtual int? IdPessoaTransf { get; set; }
        public virtual int? IdPessoa { get; set; }
        public virtual int? IdEmpresa { get; set; }
        public virtual int? Plano { get; set; }
        public virtual string? Placonta { get; set; }
        public virtual int? UnidNegoc { get; set; }
        public virtual int? PlnCodigo { get; set; }
        public virtual string? CodCentroCusto { get; set; }
        public virtual int? CodAlmoxarifado { get; set; }
        public virtual DateTime? DataMov { get; set; }
        public virtual decimal? QtdeMov { get; set; }
        public virtual decimal? ValorMov { get; set; }
        public virtual DateTime? DataLancMov { get; set; }
        public virtual decimal? CustoMedioMov { get; set; }
        public virtual decimal? SaldoQtdeMov { get; set; }
        public virtual string? NumDocumento { get; set; }
        public virtual int? CodAlmoxTransf { get; set; }
        public virtual int? IdMovEntrada { get; set; }
        public virtual string? FlgEstorno { get; set; } = "N";
        public virtual int? IdTipoPerda { get; set; }
        public virtual string? FlgEntradaCusto { get; set; } = "N";
        public virtual int? IdLoteArtigo { get; set; }
        public virtual string? NumLote { get; set; }
        public virtual string? FlgIntegrada { get; set; }
        public virtual long? NumRequisicao { get; set; }
        public virtual string? PlacontaEntrada { get; set; }
        public virtual int? SubContaEntrada { get; set; }
        public virtual int? PlanoEntrada { get; set; }
        public virtual string? CodFiscal { get; set; }
        public virtual int? IdTpFinalidade { get; set; }
        public virtual int? IdForCli { get; set; }
        public virtual decimal? QtdeMovEspelho { get; set; } = 0.00m;
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
    }
}
