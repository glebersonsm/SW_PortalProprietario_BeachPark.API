namespace CMDomain.Entities
{
    public class LogAcessoSis : CMEntityBase
    {
        public virtual int? IdUsuario { get; set; }
        public virtual int? IdModulo { get; set; }
        public virtual int? IdLogAcessoSis { get; set; }
        public virtual string? FlgOperacao { get; set; } = "O";
        public virtual string? Versao { get; set; } = "12.01.2204.04";

    }
}
