using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class ContratoProdMap : ClassMap<ContratoProd>
    {
        public ContratoProdMap()
        {
            Id(x => x.IdContratoProd)
             .GeneratedBy.Sequence("SEQCONTRATOPROD");

            Map(p => p.IdPessoa);
            Map(p => p.IdEmpresa);
            Map(p => p.IdForCli);
            Map(p => p.CodArtigo);
            Map(p => p.CodMedida);
            Map(p => p.VlrUnitario);
            Map(p => p.PrazoPag);
            Map(p => p.PrazoEntrega);
            Map(p => p.DataInicio);
            Map(p => p.DataTermino);
            Map(p => p.FlgListaPreco);
            Map(p => p.Status);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Schema("cm");
            Table("ContratoProd");
        }
    }
}
