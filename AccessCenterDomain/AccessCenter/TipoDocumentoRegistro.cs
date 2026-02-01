namespace AccessCenterDomain.AccessCenter
{
    public class TipoDocumentoRegistro : EntityBase
    {
        public virtual string PessoaTipo { get; set; }
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual string Mascara { get; set; }
    }
}
