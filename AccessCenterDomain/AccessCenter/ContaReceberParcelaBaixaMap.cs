using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class ContaReceberParcelaBaixaMap : ClassMap<ContaReceberParcelaBaixa>
    {
        public ContaReceberParcelaBaixaMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("CONTARECEBERPARBAIXA_SEQ");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.ContaReceberParcela);
            Map(b => b.DataBaixa);
            Map(b => b.Valor);
            Map(b => b.Juro);
            Map(b => b.Multa);
            Map(b => b.Desconto);
            Map(b => b.TaxaCobranca);
            Map(b => b.ValorAmortizado);
            Map(b => b.AgrupamConRecParcBai);
            Map(b => b.TipoContaReceber);

            Table("ContaReceberParcelaBaixa");
        }
    }
}
