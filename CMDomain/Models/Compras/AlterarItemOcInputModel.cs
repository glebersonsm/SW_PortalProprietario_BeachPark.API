namespace CMDomain.Models.Compras
{
    public class AlterarOcInputModel : ModelRequestBase
    {
        public string? NumOc { get; set; }
        public List<ItemOcAlteracaoInputModel> itensAlterar { get; set; } = new List<ItemOcAlteracaoInputModel>();

    }
}
