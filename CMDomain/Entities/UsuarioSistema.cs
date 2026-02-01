namespace CMDomain.Entities
{
    public class UsuarioSistema : CMEntityBase
    {
        public virtual int? IdUsuario { get; set; }
        public virtual string? NomeUsuario { get; set; }
        public virtual string? Senha { get; set; }
        public virtual string? SenhaTransf { get; set; }
        public virtual string? MudarSenha { get; set; }
        public virtual string? SenhaPermanente { get; set; }
        public virtual string? NaoMudaSenha { get; set; }
        public virtual int? IdEspAcesso { get; set; }
        public virtual string? FlgAusente { get; set; }
        public virtual DateTime? ValidadeSenha { get; set; }
        public virtual string? Descricao { get; set; }
        public virtual string? Bloqueado { get; set; }
        public virtual string? Desativado { get; set; }
        public virtual int? IdTzLocations { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
        public virtual string? SwPasswordHash { get; set; }
    }
}
