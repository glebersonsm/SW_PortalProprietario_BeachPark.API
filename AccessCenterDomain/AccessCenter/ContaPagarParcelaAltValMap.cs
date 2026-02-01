using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class ContaPagarParcelaAltValMap : ClassMap<ContaPagarParcelaAltVal>
    {
        public ContaPagarParcelaAltValMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("CONTAPAGARPARCELAALTVAL_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.ContaPagarParcela);
            Map(b => b.AlteradorValor);
            Map(b => b.Data);
            Map(b => b.Valor);
            Map(b => b.DataOriginal);
            Map(b => b.DataProvisao);
            Map(b => b.DataProvisaoOriginal);
            Map(b => b.Estornado);
            Map(b => b.DataEstorno);
            Map(b => b.ItemEstornado);
            Map(b => b.LancamentoEstorno);
            Map(b => b.ValorIntegralizado);
            Map(b => b.Observacao);
            Map(b => b.TipoContaPagar);
            Map(b => b.FilialDestino);
            Map(b => b.CentroCustoDestino);
            Map(b => b.Contabilizar);

            Table("ContaPagarParcelaAltVal");
        }
    }
}
