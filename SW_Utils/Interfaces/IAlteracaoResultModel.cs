namespace SW_Utils.Interfaces
{
    public interface IAlteracaoResultModel
    {
        string? NomeCampo { get; set; }
        string? TipoCampo { get; set; }
        object? ValorAntes { get; set; }
        object? ValorApos { get; set; }
    }
}
