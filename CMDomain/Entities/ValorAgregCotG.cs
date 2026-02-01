namespace CMDomain.Entities
{
    public class ValorAgregCotG : CMEntityBase
    {
        public virtual int? IdValorAgregCotG { get; set; }
        public virtual int? CodProcesso { get; set; }
        public virtual int? IdForCli { get; set; }
        public virtual int? Proposta { get; set; }
        public virtual decimal? Valor { get; set; }
        public virtual decimal? BaseCalculo { get; set; }
        public virtual decimal? Percentual { get; set; }
        public virtual int? CodTipoCustAgreg { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }

    }
}
