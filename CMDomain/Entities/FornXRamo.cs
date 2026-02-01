namespace CMDomain.Entities
{
    public class FornXRamo : CMEntityBase
    {
        public virtual int? IdPessoa { get; set; }
        public virtual int? IdRamoFornecedor { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }

        public override int GetHashCode()
        {
            return IdRamoFornecedor.GetHashCode() + IdPessoa.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            FornXRamo? cc = obj as FornXRamo;
            if (cc is null) return false;
            return cc.Equals(this);
        }

    }
}
