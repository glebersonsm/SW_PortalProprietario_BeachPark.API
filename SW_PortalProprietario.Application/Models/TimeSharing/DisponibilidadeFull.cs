using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class DisponibilidadeDoHotel
    {
        public int? IdHotel { get; set; }
        public IList<DisponibilidadeDia> DisponibilidadesDias = new List<DisponibilidadeDia>();
        public IList<DisponibilidadeDia> TodosAptos = new List<DisponibilidadeDia>();
    }

    public class DisponibilidadeDia
    {
        public DateTime Data { get; set; }
        public int IdTipoUh { get; set; }
        public string? CodigoTipoUh { get; set; }
        public int QtdeTotalUh { get; set; }
        public int Capacidade { get; set; }
        public decimal? OcupacaoTipoApartamentoNoDia { get; set; }
        public decimal? OcupacaoGeralHotel { get; set; }
        public int QtdeDisponivel
        {
            get
            {
                return QtdeTotalUh - (QtdeBloqueadasManutencao + QuantidadeUtilizada);
            }
        }
        public string? NomeTipoApto { get; set; }
        public int QtdeBloqueadasManutencao { get; set; }
        public int QuantidadeUtilizada { get; set; }
    }
}
