

using CMDomain.Entities;

namespace CMDomain.Models.Financeiro
{
    public class ContaPagarRateioViewModel
    {
        public int? IdDocumento { get; set; }
        public int? IdRateio { get; set; }
        public string? CodCentroCusto { get; set; }
        public string? NomeCentroCusto { get; set; }
        public string? CodCentroResponsabilidade { get; set; }
        public string? CodTipoDesembolso { get; set; }
        public string? NomeTipoDesembolso { get; set; }
        public int? PlanoContaContabil { get; set; }
        public string? ContaContabil { get; set; }
        public int? UnidadeNegocioId { get; set; }
        public string? NomeUnidadeNegocio { get; set; }
        public decimal? Valor { get; set; }
        public int? NumFatura { get; set; }
        public DateTime? TrgDtInclusao { get; set; }
        public string? TrgUserInclusao { get; set; }

        public static explicit operator ContaPagarRateioViewModel(RateioDocum model)
        {
            return new ContaPagarRateioViewModel
            {
                IdDocumento = model.CodDocumento,
                Valor = model.Valor,
                CodCentroCusto = model.CodCentroCusto,
                CodCentroResponsabilidade = model.CodCentroRespon,
                CodTipoDesembolso = model.CodTipRecDes,
                UnidadeNegocioId = model.UnidNegoc,
                TrgDtInclusao = model.TrgDtInclusao,
                TrgUserInclusao = model.TrgUserInclusao
            };
        }
    }
}
