using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;

namespace SW_PortalProprietario.Application.Models.PessoaModels
{
    public class TipoEnderecoInputModel : CreateUpdateModelBase
    {
        public string? Nome { get; set; }

        public static explicit operator TipoEndereco(TipoEnderecoInputModel model)
        {
            return new TipoEndereco
            {
                Nome = model.Nome

            };
        }
    }
}
