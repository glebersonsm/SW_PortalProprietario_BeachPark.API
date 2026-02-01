namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrAtendimentoFuncao : EntityBase
    {
        public virtual int? FrAtendimento { get; set; }
        public virtual int? FrFuncao { get; set; }
        public virtual string InformarUsuario { get; set; }
        public virtual string UtilizouPerfilAnalise { get; set; }
        public virtual int? FrUsuario { get; set; }

    }
}
