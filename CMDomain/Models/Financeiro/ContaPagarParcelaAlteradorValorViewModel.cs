


using CMDomain.Entities;

namespace CMDomain.Models.Financeiro
{
    public class ContaPagarParcelaAlteradorValorViewModel
    {
        /// <summary>
        /// IdDocumento chave primária composta com o campo NumeroLancamento
        /// </summary>
        public int? IdDocumento { get; set; }
        /// <summary>
        /// NumeroLancamento chave primária composta com o campo IdDocumento
        /// </summary>
        public int? NumeroLancamento { get; set; }
        /// <summary>
        /// IdAlteradorValor chave primária do tipo de alterador
        /// </summary>
        public int? IdAlteradorValor { get; set; }
        /// <summary>
        /// NomeAlteradorValro nome do tipo de alterador de valor
        /// </summary>
        public string? NomeAlteradorValor { get; set; }
        public decimal? ValorAlteracao { get; set; }
        /// <summary>
        /// Estornado se true o item foi estornado se false á um item que está compondo o valor do documento
        /// </summary>
        public bool? Estornado { get; set; }
        public int? Usuario { get; set; }
        public string? NomeUsuario { get; set; }
        public string? HistoricoComplementar { get; set; }
        public DateTime? DataAlteracao { get; set; }
        public DateTime? TrgDtInclusao { get; set; }
        public string? TrgUserInclusao { get; set; }


        public static explicit operator ContaPagarParcelaAlteradorValorViewModel(LanctoDocum model)
        {
            return new ContaPagarParcelaAlteradorValorViewModel
            {
                IdDocumento = model.CodDocumento,
                NumeroLancamento = model.NumLancto,
                IdAlteradorValor = model.CodAlterador,
                ValorAlteracao = model.Valor * (model.DebCre == "D" ? -1 : 1),
                Estornado = model.Estorno.GetValueOrDefault(0) > 0,
                Usuario = model.IdUsuarioInclusao,
                DataAlteracao = model.DataLancto,
                HistoricoComplementar = model.HistoricoCompl,
                TrgDtInclusao = model.TrgDtInclusao,
                TrgUserInclusao = model.TrgUserInclusao

            };
        }
    }
}
