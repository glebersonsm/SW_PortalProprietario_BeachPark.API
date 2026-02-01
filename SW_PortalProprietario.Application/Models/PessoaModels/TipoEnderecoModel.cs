using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;

namespace SW_PortalProprietario.Application.Models.PessoaModels
{
    public class TipoEnderecoModel : ModelBase
    {
        public TipoEnderecoModel()
        { }

        public string? Nome { get; set; }

        public static explicit operator TipoEndereco(TipoEnderecoModel model)
        {
            return new TipoEndereco
            {
                Id = model.Id.GetValueOrDefault(),
                UsuarioCriacao = model.UsuarioCriacao,
                DataHoraCriacao = model.DataHoraCriacao,
                Nome = model.Nome
            };
        }

        public static explicit operator TipoEnderecoModel(TipoEndereco model)
        {
            return new TipoEnderecoModel
            {
                Id = model.Id,
                UsuarioCriacao = model.UsuarioCriacao,
                DataHoraCriacao = model.DataHoraCriacao,
                UsuarioAlteracao = model.UsuarioAlteracao,
                DataHoraAlteracao = model.DataHoraAlteracao,
                Nome = model.Nome
            };
        }
    }
}
