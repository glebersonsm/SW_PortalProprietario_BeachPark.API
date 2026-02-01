namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrUsuarioFrFuncao : EntityBase
    {
        public virtual int? FrUsuario { get; set; }
        public virtual int? FrFuncao { get; set; }
        public virtual int? FrEquipe { get; set; }
        public virtual string Status { get; set; }

    }
}
