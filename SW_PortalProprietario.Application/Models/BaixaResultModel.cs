namespace SW_PortalProprietario.Application.Models
{
    public class BaixaResultModel
    {
        public int? Id { get; set; }
        public List<string>? Erros { get; set; } = new List<string>();
    }
}
