namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrUsuario : EntityBase
    {
        public virtual int? GrupoEmpresa { get; set; }
        public virtual int? Empresa { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual string StatusLancamento { get; set; } = "A";
        public virtual string StatusConsulta { get; set; } = "A";
        public virtual string PossuiAjudaCusto { get; set; } = "N";
        public virtual int? UsuarioSistema { get; set; }

    }
}
