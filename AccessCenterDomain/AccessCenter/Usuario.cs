namespace AccessCenterDomain.AccessCenter
{
    public class Usuario : EntityBase
    {
        public virtual string? Codigo { get; set; }
        public virtual string? Login { get; set; }
        public virtual int? Pessoa { get; set; }
        public virtual int? CentroCusto { get; set; }
        public virtual string? Status { get; set; } = "A";
        public virtual string? UsuarioFramework { get; set; } = "N";
        public virtual string? AlterarSenhaProximoLogon { get; set; }
        public virtual string? Senha { get; set; }

    }
}
