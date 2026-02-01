namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class DadosContratoAccessCenterModel
    {
        public int CotaId { get; set; }
        public int? UhCondominio { get; set; }
        public string? CotaNome { get; set; }
        public string? CotaCodigo { get; set; }
        public string? Produto { get; set; }
        public string? NumeroContrato { get; set; }
        public string? ImovelNumero { get; set; }
        public string? ImovelBloco { get; set; }
        public string? ImovelAndar { get; set; }
        public string? EmpreendimentoId { get; set; }
        public string? Empreendimento { get; set; }
        public string? TipoRetorno { get; set; } //AC ou PORTAL
        public string? Titular1Nome { get; set; }
        public string? Titular2Nome { get; set; }
        public string? IdIntercambiadora { get; set; }
        public string? PadraoDeCor { get; set; } = "Default";
    }
}
