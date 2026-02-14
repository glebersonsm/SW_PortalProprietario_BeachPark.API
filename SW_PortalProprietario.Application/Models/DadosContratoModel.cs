namespace SW_PortalProprietario.Application.Models
{
    public class DadosContratoModel
    {
        public int? FrAtendimentoVendaId { get; set; }
        public int? IdVendaXContrato { get; set; }
        public string? Status { get; set; }
        public DateTime? DataVenda { get; set; }
        public DateTime? DataCancelamento { get; set; }
        public DateTime? DataReversao { get; set; }
        public DateTime? DataContigencia { get; set; }
        public DateTime? DataValidade { get; set; }
        public string? Contigencia { get; set; }
        public int? CotaOriginal { get; set; }
        public int? Cota { get; set; }
        public string? NumeroContrato { get; set; }
        public string? PessoaTitular1Nome { get; set; }
        public string? PessoaTitular1Id { get; set; }
        public string? PessoaTitular1Tipo { get; set; }
        public string? PessoaTitular1CPF { get; set; }
        public string? PessoaTitualar1CNPJ { get; set; }
        public string? PessoaTitular1Email { get; set; }
        public string? PessoaTitular2Id { get; set; }
        public string? PessoaTitular2Tipo { get; set; }
        public string? PessoaTitular2Nome { get; set; }
        public string? PessoaTitular2CPF { get; set; }
        public string? PessoaTitualar2CNPJ { get; set; }
        public string? PessoaTitular2Email { get; set; }
        public string? Produto { get; set; }
        public int? Empreendimento { get; set; }
        public string? IdIntercambiadora { get; set; }
        public string? NumeroImovel { get; set; }
        public string? CotaStatus { get; set; }
        public string? GrupoCotaTipoCotaNome { get; set; }
        public string? GrupoCotaTipoCotaCodigo { get; set; }
        public string? ProjetoXContrato { get; set; }
        public string? PadraoDeCor { get; set; } = "Default";
        public List<StatusCrcContratoModel> frAtendimentoStatusCrcModels { get; set; } = new List<StatusCrcContratoModel>();

    }
}
