using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class OcMap : ClassMap<Oc>
    {
        public OcMap()
        {
            Id(x => x.NumOc).GeneratedBy.Sequence("SEQOC");

            Map(p => p.IdPessoa).Nullable();
            Map(p => p.IdForCli);
            Map(p => p.OcAtendida);
            Map(p => p.FlgImpressa);
            Map(p => p.FlgComSemOc);
            Map(p => p.ObsOc);
            Map(p => p.FlgComSemCot);
            Map(p => p.FlgRegularizaCap);
            Map(p => p.Contato);
            Map(p => p.FlgTipoFrete);
            Map(p => p.IdComprador);
            Map(p => p.DataOc);
            Map(p => p.FlgMail);
            Map(p => p.IdEmpCompradora);
            Map(p => p.FlgDataVencFrete);
            Map(p => p.FlgDataVenc);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("Oc");
        }
    }
}
