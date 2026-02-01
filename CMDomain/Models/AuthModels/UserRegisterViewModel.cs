


using CMDomain.Entities;

namespace CMDomain.Models.AuthModels
{
    public class UserRegisterViewModel
    {
        public string? CMUserName { get; set; }
        public string? PasswordHash { get; set; }

        public static explicit operator UserRegisterViewModel(UsuarioSistema model)
        {
            return new UserRegisterViewModel
            {
                CMUserName = model.NomeUsuario,
                PasswordHash = model.SwPasswordHash
            };
        }
    }
}
