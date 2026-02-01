namespace AccessCenterDomain.AccessCenter
{
    public class TipoProduto : EntityBase
    {
        public virtual int? GrupoEmpresa { get; set; }
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual string TipoProdutoTipo { get; set; } = "P";
        public virtual string ExigeOrdemCompra { get; set; } = "N";
        public virtual int? CentroCusto { get; set; }
        public virtual string PermiteReterInss { get; set; }
        public virtual string EnviaEstoqueSped { get; set; } = "N";

    }
}
