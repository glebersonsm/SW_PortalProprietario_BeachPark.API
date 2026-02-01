

using CMDomain.Entities;
using CMDomain.Models.Compras;

namespace CMDomain.Models.Financeiro
{
    public class ContaPagarDocumentoViewModel
    {
        /// <summary>
        /// Id chave primária do contas a pagar em exibição
        /// </summary>
        public int? IdDocumento { get; set; }
        /// <summary>
        /// Id da empresa onde foi realizado o lançamento
        /// </summary>
        public int? IdEmpresa { get; set; }
        /// <summary>
        /// Id chave primária do fornecedor beneficiado com o lançamento
        /// </summary>
        public int? IdFornecedor { get; set; }
        public decimal? ValorOriginal => Parcelas.Sum(c => c.ValorOriginal);
        public decimal? ValorAtual => Parcelas.Sum(c => c.ValorAtual);
        public string? NomeFornecedor { get; set; }
        public string? RazaoSocialFornecedor { get; set; }
        public string? CpfCnpjFornecedor { get; set; }
        public string? TipoPessoaFornecedor { get; set; }
        /// <summary>
        /// Id chave primária do tipo de documento do lançamento, similar a natureza de operação do eSolution 
        /// </summary>
        public int? CodTipoDocumento { get; set; }
        /// <summary>
        /// Nome do tipo documento do lançamento, similar a natureza de operação do eSolution
        /// </summary>
        public string? TipoDocumento { get; set; }
        /// <summary>
        /// Número da nota ou documento constante no item que está sendo lançado
        /// </summary>
        public string? NumeroDocumento { get; set; }
        public string? ComplDocumento { get; set; }
        public string? TipoLancamentoDocumento => !string.IsNullOrEmpty(Operacao) && Operacao.TrimEnd() == "1" && NumFatura.GetValueOrDefault(0) > 0 ? "Documento original Englobado/Parcelado" : "Documento original ou Parcela";
        public int? Usuario { get; set; }
        /// <summary>
        /// Controle interno do CM para identificação do tipo de lançamento (1 - Documento que será parcelado, 3 - Parcela do documento, 2 - Documento não parcelado etc..)
        /// </summary>
        public string? Operacao { get; set; }
        public string? NomeUsuario { get; set; }
        public DateTime? DataEmissao { get; set; }
        public DateTime? Vencimento { get; set; }
        /// <summary>
        /// Campo de controle que vincula o documento as suas respectivas parcelas
        /// </summary>
        public int? NumFatura { get; set; }
        public string? ObsLancamento { get; set; }
        public string? HistoricoLanc { get; set; }
        public string? LinhaDigitavelBoleto { get; set; }
        public string? CodigoBarrasBoleto { get; set; }
        public string? ChavePix { get; set; }
        public DateTime? DataLancto { get; set; }
        public string? Cfop { get; set; }
        public int? IdModeloDocumento { get; set; }
        public string? CodSituacao { get; set; } = "00";
        public string? CodClasseConsumo { get; set; }
        public string? CodTipoLigacao { get; set; }
        public string? CodGrupoTensao { get; set; }

        //public List<ContaBancariaViewModel> DadosBancarios { get; set; } = new List<ContaBancariaViewModel>();
        public List<OrdemCompraVinculadaDocumentoViewModel> Ocs { get; set; } = new List<OrdemCompraVinculadaDocumentoViewModel>();
        public DateTime? DataProgramada { get; set; }
        public DateTime? TrgDtInclusao { get; set; }
        public string? TrgUserInclusao { get; set; }
        public int? IdRecebimentoMercadoria { get; set; }
        public string? IdRequestia { get; set; }
        public int? IntegradoCmParaRequestia { get; set; }
        public string? TipoPagamento { get; set; }
        public string? TipoPagamentoDetalhado { get; set; }
        public int? IdCBancaria { get; set; }
        public int? IdContaXChave { get; set; }

        public List<ContaPagarAlteradorValorViewModel>? AlteradoresValoresDocumento { get; set; }
        public List<ContaPagarParcelaViewModel> Parcelas { get; set; } = new List<ContaPagarParcelaViewModel>();
        public List<ContaPagarRateioViewModel> Rateios { get; set; } = new List<ContaPagarRateioViewModel>();
        public List<AnexoItemModel> Anexos { get; set; } = new List<AnexoItemModel>();


        public static explicit operator ContaPagarDocumentoViewModel(Documento model)
        {
            return new ContaPagarDocumentoViewModel
            {
                IdDocumento = model.CodDocumento,
                IdEmpresa = model.IdEmpresa,
                IdFornecedor = model.IdForCli,
                CodTipoDocumento = model.CodTipDoc,
                NumeroDocumento = model.NoDocumento,
                ComplDocumento = model.ComplDocumento,
                DataEmissao = model.DataEmissao,
                DataProgramada = model.DataProgramada,
                Usuario = model.IdUsuarioInclusao,
                Vencimento = model.DataVencto,
                LinhaDigitavelBoleto = model.NumDigCodBarras,
                CodigoBarrasBoleto = model.NumLeitCodBarras,
                ChavePix = model.ChavePix,
                TrgDtInclusao = model.TrgDtInclusao,
                TrgUserInclusao = model.TrgUserInclusao

            };
        }
    }
}
