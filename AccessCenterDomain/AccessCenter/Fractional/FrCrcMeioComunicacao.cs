namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrCrcMeioComunicacao : EntityBase
    {
        public virtual int? GrupoEmpresa { get; set; } = 1;
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }

    }
}
