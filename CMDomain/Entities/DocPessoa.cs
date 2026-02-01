namespace CMDomain.Entities
{
    public class DocPessoa : CMEntityBase
    {
        public virtual int? IdDocumento { get; set; }
        public virtual int? IdPessoa { get; set; }
        public virtual string? NumDocumento { get; set; }
        public virtual string? Orgao { get; set; }
        public virtual int? IdEstado { get; set; }
        public virtual DateTime? DataEmissao { get; set; }
        public virtual DateTime? DataValidade { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
        public virtual DateTime? TrgDtAlteracao { get; set; }
        public virtual string? TrgUserAlteracao { get; set; }

        public override int GetHashCode()
        {
            return IdDocumento.GetHashCode() + IdPessoa.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            DocPessoa? cc = obj as DocPessoa;
            if (cc is null) return false;
            return cc.Equals(this);
        }

    }
}
