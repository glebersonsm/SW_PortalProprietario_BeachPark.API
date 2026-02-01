


using CMDomain.Entities;

namespace CMDomain.Models.CentroCustoModels
{
    public class CentroCustoViewModel
    {
        public string? CodCentroCusto { get; set; }
        public int? IdEmpresa { get; set; }
        public string? Nome { get; set; }
        public string? AnaliticoSintetico { get; set; }
        public string? Ativo { get; set; }
        public string? CodExterno { get; set; }

        public static explicit operator CentroCustoViewModel(CentCust model)
        {
            return new CentroCustoViewModel
            {
                CodCentroCusto = model.CodCentroCusto,
                IdEmpresa = model.IdEmpresa,
                Nome = model.Nome,
                AnaliticoSintetico = model.StatusGrupoCDC,
                Ativo = model.Ativo,
                CodExterno = model.CodExterno
            };
        }
    }
}
