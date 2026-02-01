using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.PessoaModels
{
    public class TipoDocumentoPessoaModel : ModelBase
    {
        public string? Nome { get; set; }
        public string? Mascara { get; set; }
        public EnumSimNao? ExigeOrgaoEmissor { get; set; }
        public EnumSimNao? ExigeDataEmissao { get; set; }
        public EnumSimNao? ExigeDataValidade { get; set; }
        public EnumTiposPessoa? TipoPessoa { get; set; } = EnumTiposPessoa.PessoaFisicaEJuridica;

        public static explicit operator TipoDocumentoPessoaModel(TipoDocumentoPessoa model)
        {
            return new TipoDocumentoPessoaModel
            {
                Id = model.Id,
                UsuarioCriacao = model.UsuarioCriacao,
                DataHoraCriacao = model.DataHoraCriacao,
                UsuarioAlteracao = model.UsuarioAlteracao,
                DataHoraAlteracao = model.DataHoraAlteracao,
                Nome = model.Nome,
                Mascara = model.Mascara,
                TipoPessoa = model.TipoPessoa,
                ExigeOrgaoEmissor = model.ExigeOrgaoEmissor,
                ExigeDataEmissao = model.ExigeDataEmissao,
                ExigeDataValidade = model.ExigeDataValidade

            };
        }

    }
}
