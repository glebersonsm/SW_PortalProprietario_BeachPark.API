using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;

namespace SW_PortalProprietario.Application.Models.PessoaModels
{
    public class TipoTelefoneInputModel : CreateUpdateModelBase
    {
        public string? Nome { get; set; }
        public string? Mascara { get; set; }

        public static explicit operator TipoTelefone(TipoTelefoneInputModel model)
        {
            return new TipoTelefone
            {
                Nome = model.Nome,
                Mascara = model.Mascara
            };
        }

    }
}
