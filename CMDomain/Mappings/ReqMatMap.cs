using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class ReqMatMap : ClassMap<ReqMat>
    {
        public ReqMatMap()
        {
            Id(x => x.NumRequisicao)
                .GeneratedBy.Assigned();

            Map(p => p.IdPessoa);

            Map(b => b.IdUsuarioInclusao);

            Map(b => b.IdProcesso);

            Map(p => p.UnidNegoc);

            Map(b => b.CustoTransf);

            Map(b => b.IdEmpresa);

            Map(b => b.IdEmpresaDestino);
            Map(b => b.CodCentroCusto);
            Map(b => b.DataEmissao);
            Map(b => b.ReqAtendida);
            Map(b => b.CodAlmoxaOrigem);
            Map(b => b.CodAlmoxaDestino);
            Map(b => b.DataNecessidade);
            Map(b => b.Impresso);
            Map(b => b.Obs);
            Map(b => b.IdEvento);
            Map(b => b.IdNotaTransf);
            Map(b => b.CodCentroCustoOrigem);
            Map(b => b.UnidNegocOrigem);
            Map(b => b.IdPessoaOrigem);

            Map(b => b.TrgDtInclusao);
            Map(b => b.TrgUserInclusao);

            Table("ReqMat");
        }
    }
}
