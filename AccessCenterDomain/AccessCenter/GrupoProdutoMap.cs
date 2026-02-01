using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class GrupoProdutoMap : ClassMap<GrupoProduto>
    {
        public GrupoProdutoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("GRUPOPRODUTO_SEQUENCE");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);


            Map(b => b.GrupoEmpresa);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(p => p.Codigo);
            Map(p => p.PermiteSubsidio);
            Map(p => p.NaturezaReceitaPis);
            Map(p => p.NaturezaReceitaCofins);

            Table("GrupoProduto");
        }
    }
}
