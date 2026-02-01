using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class TipoClienteMap : ClassMap<TipoCliente>
    {
        public TipoClienteMap()
        {
            Id(x => x.Id).GeneratedBy.Sequence("GRUPOCLIENTE_SEQUENCE");
            Map(b => b.Tag);
            Map(b => b.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.UsuarioAlteracao);
            Map(b => b.DataHoraAlteracao);

            Map(b => b.Empresa);
            Map(b => b.GrupoEmpresa);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(b => b.DadosCliente);
            Map(b => b.DadosFornecedor);
            Map(b => b.DadosCobrador);

            Table("TipoCliente");
        }
    }
}
