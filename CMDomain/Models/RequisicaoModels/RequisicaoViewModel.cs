


using CMDomain.Entities;

namespace CMDomain.Models.RequisicaoModels
{
    public class RequisicaoViewModel
    {
        public Int64? NumRequisicao { get; set; }
        public int? IdPessoa { get; set; }
        public int? UnidNegoc { get; set; }
        public int? IdProcesso { get; set; }
        public int? IdUsuarioInclusao { get; set; }
        public string? CustoTransf { get; set; }
        public int? IdEmpresa { get; set; }
        public string? CodCentroCusto { get; set; }
        public DateTime? DataEmissao { get; set; }
        public string? ReqAtendida { get; set; }
        public int? CodAlmoxaOrigem { get; set; }
        public DateTime? DataNecessidade { get; set; }
        public string? Impresso { get; set; }
        public int? CodAlmoxaDestino { get; set; }
        public string? Obs { get; set; }
        public int? IdNotaTransf { get; set; }
        public int? IdEmpresaDestino { get; set; }
        public string? CodCentroCustoOrigem { get; set; }
        public int? UnidNegocOrigem { get; set; }
        public int? IdPessoaOrigem { get; set; }
        public DateTime? TrgDtInclusao { get; set; }
        public string? TrgUserInclusao { get; set; }

        public List<RequisicaoItemViewModel> Itens { get; set; } = new List<RequisicaoItemViewModel>();

        public static explicit operator RequisicaoViewModel(ReqMat model)
        {
            return new RequisicaoViewModel
            {
                NumRequisicao = model.NumRequisicao,
                IdPessoa = model.IdPessoa,
                UnidNegoc = model.UnidNegoc,
                IdProcesso = model.IdProcesso,
                IdUsuarioInclusao = model.IdUsuarioInclusao,
                CustoTransf = model.CustoTransf,
                IdEmpresa = model.IdEmpresa,
                CodCentroCusto = model.CodCentroCusto,
                DataEmissao = model.DataEmissao,
                ReqAtendida = model.ReqAtendida,
                CodAlmoxaOrigem = model.CodAlmoxaOrigem,
                DataNecessidade = model.DataNecessidade,
                Impresso = model.Impresso,
                CodAlmoxaDestino = model.CodAlmoxaDestino,
                Obs = model.Obs,
                IdNotaTransf = model.IdNotaTransf,
                IdEmpresaDestino = model.IdEmpresaDestino,
                CodCentroCustoOrigem = model.CodCentroCustoOrigem,
                UnidNegocOrigem = model.UnidNegocOrigem,
                IdPessoaOrigem = model.IdPessoaOrigem,
                TrgDtInclusao = model.TrgDtInclusao,
                TrgUserInclusao = model.TrgUserInclusao
            };
        }
    }
}
