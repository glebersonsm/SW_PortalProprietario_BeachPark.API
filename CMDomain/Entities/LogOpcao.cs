namespace CMDomain.Entities
{
    public class LogOpcao : CMEntityBase
    {
        public virtual int IdLogOpcao { get; set; }
        public virtual int IdUsuario { get; set; }
        public virtual int IdPessoa { get; set; }
        public virtual int IdModulo { get; set; }
        public virtual string? NomeOpcao { get; set; }
        public virtual DateTime? DataLog { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }

    }
}
