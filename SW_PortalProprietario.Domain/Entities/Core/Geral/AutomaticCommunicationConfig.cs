using SW_PortalProprietario.Domain.Entities.Core;
using SW_PortalProprietario.Domain.Enumns;
using System.Text.Json;

namespace SW_PortalProprietario.Domain.Entities.Core.Geral
{
    public class AutomaticCommunicationConfig : EntityBaseCore
    {
        public virtual EnumDocumentTemplateType CommunicationType { get; set; }
        public virtual EnumProjetoType ProjetoType { get; set; }
        public virtual bool Enabled { get; set; }
        public virtual int? TemplateId { get; set; }
        public virtual string Subject { get; set; } = string.Empty;
        public virtual string DaysBeforeCheckInJson { get; set; } = "[]";
        public virtual string ExcludedStatusCrcIdsJson { get; set; } = "[]";
        public virtual bool SendOnlyToAdimplentes { get; set; } = false;
        public virtual bool AllCompanies { get; set; } = true;
        public virtual string EmpresaIdsJson { get; set; } = "[]";
        public virtual EnumTemplateSendMode TemplateSendMode { get; set; } = EnumTemplateSendMode.BodyHtmlOnly;

        public virtual List<int> GetDaysBeforeCheckIn()
        {
            if (string.IsNullOrEmpty(DaysBeforeCheckInJson))
                return new List<int>();
            
            try
            {
                return JsonSerializer.Deserialize<List<int>>(DaysBeforeCheckInJson) ?? new List<int>();
            }
            catch
            {
                return new List<int>();
            }
        }

        public virtual void SetDaysBeforeCheckIn(List<int> days)
        {
            DaysBeforeCheckInJson = JsonSerializer.Serialize(days ?? new List<int>());
        }

        public virtual List<int> GetExcludedStatusCrcIds()
        {
            if (string.IsNullOrEmpty(ExcludedStatusCrcIdsJson))
                return new List<int>();
            
            try
            {
                return JsonSerializer.Deserialize<List<int>>(ExcludedStatusCrcIdsJson) ?? new List<int>();
            }
            catch
            {
                return new List<int>();
            }
        }

        public virtual void SetExcludedStatusCrcIds(List<int> statusCrcIds)
        {
            ExcludedStatusCrcIdsJson = JsonSerializer.Serialize(statusCrcIds ?? new List<int>());
        }

        public virtual List<int> GetEmpresaIds()
        {
            if (string.IsNullOrEmpty(EmpresaIdsJson))
                return new List<int>();
            
            try
            {
                return JsonSerializer.Deserialize<List<int>>(EmpresaIdsJson) ?? new List<int>();
            }
            catch
            {
                return new List<int>();
            }
        }

        public virtual void SetEmpresaIds(List<int> empresaIds)
        {
            EmpresaIdsJson = JsonSerializer.Serialize(empresaIds ?? new List<int>());
        }
    }
}

