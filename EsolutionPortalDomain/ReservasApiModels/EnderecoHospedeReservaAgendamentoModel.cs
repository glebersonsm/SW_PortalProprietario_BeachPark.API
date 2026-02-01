namespace EsolutionPortalDomain.ReservasApiModels
{
    public class EnderecoHospedeReservaAgendamentoModel
    {
        public int? Id { get; set; }
        public string? Logradouro { get; set; }
        public string? Numero { get; set; }
        public string? Bairro { get; set; }
        public CidadeEnderecoHospedeReservaAgendamentoModel? Cidade { get; set; }
        public string? UF { get; set; }
        public string? Complemento { get; set; }
    }
}
