namespace AccessCenterDomain.AccessCenter
{
    public class TipoParcela : EntityBase
    {
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual string Categoria { get; set; }
        public virtual string ProcessaReajuste { get; set; } = "S";
        public virtual int? GrupoEmpresa { get; set; }
    }
}
