using FluentNHibernate.Mapping;

namespace AccessCenterDomain.AccessCenter
{
    public class ProdutoItemAlmoxarifadoMap : ClassMap<ProdutoItemAlmoxarifado>
    {
        public ProdutoItemAlmoxarifadoMap()
        {
            Id(x => x.Id).GeneratedBy.Sequence("PRODUTOITEMALMOXARIFADO_");
            Map(b => b.Tag);
            Map(b => b.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.UsuarioAlteracao);
            Map(b => b.DataHoraAlteracao);

            Map(b => b.Filial);
            References(b => b.ProdutoItem, "ProdutoItem");
            References(b => b.Almoxarifado, "Almoxarifado");
            Map(b => b.QuantidadeMinima);
            Map(b => b.QuantidadeMaxima);
            Map(b => b.DiasReposicao);
            Map(b => b.Status);
            Map(b => b.AjustePrecoVenda);
            Map(b => b.PrecoVenda);
            Map(b => b.PrecoCusto);
            Map(b => b.PercentAcrescPrecoVenda);
            Map(b => b.ControlaPeca);
            Map(b => b.ControlaLote);
            Map(b => b.PermiteMaisDeUmLotePendente);
            Map(b => b.PermiteEstoquePendente);
            Map(b => b.VerificaVariacaoCusto);
            Map(b => b.PercentualMaximoVarCusParCim);
            Map(b => b.PercentualMaximoVarCusParBai);
            Map(b => b.PermitirInformarProdutoEstMin);
            Map(b => b.ControlaEstoque);

            Table("ProdutoItemAlmoxarifado");
        }
    }
}
