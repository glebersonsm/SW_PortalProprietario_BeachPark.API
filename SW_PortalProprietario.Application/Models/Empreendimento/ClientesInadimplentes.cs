using EsolutionPortalDomain.Portal;

namespace SW_PortalProprietario.Application.Models.Empreendimento
{
    public class ClientesInadimplentes
    {
        public int? PessoaProviderId { get; set; }
        public int? IdVendaXContrato { get; set; }
        public int? FrAtendimentoVendaId { get; set; }
        public string? Nome { get; set; }
        public Int64? CpfCnpj { get; set; }
        public string? Email { get; set; }
        public bool? InadimplenteParcelaContrato => TotalInadimplenciaContrato.GetValueOrDefault(0) > 0;
        public decimal? TotalInadimplenciaContrato { get; set; }
        public bool? InadimplenteCondominio => TotalInadimplenciaCondominio.GetValueOrDefault(0) > 0;
        public decimal? TotalInadimplenciaCondominio { get; set; }
        public decimal? TotalInadimplencia => TotalInadimplenciaContrato.GetValueOrDefault(0) + TotalInadimplenciaCondominio.GetValueOrDefault(0);
    }
}
