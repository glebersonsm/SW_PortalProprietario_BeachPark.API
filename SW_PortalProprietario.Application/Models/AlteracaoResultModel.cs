using SW_Utils.Interfaces;

namespace SW_PortalProprietario.Application.Models
{
    public class AlteracaoResultModel : IAlteracaoResultModel
    {
        public string? TipoCampo { get; set; }
        public string? NomeCampo { get; set; }
        public object? ValorAntes { get; set; }
        public object? ValorApos { get; set; }
    }
}
