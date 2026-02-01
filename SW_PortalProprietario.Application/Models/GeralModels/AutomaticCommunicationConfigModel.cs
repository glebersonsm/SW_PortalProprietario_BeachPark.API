using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class AutomaticCommunicationConfigModel : ModelBase
    {
        public string CommunicationType { get; set; } = string.Empty;
        public int ProjetoType { get; set; }
        public bool Enabled { get; set; }
        public int? TemplateId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public List<int> DaysBeforeCheckIn { get; set; } = new List<int>();
        public List<int> ExcludedStatusCrcIds { get; set; } = new List<int>();
        public bool SendOnlyToAdimplentes { get; set; } = false;
        public bool AllCompanies { get; set; } = true;
        public List<int> EmpresaIds { get; set; } = new List<int>();
        public EnumTemplateSendMode? TemplateSendMode { get; set; } = EnumTemplateSendMode.BodyHtmlOnly;
    }
}

