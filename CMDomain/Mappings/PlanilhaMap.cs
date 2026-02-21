using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class PlanilhaMap : ClassMap<Planilha>
    {
        public PlanilhaMap()
        {
            Id(x => x.PlnCodigo).GeneratedBy.Assigned();

            Map(p => p.IdModulo);
            Map(p => p.PanCodigo);
            Map(p => p.PerNumero);
            Map(p => p.PerExercicio);
            Map(p => p.PlnDatDia);
            Map(p => p.PlnPlanil);
            Map(p => p.PlnNumLan);
            Map(p => p.TipCodigo);
            Map(p => p.PlnTotDebOficial);
            Map(p => p.PlnTotCreOficial);
            Map(p => p.PlnTotDebHist);
            Map(p => p.PlnTotCreHist);
            Map(p => p.PlnTotDebGer);
            Map(p => p.PlnTotCreGer);
            Map(p => p.PlnTotDebGeren1);
            Map(p => p.PlnTotCreGeren1);
            Map(p => p.PlnTotDebGeren2);
            Map(p => p.PlnTotCreGeren2);
            Map(p => p.PlnTotDeb);
            Map(p => p.PlnTotCre);
            Map(p => p.IdUsuarioInclusao);
            Map(p => p.IdPessoa);
            Map(p => p.PlnEmUso);
            Map(p => p.LoteTransmissao);
            Map(p => p.PlnPlanEstorno);
            Map(p => p.PlnReferencia);
            Map(p => p.FlgEncerramento);
            Map(p => p.IdEmpresaOrigem);
            Map(p => p.DtExtemporaneo);
            Map(p => p.PlnRefOutroSis);
            Map(p => p.PlnEfetivado);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("Planilha");
            Schema("cm");
        }
    }
}
