namespace SW_PortalProprietario.Application.Models.TransacoesFinanceiras
{
    public class TokenizeMyCardInputModel
    {
        public CardModel? card { get; set; }
        public bool? KeepCardData { get; set; } = true;
    }
}
