namespace AccessCenterDomain.AccessCenter
{
    public class TipoCliente : EntityBase
    {
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual string DadosCliente { get; set; }
        public virtual string DadosFornecedor { get; set; }
        public virtual string DadosCobrador { get; set; }
        public virtual int? GrupoEmpresa { get; set; }
        public virtual int? Empresa { get; set; }


    }
}
