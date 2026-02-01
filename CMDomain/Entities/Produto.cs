namespace CMDomain.Entities
{
    public class Produto : CMEntityBase
    {
        public virtual string? CodProduto { get; set; }
        public virtual GrupProd? GrupProd { get; set; }
        public virtual string? CodMedCusto { get; set; }
        public virtual string? DescProd { get; set; }
        public virtual string? CodMedAnalise { get; set; }
        public virtual string? ConsumoRevenda { get; set; } = "R";
        public virtual string? LoteValidade { get; set; } = "F";
        public virtual string? ItemEstocavel { get; set; } = "S";
        public virtual string? DescrCompl { get; set; }
        public virtual string? CodMenorMed { get; set; }
        public virtual string? FlgVariavel { get; set; } = "N";
        public virtual string? CodigoNCM { get; set; }
        public virtual string? CodGenero { get; set; }
        public virtual string? Cest { get; set; }
        public virtual int? IdGtin { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
    }
}
