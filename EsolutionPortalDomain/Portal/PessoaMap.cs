using FluentNHibernate.Mapping;


namespace EsolutionPortalDomain.Portal
{
    public class PessoaMap : ClassMap<Pessoa>
    {
        public PessoaMap()
        {
            Id(x => x.Id)
            .GeneratedBy.SequenceIdentity();

            Map(p => p.Tipo);
            Map(b => b.DataHoraCadastro);
            Map(b => b.DataHoraModificacao);
            Map(b => b.UsuarioCadastro);
            Map(b => b.UsuarioModificacao);
            Map(b => b.Nome);
            Map(b => b.NomeFantasia);
            Map(b => b.EstadoCivil);
            Map(b => b.Sexo);
            Map(b => b.Nascimento);
            Map(b => b.RG);
            Map(b => b.CPF);
            Map(b => b.eMail);
            Map(b => b.Renda);
            Map(b => b.Estrangeiro);
            Map(b => b.IntegracaoStatus);
            Map(b => b.IntegracaoTotalTentativas);

            Table("Pessoa");
        }
    }
}
