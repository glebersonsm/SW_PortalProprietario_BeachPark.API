using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class EstoqueAtualMap : ClassMap<EstoqueAtual>
    {
        public EstoqueAtualMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("ESTOQUEATUAL_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.ProdutoItemAlmoxarifado);
            Map(b => b.TipoEstoque);
            Map(p => p.PrecoCusto);
            Map(p => p.QuantidadeSaldo);
            Map(p => p.UnidadeMedida);
            Map(p => p.ValorSaldo);
            Map(p => p.Ordem);

            Table("EstoqueAtual");
        }
    }
}
