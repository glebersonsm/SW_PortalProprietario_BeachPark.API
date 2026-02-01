using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class LanctoDocumMap : ClassMap<LanctoDocum>
    {
        public LanctoDocumMap()
        {

            CompositeId()
                .KeyProperty(p => p.CodDocumento)
                .KeyProperty(p => p.NumLancto);

            Map(p => p.IdPessoa);
            Map(p => p.CodAlterador);
            Map(p => p.DataLancto);
            Map(p => p.Valor);
            Map(p => p.DebCre);
            Map(p => p.HistoricoCompl);
            Map(p => p.ValorOutraMoeda);
            Map(p => p.Operacao);
            Map(p => p.IdUsuarioInclusao);
            Map(p => p.Estorno);
            Map(p => p.VlrLiquido);
            Map(p => p.NumRecibo);
            Map(p => p.CodTipDoc);
            Map(p => p.PlnCodigo);
            Map(p => p.FlgContabilizado);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);
            Map(p => p.TrgDtAlteracao);
            Map(p => p.TrgUserAlteracao);

            Table("LanctoDocum");
        }
    }
}
