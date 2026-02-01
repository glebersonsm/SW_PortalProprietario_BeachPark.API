namespace CMDomain.Models.AuthModels
{
    public class AlterarStatusUsuarioInputModel : ModelRequestBase
    {
        public int? UsuarioId { get; set; }
        public bool? Desativado { get; set; }
        public bool? Bloqueado { get; set; }

    }
}
