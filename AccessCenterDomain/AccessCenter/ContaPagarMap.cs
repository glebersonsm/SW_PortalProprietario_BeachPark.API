using FluentNHibernate.Mapping;

namespace AccessCenterDomain.AccessCenter
{
    public class ContaPagarMap : ClassMap<ContaPagar>
    {
        public ContaPagarMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("CONTAPAGAR_SEQUENCE");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.Filial);
            Map(b => b.Documento);
            Map(b => b.Observacao);
            Map(b => b.Emissao);
            Map(b => b.Cliente);
            Map(b => b.Valor);
            Map(b => b.ValorOriginal);
            Map(b => b.OperacaoFinanceira);
            Map(b => b.DataMovimento);
            Map(b => b.EmprestimoReceber);
            Map(b => b.Contrato);
            Map(b => b.OrdemPagamentoAgenciaDigito);
            Map(b => b.ContaPagarOrigem);

            Table("ContaPagar");
        }
    }
}
