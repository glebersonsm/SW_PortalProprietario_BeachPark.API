using SW_PortalProprietario.Domain.Entities.Core;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Domain.Entities.Core.Geral
{
    public class AutomaticCommunicationSent : EntityBaseCore
    {
        public virtual string CommunicationType { get; set; } = string.Empty;
        public virtual long ReservaId { get; set; }
        public virtual int DaysBeforeCheckIn { get; set; }
        public virtual DateTime DataCheckIn { get; set; }
        public virtual DateTime DataEnvio { get; set; }
        public virtual int? EmailId { get; set; }
        public virtual int? FrAtendimentoVendaId { get; set; }
        public virtual EnumProjetoType EmpreendimentoTipo { get; set; } = EnumProjetoType.Multipropriedade;
    }
}

