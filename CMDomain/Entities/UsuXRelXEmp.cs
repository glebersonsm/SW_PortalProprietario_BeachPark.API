namespace CMDomain.Entities
{
    public class UsuXRelXEmp : CMEntityBase
    {
        public virtual int? IdEmpresa { get; set; }
        public virtual int? IdReports { get; set; }
        public virtual int? OrigemCM { get; set; }
        public virtual int? IdEspAcesso { get; set; }
        public virtual string? FlgHabilita { get; set; }
        public virtual string? FlgVisualiza { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }

        public override int GetHashCode()
        {
            return IdEmpresa.GetHashCode() + IdReports.GetHashCode() + OrigemCM.GetHashCode() + IdEspAcesso.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            UsuXRelXEmp? usuxrelxemp = obj as UsuXRelXEmp;
            if (usuxrelxemp is null) return false;
            return usuxrelxemp.Equals(this);
        }

    }
}
