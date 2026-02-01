using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class LogTsMap : ClassMap<LogTs>
    {
        public LogTsMap()
        {
            Id(x => x.IdLogTs).GeneratedBy.Sequence("SEQLOGTS");

            Map(p => p.IdUsuario);
            Map(p => p.IdTipoLogTs);
            Map(p => p.DataSistema);
            Map(p => p.DataHora);
            Map(p => p.Chave);
            Map(p => p.IdUsuarioAut);
            Map(p => p.IdLancPagRecorrenteTs); 
            Map(p => p.IdLancamentoTs);
            Map(p => p.Status);
            Map(p => p.IdVendaXContrato);
            Map(p => p.IdCliente);
            Map(p => p.IdInfoCartao);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("LogTs");
        }
    }
}
