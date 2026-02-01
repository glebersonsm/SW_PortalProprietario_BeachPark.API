using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class PDVMap : ClassMap<PDV>
    {
        public PDVMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("PDV_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.Filial);
            Map(b => b.Empresa);
            Map(b => b.GrupoEmpresa);
            Map(p => p.Codigo);
            Map(b => b.Nome);
            Map(p => p.NomePesquisa);
            Map(p => p.Caixa);
            Map(p => p.Loja);
            Map(p => p.CentroCusto);
            Map(p => p.CaixaPrincipal);
            Map(p => p.CaixaPrincipalSubVariacao);

            Table("PDV");
        }
    }
}
