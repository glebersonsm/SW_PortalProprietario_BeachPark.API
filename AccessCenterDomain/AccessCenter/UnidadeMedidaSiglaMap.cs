using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class UnidadeMedidaSiglaMap : ClassMap<UnidadeMedidaSigla>
    {
        public UnidadeMedidaSiglaMap()
        {
            Id(x => x.Id).GeneratedBy.Sequence("UNIDADEMEDIDASIGLA_SEQUENCE");
            Map(b => b.Tag);
            Map(b => b.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.UsuarioAlteracao);
            Map(b => b.DataHoraAlteracao);

            Map(b => b.GrupoEmpresa);
            Map(b => b.Sigla);
            Map(b => b.Padrao);
            References(b => b.UnidadeMedida, "UnidadeMedida");

            Table("UnidadeMedidaSigla");
        }
    }
}
