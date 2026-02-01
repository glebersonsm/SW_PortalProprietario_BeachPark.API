namespace AccessCenterDomain.AccessCenter
{
    public class GrupoProduto : EntityBase
    {
        public virtual int? GrupoEmpresa { get; set; }
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual string PermiteSubsidio { get; set; }
        public virtual int? NaturezaReceitaPis { get; set; }
        public virtual int? NaturezaReceitaCofins { get; set; }

    }
}
