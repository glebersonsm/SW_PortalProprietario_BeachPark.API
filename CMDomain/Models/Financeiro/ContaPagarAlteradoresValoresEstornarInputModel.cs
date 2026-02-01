namespace CMDomain.Models.Financeiro
{
    public class ContaPagarAlteradoresValoresEstornarInputModel : ModelRequestBase
    {
        public int? IdDocumento { get; set; }
        public List<int>? NumLancamentosAlteradoresEstornar { get; set; }
    }

}
