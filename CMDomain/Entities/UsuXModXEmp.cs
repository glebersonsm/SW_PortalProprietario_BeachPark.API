namespace CMDomain.Entities
{
    public class UsuXModXEmp : CMEntityBase
    {
        public virtual int? IdEspAcesso { get; set; }
        public virtual int? IdEmpresa { get; set; }
        public virtual int? IdModulo { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }


        public override int GetHashCode()
        {
            return IdEspAcesso.GetHashCode() + IdEmpresa.GetHashCode() + IdModulo.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            UsuXModXEmp? cc = obj as UsuXModXEmp;
            if (cc is null) return false;
            return cc.Equals(this);
        }

    }
}
