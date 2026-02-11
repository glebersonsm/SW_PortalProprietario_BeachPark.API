namespace SW_PortalProprietario.Application.Models.SystemModels
{
    public class ResetPasswordoUserModel
    {
        public string? Login { get; set; }
        /// <summary> Canal escolhido para envio da nova senha: "email" | "sms" </summary>
        public string? Channel { get; set; }
    }
}
