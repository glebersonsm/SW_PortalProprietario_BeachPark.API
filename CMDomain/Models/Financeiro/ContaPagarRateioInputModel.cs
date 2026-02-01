namespace CMDomain.Models.Financeiro
{
    public class ContaPagarRateioInputModel
    {
        /// <summary>
        /// Código do centro de custo informado no rateio
        /// </summary>
        public string? CentroCusto { get; set; }
        /// <summary>
        /// Valor do rateio
        /// </summary>
        public decimal? Valor { get; set; }
        /// <summary>
        /// TipoDesembolso código do tipo de desembolso, similar ao destino contábil do eSolution
        /// </summary>
        public string? TipoDesembolso { get; set; }
        public string? CodCentroResponsabilidade { get; set; }
        /// <summary>
        /// UnidadeNegócio chave primária da unidade de negócio, similar Atividade de projeto do eSolution
        /// </summary>
        public int? UnidadeNegocio { get; set; }

    }
}
