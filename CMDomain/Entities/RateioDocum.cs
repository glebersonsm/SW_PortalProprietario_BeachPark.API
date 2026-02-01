using CMDomain.Models.Financeiro;

namespace CMDomain.Entities
{
    public class RateioDocum : CMEntityBase
    {
        public virtual int? IdRateioDocum { get; set; }
        public virtual int? CodDocumento { get; set; }
        public virtual string? CodFiscal { get; set; }
        public virtual string? CodTipRecDes { get; set; }
        public virtual int? IdEmpresa { get; set; }
        public virtual string? CodCentroCusto { get; set; }
        public virtual string? RecPag { get; set; }
        public virtual int? IdPessoa { get; set; }
        public virtual string? CodCentroRespon { get; set; }
        public virtual int? UnidNegoc { get; set; } = -1;
        public virtual decimal? Valor { get; set; }
        public virtual decimal? ValorOutraMoeda { get; set; }
        public virtual int? IdUsuarioInclusao { get; set; }
        public virtual int? IdItemOc { get; set; }
        public virtual int? Plano { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }

        #region properties
        public virtual TipoDesembolsoViewModel TipoDesembolsoViewModel { get; set; }
        #endregion
    }
}
