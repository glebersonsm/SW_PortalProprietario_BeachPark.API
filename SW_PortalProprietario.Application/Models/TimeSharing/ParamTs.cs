using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class ParamTs
    {
        public int? IdParamTs {get; set; }
        public int? IdHotel { get; set; }
        public DateTime? DataSistema { get; set; }
        public int? PrazoCancelamento { get; set; }
        public string? TipoPrazoCanc { get; set; }
        public int? NumMaxPernoites { get; set; }
        
    }
}
