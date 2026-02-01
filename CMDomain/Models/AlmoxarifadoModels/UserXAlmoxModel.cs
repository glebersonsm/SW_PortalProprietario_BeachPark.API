using CMDomain.Entities;

namespace CMDomain.Models.AlmoxarifadoModels
{
    public class UserXAlmoxModel
    {
        public string? NomeUsuario { get; set; }
        public int? IdPessoa { get; set; }
        public int? CodAlmoxarifado { get; set; }
        public string? DescAlmox { get; set; }
        public int? IdUsuario { get; set; }

        public static explicit operator UserXAlmoxModel(UsuXAlmox model)
        {
            return new UserXAlmoxModel
            {
                IdPessoa = model.IdPessoa,
                IdUsuario = model.IdUsuario,
                CodAlmoxarifado = model.CodAlmoxarifado
            };
        }
    }
}
