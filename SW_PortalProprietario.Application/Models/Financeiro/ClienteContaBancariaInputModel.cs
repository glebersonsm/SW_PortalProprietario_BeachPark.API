namespace SW_PortalProprietario.Application.Models.Financeiro
{
    public class ClienteContaBancariaInputModel
    {
        public int? Id { get; set; }
        public int? ClienteId { get; set; }
        public int? IdBanco { get; set; }
        public string? CodigoBanco { get; set; }
        public string? Agencia { get; set; }
        public string? AgenciaDigito { get; set; }
        public string? ContaNumero { get; set; }
        public string? Variacao { get; set; }
        public string? ContaDigito { get; set; }
        public bool? Preferencial { get; set; } = false;
        public int? IdCidade { get; set; }
        public string? TipoChavePix { get; set; }
        public string? ChavePix { get; set; }
        public string? Status { get; set; }
        public List<int>? ClientesIds { get; set; }
        public List<int>? EmpresasIds { get; set; }
        public int? EmpreendimentoId { get; set; }
        public int? EmpresaId { get; set; }

    }
}
