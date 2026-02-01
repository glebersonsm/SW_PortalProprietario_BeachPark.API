namespace CMDomain.Entities
{
    public class GrupProd : CMEntityBase
    {
        public virtual string? CodGrupoProd { get; set; }
        public virtual string? DescGrupoProd { get; set; }
        public virtual string? StatusGrupo { get; set; }
        public virtual string? FlgIndicaServico { get; set; }
        public virtual string? CodigoNCM { get; set; }
        public virtual int? IdPessoa { get; set; }
        public virtual NaturezaEstoque? NaturezaEstoque { get; set; }
        public virtual string? FlgGrupoFixo { get; set; }
        public virtual string? TipoEstoque { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
    }
}
