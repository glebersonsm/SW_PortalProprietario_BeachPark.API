namespace CMDomain.Models.AuthModels
{
    public class GetDireitoAcessoTratadoPorModulo : ModelRequestBase
    {
        public int? ModuloId { get; set; }
        public string? ModuloNome { get; set; }
        public int? OperacaoId { get; set; }
        public string? OperacaoNome { get; set; }
        public int? FuncaoId { get; set; }
        public string? FuncaoNome { get; set; }
    }
}
