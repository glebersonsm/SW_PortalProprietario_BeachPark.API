namespace CMDomain.Models
{
    public class ModelRequestNomeTipoPessoa : ModelRequestBase
    {
        public string? Nome { get; set; }
        public string? PessoaTipo { get; set; } = "J";
    }
}
