using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class AlteradorValorMap : ClassMap<AlteradorValor>
    {
        public AlteradorValorMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("ALTERADORVALOR_SEQUENCE");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.Empresa);
            Map(b => b.Codigo);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(b => b.AlteradorValorAplicacao);
            Map(b => b.Contabilizar);
            Map(b => b.Categoria);
            Map(b => b.Provisao);
            Map(b => b.ExigeFilial);
            Map(b => b.ExigeCentroCusto);
            Map(b => b.Status);

            Table("AlteradorValor");
        }
    }
}
