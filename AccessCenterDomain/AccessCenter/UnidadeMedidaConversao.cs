namespace AccessCenterDomain.AccessCenter
{
    public class UnidadeMedidaConversao : EntityBase
    {
        public virtual decimal? Conversao { get; set; }
        public virtual UnidadeMedida UnidadeMedida { get; set; }
        public virtual UnidadeMedida UnidadeMedidaBase { get; set; }

    }
}
