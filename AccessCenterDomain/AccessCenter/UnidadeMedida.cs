namespace AccessCenterDomain.AccessCenter
{
    public class UnidadeMedida : EntityBase
    {
        public virtual int? GrupoEmpresa { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual string PermiteFracionamento { get; set; }
        public virtual string UnidadeBase { get; set; }
        public virtual int? QuantidadeCasasDecimais { get; set; }
        public virtual UnidadeMedidaSigla? SiglaPadrao { get; set; }

    }
}
