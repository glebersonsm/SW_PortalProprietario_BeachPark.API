namespace CMDomain.Entities
{
    public class UsuXGrupProd : CMEntityBase
    {
        public virtual string? CodGrupoProd { get; set; }
        public virtual int? IdPessoa { get; set; }
        public virtual int? IdUsuario { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }

        public override int GetHashCode()
        {
            return CodGrupoProd.GetHashCode() + IdPessoa.GetHashCode() + IdUsuario.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            UsuXGrupProd? usuXGrupProd = obj as UsuXGrupProd;
            if (usuXGrupProd is null) return false;
            return usuXGrupProd.Equals(this);
        }
    }
}
