namespace CMDomain.Entities
{
    public class UsuarioXTpDocto : CMEntityBase
    {
        public virtual int? CodTipDoc { get; set; }
        public virtual string? RecPag { get; set; }
        public virtual int? IdUsuario { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }

        public override int GetHashCode()
        {
            return IdUsuario.GetHashCode() + CodTipDoc.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            UsuarioXTpDocto? item = obj as UsuarioXTpDocto;
            if (item is null) return false;
            return item.Equals(this);
        }
    }
}
