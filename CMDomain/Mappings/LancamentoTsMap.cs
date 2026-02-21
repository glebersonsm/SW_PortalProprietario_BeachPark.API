using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class LancamentoTsMap : ClassMap<LancamentoTs>
    {
        public LancamentoTsMap()
        {
            Id(x => x.IdLancamentoTs).GeneratedBy.Sequence("SEQLANCAMENTOTS");

            Map(p => p.IdLancPontosTs);
            Map(p => p.IdTipoDebCred);
            Map(p => p.IdHotel);
            Map(p => p.VlrLancamento);
            Map(p => p.VlrAPagar);
            Map(p => p.IdTipoLancamento);
            Map(p => p.DataLancamento);
            Map(p => p.DataPagamento);
            Map(p => p.Documento);
            Map(p => p.IdUsuario);
            Map(p => p.IdVendaTs);
            Map(p => p.ValidadeCredito);
            Map(p => p.FlgTaxaAdm);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("LancamentoTs");
            Schema("cm");
        }
    }
}
