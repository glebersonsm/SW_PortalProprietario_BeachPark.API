using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;

namespace SW_PortalProprietario.Application.Models.PessoaModels
{
    public class TipoTelefoneModel : ModelBase
    {
        public TipoTelefoneModel()
        { }

        public string? Nome { get; set; }
        public string? Mascara { get; set; }

        public static explicit operator TipoTelefone(TipoTelefoneModel model)
        {
            return new TipoTelefone
            {
                Id = model.Id.GetValueOrDefault(),
                UsuarioCriacao = model.UsuarioCriacao,
                DataHoraCriacao = model.DataHoraCriacao,
                Nome = model.Nome,
                Mascara = model.Mascara
            };
        }

        public static explicit operator TipoTelefoneModel(TipoTelefone model)
        {
            return new TipoTelefoneModel
            {
                Id = model.Id,
                UsuarioCriacao = model.UsuarioCriacao,
                DataHoraCriacao = model.DataHoraCriacao,
                UsuarioAlteracao = model.UsuarioAlteracao,
                DataHoraAlteracao = model.DataHoraAlteracao,
                Nome = model.Nome,
                Mascara = model.Mascara
            };
        }
    }
}
