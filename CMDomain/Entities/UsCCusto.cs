namespace CMDomain.Entities
{
    public class UsCCusto : CMEntityBase
    {
        public virtual string CodCentroCusto { get; set; } = "";
        public virtual int IdEmpresa { get; set; }
        public virtual int? IdUsuario { get; set; }
        public virtual int? IdPessoa { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }

        public override int GetHashCode()
        {
            return Convert.ToString(CodCentroCusto).GetHashCode() + IdEmpresa.GetHashCode() + IdPessoa.GetHashCode() + IdUsuario.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            UsCCusto? cc = obj as UsCCusto;
            if (cc is null) return false;
            return cc.Equals(this);
        }
    }
}
