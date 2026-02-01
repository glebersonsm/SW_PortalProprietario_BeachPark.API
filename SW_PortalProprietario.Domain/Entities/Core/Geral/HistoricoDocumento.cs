namespace SW_PortalProprietario.Domain.Entities.Core.Geral
{
    public class HistoricoDocumento : EntityBaseCore, IEntityValidateCore
    {
        public virtual Documento? Documento { get; set; }
        public virtual string? Acao { get; set; }
        public virtual string? Path { get; set; }
        public virtual DateTime? DataHoraRemocao { get; set; }
        public virtual int? UsuarioRemocao { get; set; }

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (Documento == null)
                mensagens.Add("O Documento deve ser informado");

            if (string.IsNullOrEmpty(Acao))
                mensagens.Add($"A ação deve ser informada");

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
