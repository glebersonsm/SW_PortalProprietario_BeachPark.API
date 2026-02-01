namespace SW_Utils.Models
{
    public class AlteracaoResultModel
    {
        public string? TipoCampo { get; set; }
        public string? NomeCampo { get; set; }
        public object? ValorAntes { get; set; }
        public object? ValorApos { get; set; }
    }
}
