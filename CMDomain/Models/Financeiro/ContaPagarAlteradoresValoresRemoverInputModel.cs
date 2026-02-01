namespace CMDomain.Models.Financeiro
{
    public class ContaPagarAlteradoresValoresRemoverInputModel : ModelRequestBase
    {
        public int? IdDocumento { get; set; }
        public List<int>? AlteradoresRemover { get; set; }
    }

}
