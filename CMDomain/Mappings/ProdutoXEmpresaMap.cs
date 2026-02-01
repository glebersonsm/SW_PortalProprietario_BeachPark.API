using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class ProdutoXEmpresaMap : ClassMap<ProdutoXEmpresa>
    {
        public ProdutoXEmpresaMap()
        {
            Id(x => x.IdProdutoXEmpresa).GeneratedBy.Sequence("SEQPRODUTOXEMPRESA");//seqprodutoxempresa

            Map(p => p.SituacaoTrib);
            Map(p => p.IdPessoa);
            Map(p => p.CodProduto);
            Map(p => p.IsentoOutros);
            Map(p => p.CodFiscalPadrao);
            Map(p => p.ConsumoRevenda);
            Map(p => p.SituacaoTribA);
            Map(p => p.CodStCofins);
            Map(p => p.CodStPis);
            Map(p => p.SituacaoTribSaida);
            Map(p => p.CodStPisSaida);
            Map(p => p.CodStCofinsSaida);
            Map(p => p.CodStIpiSaida);
            Map(p => p.CodStIpi);
            Map(p => p.FlgRegraCalc);
            Map(p => p.RegimeApuracao);
            Map(p => p.FlgIntTHex);
            Map(p => p.TipoEstoque);
            Map(p => p.FlgIndicadorProp);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("ProdutoXEmpresa");
        }
    }
}
