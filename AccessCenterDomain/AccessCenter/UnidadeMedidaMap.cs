using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class UnidadeMedidaMap : ClassMap<UnidadeMedida>
    {
        public UnidadeMedidaMap()
        {
            Id(x => x.Id).GeneratedBy.Sequence("UNIDADEMEDIDA_SEQUENCE");
            Map(b => b.Tag);
            Map(b => b.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.UsuarioAlteracao);
            Map(b => b.DataHoraAlteracao);

            Map(b => b.GrupoEmpresa);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(b => b.PermiteFracionamento);
            Map(b => b.UnidadeBase);
            Map(b => b.QuantidadeCasasDecimais);
            References(b => b.SiglaPadrao, "SiglaPadrao");

            Table("UnidadeMedida");
        }
    }
}
