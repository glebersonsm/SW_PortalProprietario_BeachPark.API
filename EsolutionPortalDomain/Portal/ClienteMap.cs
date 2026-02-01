using FluentNHibernate.Mapping;


namespace EsolutionPortalDomain.Portal
{
    public class ClienteMap : ClassMap<Cliente>
    {
        public ClienteMap()
        {
            Id(x => x.Id)
            .GeneratedBy.SequenceIdentity();

            Map(p => p.Situacao);
            Map(b => b.DataHoraCadastro);
            Map(b => b.DataHoraModificacao);
            Map(b => b.PossuiLimiteCredito);
            Map(b => b.ValorCredito);
            Map(b => b.PossuiDescontoEspecial);
            Map(b => b.DescontoEspecial);
            Map(b => b.Pessoa);
            Map(b => b.Empresa);
            Map(b => b.IntegracaoStatus);
            Map(b => b.IntegracaoTotalTentativa);

            Table("Cliente");
        }
    }
}
