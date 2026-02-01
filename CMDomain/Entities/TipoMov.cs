namespace CMDomain.Entities
{
    public class TipoMov : CMEntityBase
    {
        public virtual string? CodTipoMov { get; set; }
        public virtual string? ConsumoMov { get; set; } = "F";
        public virtual string? DescTipoMov { get; set; }
        public virtual string? DescResumida { get; set; }
        public virtual string? EntradaSaida { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
    }
}
