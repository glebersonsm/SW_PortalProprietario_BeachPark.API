


using CMDomain.Entities;

namespace CMDomain.Models.GrupoProdutoModels
{
    public class GrupoProdutoViewModel
    {
        public string? CodGrupoProd { get; set; }
        public int? IdPessoa { get; set; }
        public string? DescGrupoProd { get; set; }
        public int? IdNaturezaEstoque { get; set; }
        public string? NomeNaturezaEstoque { get; set; }
        public string? StatusGrupo { get; set; }
        public string? FlgGrupoFixo { get; set; }
        public string? TipoEstoque { get; set; }
        public string? FlgIndicaServico { get; set; }
        public string? CodigoNcm { get; set; }

        public static explicit operator GrupoProdutoViewModel(GrupProd model)
        {
            return new GrupoProdutoViewModel
            {
                CodGrupoProd = model.CodGrupoProd,
                IdPessoa = model.IdPessoa,
                DescGrupoProd = model.DescGrupoProd,
                IdNaturezaEstoque = model.NaturezaEstoque?.IdNaturezaEstoque,
                NomeNaturezaEstoque = model.NaturezaEstoque?.DescNatureza,
                StatusGrupo = model.StatusGrupo,
                FlgGrupoFixo = model.FlgGrupoFixo,
                FlgIndicaServico = model.FlgIndicaServico,
                TipoEstoque = model.TipoEstoque,
                CodigoNcm = model.CodigoNCM
            };
        }
    }
}
