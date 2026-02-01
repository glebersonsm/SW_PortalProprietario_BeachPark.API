using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class CotacoesMap : ClassMap<Cotacoes>
    {
        public CotacoesMap()
        {
            CompositeId()
                .KeyProperty(p => p.IdProcXArt)
                .KeyProperty(p => p.IdForCli)
                .KeyProperty(p => p.CodProcesso)
                .KeyProperty(p => p.Proposta);


            Map(p => p.QtdeFornecida);
            Map(p => p.IdCondicoesPagto);
            Map(p => p.IdItemOc);
            Map(p => p.CodMedida);
            Map(p => p.NumCot);
            Map(p => p.DataCot);
            Map(p => p.Status);
            Map(p => p.Obs);
            Map(p => p.TxJuros);
            Map(p => p.PrecoAValorPres);
            Map(p => p.Contato);
            Map(p => p.PrecoAValorEstoq);
            Map(p => p.IdArquivo);
            Map(p => p.Preco);
            Map(p => p.TrgUserInclusao);
            Map(p => p.TrgDtInclusao);

            Table("Cotacoes");
        }
    }
}
