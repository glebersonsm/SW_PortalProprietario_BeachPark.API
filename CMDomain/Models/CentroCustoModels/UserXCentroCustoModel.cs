


using CMDomain.Entities;

namespace CMDomain.Models.CentroCustoModels
{
    public class UserXCentroCustoModel
    {
        public string? CMUserName { get; set; }
        public int? IdEmpresa { get; set; }
        public int? IdPessoa { get; set; }
        public string? CodCentroCusto { get; set; }
        public int? IdUsuario { get; set; }

        public static explicit operator UserXCentroCustoModel(UsCCusto model)
        {
            return new UserXCentroCustoModel
            {
                IdEmpresa = model.IdEmpresa,
                IdUsuario = model.IdUsuario,
                CodCentroCusto = model.CodCentroCusto,
                IdPessoa = model.IdPessoa
            };
        }
    }
}
