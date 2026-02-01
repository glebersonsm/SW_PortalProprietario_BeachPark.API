namespace CMDomain.Entities
{
    public class TipoAlterador : CMEntityBase
    {
        public virtual int? CodAlterador { get; set; }
        public virtual int? IdPessoa { get; set; }
        public virtual int? IdEmpresa { get; set; }
        public virtual int? Plano { get; set; }
        public virtual string? PlaConta { get; set; }
        public virtual string? Descricao { get; set; }
        public virtual string? AcresDecres { get; set; }
        public virtual string? FlgStatus { get; set; }
        public virtual string? RecPag { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }

    }
}
