using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class CategoriaCotaMap : ClassMap<CategoriaCota>
    {
        public CategoriaCotaMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("CATEGORIACOTA_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.Empresa);
            Map(b => b.Codigo);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(b => b.TipoContaReceber);
            Map(b => b.GrupoEmpresa);
            Map(b => b.Pool);
            Map(b => b.CategoriaForaPool);

            Table("CategoriaCota");
        }
    }
}
