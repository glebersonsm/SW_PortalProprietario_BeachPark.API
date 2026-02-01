namespace CMDomain.Models.Financeiro
{
    public class ContaPagarAlteradoresValoresInputModel : ModelRequestBase
    {
        public int? IdDocumento { get; set; }
        public List<ContaPagarAlteradorValorInputModel>? AlteradoresIncluir { get; set; }
    }

}
