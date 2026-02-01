using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class ContaReceberMap : ClassMap<ContaReceber>
    {
        public ContaReceberMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("CONTARECEBER_SEQUENCE");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.Filial);
            Map(b => b.Documento);
            Map(b => b.Titulo);
            Map(b => b.Observacao);
            Map(b => b.Emissao);
            Map(b => b.Cliente);
            Map(b => b.ClienteAnterior);
            Map(b => b.Valor);
            Map(b => b.ValorOriginal);
            Map(b => b.PDV);
            Map(b => b.Importacao);
            Map(b => b.OperacaoFinanceira);
            Map(b => b.DataMovimento);
            Map(b => b.Operador);
            Map(b => b.QuantidadeParcelas);
            Map(b => b.GrupoEmpresa);
            Map(b => b.Empresa);
            Map(b => b.IntegracaoId);
            Map(b => b.Cota);

            Table("ContaReceber");
        }
    }
}
