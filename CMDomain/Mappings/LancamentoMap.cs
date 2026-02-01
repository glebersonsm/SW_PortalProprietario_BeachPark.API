using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class LancamentoMap : ClassMap<Lancamento>
    {

        public LancamentoMap()
        {
            CompositeId()
                .KeyProperty(x => x.PlnCodigo)
                .KeyProperty(x => x.LacNumLan)
                .KeyProperty(x => x.LacDebCre);

            Map(p => p.UnidNegoc);
            Map(p => p.IdPlanoPrev);
            Map(p => p.IdPatro);
            Map(p => p.IdElemDemonstrat);
            Map(p => p.HitCodHist);
            Map(p => p.IdPessoa);
            Map(p => p.IdEmpresa);
            Map(p => p.IdModulo);
            Map(p => p.IdUsuarioInclusao);
            Map(p => p.CodCentroCusto);
            Map(p => p.Placonta);
            Map(p => p.Plano);
            Map(p => p.LacTipo);
            Map(p => p.LacNumDoc);
            Map(p => p.LacHist1);
            Map(p => p.LacHist2);
            Map(p => p.LacHist3);
            Map(p => p.LacHist4);
            Map(p => p.LacHist5);
            Map(p => p.LacValor);
            Map(p => p.LacTipConvOficial);
            Map(p => p.LacValOficial);
            Map(p => p.LacTipConvGer);
            Map(p => p.LacValGerencial);
            Map(p => p.LacTipConvGeren1);
            Map(p => p.LacTipConvGeren2);
            Map(p => p.LacValGeren2);
            Map(p => p.LacatOutMoeda);
            Map(p => p.LacOrigemAplic);
            Map(p => p.TipCodigo);
            Map(p => p.LacValHist);
            Map(p => p.CodSubConta);
            Map(p => p.LoteTransmissao);
            Map(p => p.Num_Reserva_Orig);
            Map(p => p.LacTipConvMoeHis);
            Map(p => p.PrCodigoItem);
            Map(p => p.PrQtdInicial);
            Map(p => p.PrIdentIndividual);
            Map(p => p.PrTipo);
            Map(p => p.PrDescricaoItem);
            Map(p => p.PrDataReconhec);
            Map(p => p.PrCnpjEmpInvest);
            Map(p => p.PrParcelaRealiz);

            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("Lancamento");
        }
    }
}
