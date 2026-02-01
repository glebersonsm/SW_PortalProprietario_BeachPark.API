using FluentNHibernate.Mapping;

namespace AccessCenterDomain.AccessCenter
{
    public class ProdutoItemCodigoBarrasMap : ClassMap<ProdutoItemCodigoBarras>
    {
        public ProdutoItemCodigoBarrasMap()
        {
            Id(x => x.Id).GeneratedBy.Sequence("PRODUTOCODIGOBARRAS_");
            Map(b => b.Tag);
            Map(b => b.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.UsuarioAlteracao);
            Map(b => b.DataHoraAlteracao);

            Map(b => b.CodigoBarrasAnterior);
            Map(b => b.CodigoBarrasAtual);
            References(b => b.ProdutoItem, "ProdutoItem");

            Table("ProdutoItemCodigoBarras");
        }
    }
}
