namespace CMDomain.Entities
{
    public class ContratoProd : CMEntityBase
    {
        public virtual int? IdContratoProd { get; set; }
        public virtual int? IdEmpresa { get; set; }
        public virtual int? IdPessoa { get; set; }
        public virtual int? IdForCli { get; set; }
        public virtual string? CodArtigo { get; set; }
        public virtual string? CodMedida { get; set; }
        public virtual decimal? VlrUnitario { get; set; }
        public virtual int? PrazoPag { get; set; }
        public virtual int? PrazoEntrega { get; set; }
        public virtual DateTime? DataInicio { get; set; }
        public virtual DateTime? DataTermino { get; set; }
        public virtual string? FlgListaPreco { get; set; } = "S";
        public virtual string? Status { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }

    }
}
