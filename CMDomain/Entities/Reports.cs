namespace CMDomain.Entities
{
    public class Reports : CMEntityBase
    {
        public virtual int? IdReports { get; set; }
        public virtual int? OrigemCM { get; set; }
        public virtual int? IdModulo { get; set; }
        public virtual int? IdGrupoRelatorio { get; set; }
        public virtual string? Name { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }

        public override int GetHashCode()
        {
            return IdReports.GetHashCode() + OrigemCM.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            Reports? reports = obj as Reports;
            if (reports is null) return false;
            return reports.Equals(this);
        }

    }
}
