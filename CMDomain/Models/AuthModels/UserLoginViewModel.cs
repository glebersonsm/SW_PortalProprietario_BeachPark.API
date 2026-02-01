


using CMDomain.Entities;

namespace CMDomain.Models.AuthModels
{
    public class UserLoginViewModel
    {
        public string? CMUserName { get; set; }
        public string? Token { get; set; }

        public static explicit operator UserLoginViewModel(UsuarioSistema model)
        {
            return new UserLoginViewModel
            {
                CMUserName = model.NomeUsuario,
                Token = model.SwPasswordHash
            };
        }
    }
}
