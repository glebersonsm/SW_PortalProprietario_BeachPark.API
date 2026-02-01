namespace SW_PortalProprietario.Domain.Entities.Core.Geral
{
    public class RegraPaxFreeHotel : EntityBaseCore, IEntityValidateCore
    {
        public virtual RegraPaxFree? RegraPaxFree { get; set; }
        public virtual int? HotelId { get; set; }
        public virtual int? UsuarioRemocao { get; set; }
        public virtual DateTime? DataHoraRemocao { get; set; }

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (RegraPaxFree == null)
                mensagens.Add("A RegraPaxFree deve ser informada");

            if (HotelId == null || HotelId <= 0)
                mensagens.Add("O HotelId deve ser informado e maior que zero");

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

