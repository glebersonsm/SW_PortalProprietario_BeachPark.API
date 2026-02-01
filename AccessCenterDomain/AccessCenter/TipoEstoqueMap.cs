using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class TipoEstoqueMap : ClassMap<TipoEstoque>
    {
        public TipoEstoqueMap()
        {
            Id(x => x.Id).GeneratedBy.Sequence("TIPOESTOQUE_SEQUENCE");
            Map(b => b.Tag);
            Map(b => b.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.UsuarioAlteracao);
            Map(b => b.DataHoraAlteracao);

            Map(b => b.Empresa);
            Map(b => b.Codigo);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(b => b.Principal);

            Table("TipoEstoque");
        }
    }
}
