namespace SW_PortalProprietario.Domain.Entities.Core.Geral
{
    public class RegraPaxFree : EntityBaseCore, IEntityValidateCore
    {
        public virtual string? Nome { get; set; }
        public virtual DateTime? DataInicioVigencia { get; set; }
        public virtual DateTime? DataFimVigencia { get; set; }
        public virtual int? UsuarioRemocao { get; set; }
        public virtual DateTime? DataHoraRemocao { get; set; }

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (string.IsNullOrEmpty(Nome))
                mensagens.Add("O Nome da regra deve ser informado");

            if (mensagens.Any())
                await Task.FromException(new ArgumentException(string.Join($"{Environment.NewLine}", mensagens.Select(a => a).ToList())));
            else await Task.CompletedTask;
        }

        public virtual object Clone()
        {
            return MemberwiseClone();
        }
    }
}

