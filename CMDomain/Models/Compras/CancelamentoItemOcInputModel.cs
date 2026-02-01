namespace CMDomain.Models.Compras
{
    public class CancelarItemOcInputModel : ModelRequestBase
    {
        public string? NumOc { get; set; }
        public List<int> IdItensOcCancelar = new List<int>();

    }
}
