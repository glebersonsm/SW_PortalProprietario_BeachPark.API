using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.PessoaModels
{
    public class PessoaJuridicaModel : ModelBase
    {
        public PessoaJuridicaModel()
        { }

        public DateTime? DataAbertura { get; set; }
        public DateTime? DataEncerramento { get; set; }
        public string? RazaoSocial { get; set; }
        public string? NomeFantasia { get; set; }
        public string? EmailPreferencial { get; set; }
        public string? EmailAlternativo { get; set; }
        public EnumTipoTributacao? RegimeTributario { get; set; }

    }
}

