using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.PessoaModels
{
    public class TipoDocumentoPessoaInputModel : CreateUpdateModelBase
    {
        public string? Nome { get; set; }
        public string? Mascara { get; set; }
        public EnumSimNao? ExigeOrgaoEmissor { get; set; } = EnumSimNao.Nao;
        public EnumSimNao? ExigeDataEmissao { get; set; } = EnumSimNao.Nao;
        public EnumSimNao? ExigeDataValidade { get; set; } = EnumSimNao.Nao;
        public EnumTiposPessoa? TipoPessoa { get; set; } = EnumTiposPessoa.PessoaFisicaEJuridica;

        public static explicit operator TipoDocumentoPessoa(TipoDocumentoPessoaInputModel model)
        {
            return new TipoDocumentoPessoa
            {
                Nome = model.Nome,
                Mascara = model.Mascara,
                ExigeOrgaoEmissor = model.ExigeOrgaoEmissor,
                ExigeDataEmissao = model.ExigeDataEmissao,
                ExigeDataValidade = model.ExigeDataValidade,
                TipoPessoa = model.TipoPessoa
            };
        }

    }
}
