using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Domain.Entities.Core.Geral
{
    public class Documento : EntityBaseCore, IEntityValidateCore
    {
        public virtual GrupoDocumento? GrupoDocumento { get; set; }
        public virtual string? Nome { get; set; }
        public virtual byte[]? Arquivo { get; set; }
        public virtual string? NomeArquivo { get; set; }
        public virtual string? TipoMime { get; set; }
        public virtual EnumSimNao? DocumentoPublico { get; set; }
        public virtual EnumSimNao? Disponivel { get; set; }
        public virtual int? Ordem { get; set; }
        public virtual DateTime? DataInicioVigencia { get; set; }
        public virtual DateTime? DataFimVigencia { get; set; }
        public virtual string? TagsRequeridas { get; set; }
        public virtual int? UsuarioRemocao { get; set; }
        public virtual DateTime? DataHoraRemocao { get; set; }
        public virtual string? Path { get; set; }
        public virtual string? Cor { get; set; }
        public virtual string? CorTexto { get; set; }

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (GrupoDocumento == null)
                mensagens.Add("O GrupoDocumento deve ser informado");

            if (string.IsNullOrEmpty(Nome))
                mensagens.Add($"O Nome deve ser informado");

            if (Id == 0 && (Arquivo == null || Arquivo.Length == 0))
                mensagens.Add($"O Arquivo deve ser informado");


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
