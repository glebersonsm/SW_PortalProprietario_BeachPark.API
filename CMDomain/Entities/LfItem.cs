namespace CMDomain.Entities
{
    public class LfItem : CMEntityBase
    {
        public virtual string? CodItem { get; set; }
        public virtual string? CodTipoServico { get; set; }
        public virtual string? CodArtigoRef { get; set; }
        public virtual string? CodUnidadeEst { get; set; }
        public virtual string? CodTipi { get; set; }
        public virtual string? CodsTipi { get; set; }
        public virtual string? CodigoNcm { get; set; }
        public virtual string? CodGenero { get; set; }
        public virtual string? CodSta { get; set; }
        public virtual string? CodStb { get; set; }
        public virtual string? CodStPis { get; set; }
        public virtual string? CodStCofins { get; set; }
        public virtual string? CodGrupoItem { get; set; }
        public virtual string? Descricao { get; set; }
        public virtual decimal? Quantidade { get; set; }
        public virtual decimal? VlrUnitario { get; set; }
        public virtual string? Observacao { get; set; }
        public virtual string? FlgTipo { get; set; }
        public virtual string? FlgAtivo { get; set; }
        public virtual string? CodBarra { get; set; }
        public virtual decimal? AliquotaIcms { get; set; }
        public virtual string? CodStSintegra { get; set; }
        public virtual string? FlgIndicadorProp { get; set; }
        public virtual string? FlgRegApuPisCof { get; set; }
        public virtual string? FlgRegra { get; set; }
        public virtual string? CodStIpiSaida { get; set; }
        public virtual string? CodItemRed { get; set; }
        public virtual string? CodMenorUnidade { get; set; }
        public virtual string? FlgProdComb { get; set; }
        public virtual string? CProdAnp { get; set; }
        public virtual string? Cest { get; set; }
        public virtual string? FlgSincronizaDfe { get; set; }
        public virtual int? IdCNae { get; set; }
        public virtual string? FlgSincParcialDfe { get; set; }
        public virtual string? CodEan { get; set; }
        public virtual string? CodeXTipi { get; set; }
        public virtual int? IdGtin { get; set; }
        public virtual string? IdThex { get; set; }
        public virtual string? CodExTipoServ { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }

    }
}
