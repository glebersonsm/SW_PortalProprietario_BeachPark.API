using FluentNHibernate.Mapping;

namespace AccessCenterDomain.AccessCenter
{
    public class BancoMap : ClassMap<Banco>
    {
        public BancoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("BANCO_SEQUENCE");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.Codigo);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);

            Table("Banco");
        }
    }
}
