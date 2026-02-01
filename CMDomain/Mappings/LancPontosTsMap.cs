using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class LancPontosTsMap : ClassMap<LancPontosTs>
    {
        public LancPontosTsMap()
        {
            Id(x => x.IdLancPontosTs).GeneratedBy.Sequence("SEQLANCPONTOSTS");

            Map(p => p.IdVendaXContrato);
            Map(p => p.NumeroPontos);
            Map(p => p.DebitoCredito);
            Map(p => p.IdReservasFront);
            Map(p => p.IdUsuario);
            Map(p => p.DataLancamento);
            Map(p => p.IdHotel);
            Map(p => p.IdUsuarioLogado);
            Map(p => p.FlgMigrado);
            Map(p => p.IdTipoLancPontoTs);
            Map(p => p.ValidadeCredito);
            Map(p => p.FlgVlrManual);
            Map(p => p.FlgAssociada);
            Map(p => p.IdUsuarioReserva);
            Map(p => p.IdContrXPontoCobrado);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("LancPontosTs");
        }
    }
}
