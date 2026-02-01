


using CMDomain.Entities;




namespace CMDomain.Models.AlmoxarifadoModels
{
    public class AlmoxarifadoViewModel
    {
        public int? CodAlmoxarifado { get; set; }
        public int? CodCusteio { get; set; }
        public int? IdPessoa { get; set; }
        public string? CodCentroCusto { get; set; }
        public int? IdEmpresa { get; set; }
        public string? DescAlmox { get; set; }
        public string PrincipSecund { get; set; } = "P";
        public string Contabil { get; set; } = "T";


        public static explicit operator AlmoxarifadoViewModel(Almox model)
        {
            return new AlmoxarifadoViewModel
            {
                CodAlmoxarifado = model.CodAlmoxarifado,
                CodCusteio = model.CodCusteio,
                IdPessoa = model.IdPessoa,
                CodCentroCusto = model.CodCentroCusto,
                IdEmpresa = model.IdEmpresa,
                DescAlmox = model.DescAlmox,
                PrincipSecund = model.PrincipSecund,
                Contabil = model.Contabil
            };
        }
    }
}
