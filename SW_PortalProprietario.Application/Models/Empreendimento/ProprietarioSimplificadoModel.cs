namespace SW_PortalProprietario.Application.Models.Empreendimento
{
    public class ProprietarioSimplificadoModel
    {
        public int? PessoaProviderId { get; set; }
        public DateTime? DataAquisicao { get; set; }
        public int? EmpreendimentoId { get; set; }
        public string? EmpreendimentoNome { get; set; }
        public string? EmpreendimentoCnpj { get; set; }
        public string? ImovelNumero { get; set; }
        public string? BlocoCodigo { get; set; }
        public string? BlocoNome { get; set; }
        public string? ImovelAndarCodigo { get; set; }
        public string? ImovelAndarNome { get; set; }
        public string? TipoImovelCodigo { get; set; }
        public string? TipoImovelNome { get; set; }

        public int? CotaId { get; set; }
        public string? GrupoCotaCodigo { get; set; }
        public string? GrupoCotaNome { get; set; }
        public string? CodigoFracao { get; set; }
        public string? NomeFracao { get; set; }
        public int? ClienteId { get; set; }
        public string? ClienteCodigo { get; set; }
        public string? CpfCnpjCliente { get; set; }
        public string? NomeCliente { get; set; }
        public string? NumeroContrato { get; set; }
        public string? Email { get; set; }
        public string? TipoCotaNome { get; set; }
        public int? QuantidadeSemana { get; set; }
        public bool? PossuiContratoSCP { get; set; }
        public string? IdIntercambiadora { get; set; }
        public string? PadraoDeCor { get; set; } = "Default";

    }
}
