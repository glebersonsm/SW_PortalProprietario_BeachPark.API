using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Domain.Entities.Core.Sistema
{
    public class Usuario : EntityBaseCore, IEntityValidateCore
    {
        public virtual Pessoa? Pessoa { get; set; }
        public virtual string? Login { get; set; }
        public virtual EnumSimNao? Administrador { get; set; }
        public virtual EnumSimNao? GestorFinanceiro { get; set; }
        public virtual EnumSimNao? GestorReservasAgendamentos { get; set; }
        public virtual EnumSimNao? UsuarioAdministrativo { get; set; }
        public virtual string? PasswordHash { get; set; }
        public virtual EnumStatus? Status { get; set; } = EnumStatus.Ativo;
        public virtual string? ProviderChaveUsuario { get; set; }
        public virtual string? TokenResult { get; set; }
        public virtual DateTime? DataHoraRemocao { get; set; }
        public virtual EnumSimNao? Removido { get; set; }
        public virtual string? LoginPms { get; set; }
        public virtual string? LoginSistemaVenda { get; set; }
        public virtual string? AvatarBase64 { get; set; }
        public virtual string? MenuPermissions { get; set; }

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (Pessoa == null)
                mensagens.Add("A Pessoa do Usuário deve ser informada");

            if (string.IsNullOrEmpty(Login))
                mensagens.Add("O Login do Usuário deve ser informado");

            if (string.IsNullOrEmpty(PasswordHash))
                mensagens.Add($"A PasswordHash deve ser informada");

            if (!Status.HasValue)
                mensagens.Add($"O Status do Usuário deve ser informado");


            if (mensagens.Any())
                await Task.FromException(new ArgumentException(string.Join($"{Environment.NewLine}", mensagens.Select(a => a).ToList())));
            else await Task.CompletedTask;
        }

    }
}
