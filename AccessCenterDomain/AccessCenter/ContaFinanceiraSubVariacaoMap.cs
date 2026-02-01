using FluentNHibernate.Mapping;

namespace AccessCenterDomain.AccessCenter
{
    public class ContaFinanceiraSubVariacaoMap : ClassMap<ContaFinanceiraSubVariacao>
    {
        public ContaFinanceiraSubVariacaoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("CONTAFINANCEIRASUBVARIACAO_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.ContaFinanceiraVariacao);
            Map(b => b.Codigo);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(b => b.ContaContabil);
            Map(b => b.Usuario);
            Map(b => b.PermiteConsulta);
            Map(b => b.PermiteLancamento);

            Table("ContaFinanceiraSubVariacao");
        }
    }
}
