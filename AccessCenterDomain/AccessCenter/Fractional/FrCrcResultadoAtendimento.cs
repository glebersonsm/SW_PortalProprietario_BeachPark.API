namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrCrcResultadoAtendimento : EntityBase
    {
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual int? FrCrcTipoAtendimento { get; set; }

    }
}
