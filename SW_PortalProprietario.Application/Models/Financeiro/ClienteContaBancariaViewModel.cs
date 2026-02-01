namespace SW_PortalProprietario.Application.Models.Financeiro
{
    public class ClienteContaBancariaViewModel
    {
        public int? Id { get; set; }
        public int? ClienteId { get; set; }
        public string? CodigoBanco { get; set; }
        public string? NomeBanco { get; set; }
        public string? Agencia { get; set; }
        public string? AgenciaDigito { get; set; }
        public string? ContaNumero { get; set; }
        public string? ContaDigito { get; set; }
        public string? Variacao { get; set; }
        public string? TipoConta { get; set; }
        public string? Preferencial { get; set; }
        public string? Status { get; set; }
        public string? IdCidade { get; set; }
        public string? NomeCidade { get; set; }
        public string? SiglaEstadoCidade { get; set; }
        public string? TipoChavePix { get; set; }
        public string? DescricaoTipoChavePix { get; set; }
        public string? informaPix { get; set; }
        public string? ChavePix { get; set; }
        public string? NomeNormalizado { get; set; }
        public string? Tipo { get; set; }

    }
}
