using FluentNHibernate.Mapping;

namespace AccessCenterDomain.AccessCenter
{
    public class PessoaMap : ClassMap<Pessoa>
    {
        public PessoaMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("PESSOA_SEQUENCE");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.Tipo);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(b => b.NomeFantasia);
            Map(b => b.NomeFantasiaPesquisa);
            Map(b => b.NomeExibicao);
            Map(b => b.Cnpj);
            Map(b => b.InscricaoEstadual);
            Map(b => b.InscricaoMunicipal);
            Map(b => b.InscricaoSubstituto);
            Map(b => b.Suframa);
            Map(b => b.Filial);
            Map(b => b.EstadoCivil);
            Map(b => b.RegimeCasamento);
            Map(b => b.CPF);
            Map(b => b.RGOrgaoExpedidor);
            Map(b => b.RGEstado);
            Map(b => b.Sexo);
            Map(b => b.Nascimento);
            Map(b => b.Estrangeiro);
            Map(b => b.Nacionalidade);
            Map(b => b.Visivel);
            Map(b => b.eMail);
            Map(b => b.PessoaEnderecoPreferencial);
            Map(b => b.PessoaEnderecoCobranca);
            Map(b => b.PessoaTelefonePreferencial);
            Map(b => b.PessoaProfissaoPrincipal);
            Map(b => b.ConsumidorFinal);
            Map(b => b.Segmento);
            Map(b => b.RegimeTributacao);
            Map(b => b.RecebeSMS);
            Map(b => b.Passaporte);

            Table("Pessoa");
        }
    }
}
