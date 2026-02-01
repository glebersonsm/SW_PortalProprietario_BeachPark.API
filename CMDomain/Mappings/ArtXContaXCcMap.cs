using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class ArtXContaXCcMap : ClassMap<ArtXContaXCc>
    {
        public ArtXContaXCcMap()
        {
            Id(b => b.IdArtXContaXCc).GeneratedBy.Sequence("SEQARTXCONTAXCC");

            Map(p => p.CodAlmoxarifado);
            Map(p => p.IdPessoa);
            Map(p => p.Plano);
            Map(p => p.IdEmpresa);
            Map(p => p.CodCentroCusto);
            Map(p => p.ContaEntrada);
            Map(p => p.ContaSaida);
            Map(p => p.CodArtigo);
            Map(p => p.CodGrupoProd);
            Map(p => p.UnidNegoc);
            Map(p => p.SubContaEntrada);
            Map(p => p.SubContaSaida);
            Map(p => p.TrgUserInclusao);
            Map(p => p.TrgDtInclusao);

            Table("ArtXContaXCc");
        }
    }
}
