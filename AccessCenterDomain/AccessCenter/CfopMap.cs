using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class CfopMap : ClassMap<Cfop>
    {
        public CfopMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("CFOP_SEQUENCE");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.Codigo);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(b => b.AtivoImobilizado);
            Map(b => b.LancaFinanceiro);
            Map(b => b.Fomentar);
            Map(b => b.LancaValorZeradoLivroIcms);
            Map(b => b.EntraCalculoPercentualIseTri);

            Table("Cfop");
        }
    }
}
