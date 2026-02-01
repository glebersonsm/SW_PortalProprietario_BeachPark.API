using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class LfItemMap : ClassMap<LfItem>
    {
        public LfItemMap()
        {
            Id(x => x.CodItem)
            .GeneratedBy.Assigned();
            Map(p => p.CodTipoServico);
            Map(p => p.CodArtigoRef);
            Map(p => p.CodUnidadeEst);
            Map(p => p.CodTipi);
            Map(p => p.CodsTipi);
            Map(p => p.CodigoNcm);
            Map(p => p.CodGenero);
            Map(p => p.CodSta);
            Map(p => p.CodStb);
            Map(p => p.CodStPis);
            Map(p => p.CodStCofins);
            Map(p => p.CodGrupoItem);
            Map(p => p.Descricao);
            Map(p => p.Quantidade);
            Map(p => p.VlrUnitario);
            Map(p => p.Observacao);
            Map(p => p.FlgTipo);
            Map(p => p.FlgAtivo);
            Map(p => p.CodBarra);
            Map(p => p.AliquotaIcms);
            Map(p => p.CodStSintegra);
            Map(p => p.FlgIndicadorProp);
            Map(p => p.FlgRegApuPisCof);
            Map(p => p.FlgRegra);
            Map(p => p.CodStIpiSaida);
            Map(p => p.CodItemRed);
            Map(p => p.CodMenorUnidade);
            Map(p => p.FlgProdComb);
            Map(p => p.CProdAnp);
            Map(p => p.Cest);
            Map(p => p.FlgSincronizaDfe);
            Map(p => p.IdCNae);
            Map(p => p.FlgSincParcialDfe);
            Map(p => p.CodEan);
            Map(p => p.CodeXTipi);
            Map(p => p.IdGtin);
            Map(p => p.IdThex);
            Map(p => p.CodExTipoServ);

            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("LfItem");
        }
    }
}
