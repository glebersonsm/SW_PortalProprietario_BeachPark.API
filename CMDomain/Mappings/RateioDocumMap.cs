using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class RateioDocumMap : ClassMap<RateioDocum>
    {
        public RateioDocumMap()
        {
            Id(x => x.IdRateioDocum).GeneratedBy.Assigned();

            Map(p => p.CodDocumento);
            Map(p => p.CodTipRecDes);
            Map(p => p.IdEmpresa);
            Map(p => p.CodCentroCusto);
            Map(p => p.RecPag);
            Map(p => p.IdPessoa);
            Map(p => p.CodCentroRespon);
            Map(p => p.UnidNegoc);
            Map(p => p.Valor);
            Map(p => p.ValorOutraMoeda);
            Map(p => p.IdUsuarioInclusao);
            Map(p => p.CodFiscal);
            Map(p => p.IdItemOc);
            Map(p => p.Plano);
            Map(p => p.TrgUserInclusao);

            Table("RateioDocum");
        }
    }
}
