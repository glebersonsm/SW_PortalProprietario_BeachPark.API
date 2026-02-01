using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class FrProdutoMap : ClassMap<FrProduto>
    {
        public FrProdutoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FRPRODUTO_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.Codigo);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(b => b.Filial);
            Map(b => b.Empreendimento);
            Map(b => b.TipoCota);
            Map(b => b.TipoImovel);
            Map(b => b.ImovelVista);
            Map(b => b.Tipo);
            Map(b => b.FrTipoProduto);
            Map(b => b.TipoValor);
            Map(b => b.ValorFixo);
            Map(b => b.ValorBaseComissao);
            Map(b => b.InformaCodigoContrato);
            Map(b => b.NumeroContrato);
            Map(b => b.Sequencia);
            Map(b => b.QuantidadeDigitoSequencia);
            Map(b => b.QuantidadePontos);
            Map(b => b.QuantideDiasCanResNaoPagTax);
            Map(b => b.TempoUtilizacao);
            Map(b => b.PercentualIntegralizacaoUti);
            Map(b => b.UtilizaTarifarioPontos);
            Map(b => b.FrTarifarioPonto);
            Map(b => b.FormatoDataEntrega);
            Map(b => b.GrupoEmpresa);
            Map(b => b.Empresa);
            Map(b => b.TipoReajusteContrato);
            Map(b => b.SwVinculosTse);
            Map(b => b.ProdutoConfiguracaoFinanceira);

            Table("FrProduto");
        }
    }
}
