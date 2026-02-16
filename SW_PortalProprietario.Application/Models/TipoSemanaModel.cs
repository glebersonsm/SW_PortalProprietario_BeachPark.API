using CMDomain.Entities;
using CMDomain.Models.AlmoxarifadoModels;
using Dapper;

namespace SW_PortalProprietario.Application.Models
{
    public class TipoSemanaModel
    {
        public Int64? Id { get; set; }
        public Int64? Empresa { get; set; }
        public string? Nome { get; set; }
        public string? Complemento { get; set; }
        public string? Label => $"{Id} - {Nome}";

    }
}
