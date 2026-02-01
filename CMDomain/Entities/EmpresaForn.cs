namespace CMDomain.Entities
{
    public class EmpresaForn : CMEntityBase
    {
        public virtual int? IdPessoa { get; set; }
        public virtual int? IdForCli { get; set; }
        public virtual int? IdEmpresa { get; set; }
        public virtual int? Plano { get; set; }
        public virtual int? CodSubConta { get; set; }
        public virtual string? ContaCadiantamento { get; set; }
        public virtual string? ContaCDespesa { get; set; }
        public virtual string? ContaCForn { get; set; }
        public virtual string? CodCorresp { get; set; }
        public virtual string? FlgStatus { get; set; } = "A";
        public virtual string? FlgNaoContabLanc { get; set; } = "N";
        public virtual string? FlgContBaixDesemb { get; set; } = "N";
        public virtual string? IndOpCcp { get; set; } = null;
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }

        public override int GetHashCode()
        {
            return IdForCli.GetHashCode() + IdPessoa.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            EmpresaForn? cc = obj as EmpresaForn;
            if (cc is null) return false;
            return cc.Equals(this);
        }
    }
}
