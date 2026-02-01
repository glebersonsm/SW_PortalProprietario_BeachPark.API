namespace CMDomain.Entities
{
    public class Artigo : CMEntityBase
    {
        public virtual string? CodArtigo { get; set; }
        public virtual string? CodProduto { get; set; }
        public virtual string? CodTipoArtigo { get; set; }
        public virtual string? FlgBloqueado { get; set; } = "A";
        public virtual decimal? ValUltCompra { get; set; }
        public virtual string? FlgAtivo { get; set; } = "S";
        public virtual string? CodBarra { get; set; }
        public virtual string? DescArtigo { get; set; }
        public virtual string? CodEan { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
    }
}
