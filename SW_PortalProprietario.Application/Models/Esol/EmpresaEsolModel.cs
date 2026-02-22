namespace SW_PortalProprietario.Application.Models.Esol
{
    /// <summary>
    /// Modelo de empresa - migrado do SwReservaApiMain (GeralController).
    /// Compatível com o contrato da API SwReserva.
    /// </summary>
    public class EmpresaEsolModel
    {
        public int? EmpresaId { get; set; }
        public string? Nome { get; set; }
        public string? NomeFantasia { get; set; }
        public string? CNPJ { get; set; }
    }
}
