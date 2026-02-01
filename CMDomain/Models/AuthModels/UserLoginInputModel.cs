
using System.ComponentModel;

namespace CMDomain.Models.AuthModels
{
    public class UserLoginInputModel
    {
        public string? CMUserName { get; set; }
        [PasswordPropertyText(true)]
        public string? Password { get; set; }

    }
}
