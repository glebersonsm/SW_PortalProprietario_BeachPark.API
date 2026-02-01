using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class TipoParcelaMap : ClassMap<TipoParcela>
    {
        public TipoParcelaMap()
        {
            Id(x => x.Id).GeneratedBy.Sequence("TIPOPARCELA_");
            Map(b => b.Tag);
            Map(b => b.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.UsuarioAlteracao);
            Map(b => b.DataHoraAlteracao);

            Map(b => b.GrupoEmpresa);
            Map(b => b.Nome);
            Map(b => b.Codigo);
            Map(b => b.NomePesquisa);
            Map(b => b.Categoria);
            Map(b => b.ProcessaReajuste);

            Table("TipoParcela");
        }
    }
}
