namespace CMDomain.Entities
{
    public class GrupoAcesso : CMEntityBase
    {
        public virtual int? IdGrupo { get; set; }
        public virtual string? NomeGrupo { get; set; }
        public virtual int? IdEspAcesso { get; set; }
        public virtual string? Descricao { get; set; }
        public virtual int? DiasAltSenha { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }

    }
}
