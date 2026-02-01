namespace CMDomain.Entities
{
    public class UsuXAlmox : CMEntityBase
    {
        public virtual int? CodAlmoxarifado { get; set; }
        public virtual int? IdPessoa { get; set; }
        public virtual int? IdUsuario { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }

        public override int GetHashCode()
        {
            return CodAlmoxarifado.GetHashCode() + IdPessoa.GetHashCode() + IdUsuario.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            UsuXAlmox? usuXAlmoxa = obj as UsuXAlmox;
            if (usuXAlmoxa is null) return false;
            return usuXAlmoxa.Equals(this);
        }
    }
}
