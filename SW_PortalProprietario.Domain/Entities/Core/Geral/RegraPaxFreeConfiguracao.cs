namespace SW_PortalProprietario.Domain.Entities.Core.Geral
{
    public class RegraPaxFreeConfiguracao : EntityBaseCore, IEntityValidateCore
    {
        public virtual RegraPaxFree? RegraPaxFree { get; set; }
        public virtual int? QuantidadeAdultos { get; set; }
        public virtual int? QuantidadePessoasFree { get; set; }
        public virtual int? IdadeMaximaAnos { get; set; }
        public virtual string? TipoOperadorIdade { get; set; } // ">=" para superior ou igual, "<=" para inferior ou igual
        public virtual string? TipoDataReferencia { get; set; } // "RESERVA" para data da reserva (hoje), "CHECKIN" para data de check-in
        public virtual int? UsuarioRemocao { get; set; }
        public virtual DateTime? DataHoraRemocao { get; set; }

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (RegraPaxFree == null)
                mensagens.Add("A RegraPaxFree deve ser informada");

            if (QuantidadeAdultos == null || QuantidadeAdultos <= 0)
                mensagens.Add("A Quantidade de Adultos deve ser informada e maior que zero");

            if (IdadeMaximaAnos == null || IdadeMaximaAnos < 0)
                mensagens.Add("A Idade MÃ¡xima em Anos deve ser informada e maior ou igual a zero");

            if (QuantidadePessoasFree == null || QuantidadePessoasFree < 0)
                mensagens.Add("A Quantidade de pessoas free deve ser informada.");

            if (string.IsNullOrEmpty(TipoOperadorIdade) || (TipoOperadorIdade != ">=" && TipoOperadorIdade != "<="))
                mensagens.Add("O Tipo de Operador de Idade deve ser informado e deve ser '>=' ou '<='");

            if (!string.IsNullOrEmpty(TipoDataReferencia) && TipoDataReferencia != "RESERVA" && TipoDataReferencia != "CHECKIN")
                mensagens.Add("O Tipo de Data de ReferÃªncia deve ser 'RESERVA' ou 'CHECKIN'");

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

