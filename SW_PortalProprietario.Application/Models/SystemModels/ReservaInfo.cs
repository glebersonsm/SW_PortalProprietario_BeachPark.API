using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SW_PortalProprietario.Application.Models.SystemModels
{
    public class ReservaInfo
    {
        public long ReservaId { get; set; }
        public long? NumReserva { get; set; }
        public int? IdReservasFront { get; set; }
        public long AgendamentoId { get; set; }
        public string EmailCliente { get; set; } = string.Empty;
        public DateTime DataCheckIn { get; set; }
        public int? EmpresaId { get; set; }
        public string? CotaNome { get; set; }
        public string? UhCondominioNumero { get; set; }
        public int? ClienteId { get; set; }
        public string? ClienteNome { get; set; }
    }
}
