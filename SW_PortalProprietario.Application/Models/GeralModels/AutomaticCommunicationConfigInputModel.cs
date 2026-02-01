using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class AutomaticCommunicationConfigInputModel
    {
        public int? Id { get; set; }
        public string CommunicationType { get; set; } = "VoucherReserva";
        public int ProjetoType { get; set; }
        public bool Enabled { get; set; }
        public int? TemplateId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public List<int> DaysBeforeCheckIn { get; set; } = new List<int>();
        public List<int> ExcludedStatusCrcIds { get; set; } = new List<int>();
        public bool SendOnlyToAdimplentes { get; set; } = false;
        public bool AllCompanies { get; set; } = true;
        public List<int> EmpresaIds { get; set; } = new List<int>();
        public int TemplateSendMode { get; set; } = 1; // Default: BodyHtmlOnly
        public string? SimulationEmail { get; set; } // Email opcional para simulação automática após sincronização
    }
}

