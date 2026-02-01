using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class FrComissaoCabecalhoMap : ClassMap<FrComissaoCabecalho>
    {
        public FrComissaoCabecalhoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FRCOMISSAOCABECALHO_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.NomeRegra);
            Map(b => b.FrAtendimento);
            Map(b => b.FrAtendimentoVenda);
            Map(b => b.FrAtendimentoFuncao);
            Map(b => b.Finalizado);
            Map(b => b.Tipo);
            Map(b => b.QuantidadeParcelas);
            Map(b => b.QuantidadeMaximaParcelas);
            Map(b => b.ValorTotalComissao);
            Map(b => b.ValorFimIntegralizacao);
            Map(b => b.ValorInicioIntegralizacao);
            Map(b => b.DiasRemoverDataDinheiro);
            Map(b => b.DiasRemoverDataCredito);
            Map(b => b.DiasRemoverDataDebito);
            Map(b => b.TipoDataDinheiro);
            Map(b => b.TipoDataCredito);
            Map(b => b.TipoDataDebito);

            Table("FrComissaoCabecalho");
        }
    }
}
