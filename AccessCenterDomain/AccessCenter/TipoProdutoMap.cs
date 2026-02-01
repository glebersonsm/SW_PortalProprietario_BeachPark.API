using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class TipoProdutoMap : ClassMap<TipoProduto>
    {
        public TipoProdutoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("TIPOPRODUTO_SEQUENCE");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);


            Map(b => b.GrupoEmpresa);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(p => p.Codigo);
            Map(p => p.TipoProdutoTipo);
            Map(p => p.ExigeOrdemCompra);
            Map(p => p.CentroCusto);
            Map(p => p.PermiteReterInss);
            Map(p => p.EnviaEstoqueSped);

            Table("TipoProduto");
        }
    }
}
