namespace SW_PortalProprietario.Application.Models.AuthModels
{
    public class UsuarioValidateResultModel
    {
        public int? Id { get; set; }
        public int? CotaPortalId { get; set; }
        public int? CotaAccessCenterId { get; set; }
        public string? PessoaLegadoId { get; set; }
        public int? UhCondominioId { get; set; }
        public string? Estrangeiro { get; set; }
        public VinculoAccessXPortalBase? VinculoAccessXPortal { get; set; }


    }
}
