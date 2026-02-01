namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrComissaoFrUsuarioLancamento : EntityBase
    {
        public virtual int? FrComissaoFrUsuario { get; set; }
        public virtual int? FrFuncao { get; set; }
        public virtual int? FrAtendimentoComissao { get; set; }
        public virtual string Observacao { get; set; }
        public virtual decimal Valor { get; set; }
        public virtual string Parcela { get; set; }

    }
}
