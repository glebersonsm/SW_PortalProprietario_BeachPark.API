using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class SoliCompMap : ClassMap<SoliComp>
    {
        public SoliCompMap()
        {
            Id(x => x.NumSolCompra).GeneratedBy.Assigned();

            Map(p => p.IdPessoa);

            Map(b => b.IdUsuario);

            Map(b => b.NumRequisicao);

            Map(p => p.IdProcesso);

            Map(b => b.IdReservaOrcamen);

            Map(b => b.CodAlmoxarifado);

            Map(b => b.UnidNegoc);
            Map(b => b.DataEntrega);
            Map(b => b.IdEmpresa);
            Map(b => b.CodCentroCusto);
            Map(b => b.AlgumParaEstoque);
            Map(b => b.DataEmissao);
            Map(b => b.SoliciAtendida);
            Map(b => b.SoliciAceita);
            Map(b => b.CustoEstoque);
            Map(b => b.Impresso);
            Map(b => b.FlgPrePronta);
            Map(b => b.IdContPermuta);
            Map(b => b.Status);
            Map(b => b.IdArquivo);
            Map(b => b.IdProcessoSecundario);
            Map(b => b.IdProcessoMaster);
            Map(b => b.FlgUrgente);
            Map(b => b.CodCentroRespon);
            Map(b => b.FlgWs);
            Map(b => b.FlgStatusWs);

            Map(b => b.TrgDtInclusao);
            Map(b => b.TrgUserInclusao);

            Table("SoliComp");
            Schema("cm");
        }
    }
}
