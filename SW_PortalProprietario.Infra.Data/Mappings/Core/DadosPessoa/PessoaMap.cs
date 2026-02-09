using FluentNHibernate.Mapping;
using NHibernate.Type;
using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.DadosPessoa
{
    public class PessoaMap : ClassMap<Pessoa>
    {
        public PessoaMap()
        {
            Id(x => x.Id).GeneratedBy.Native("Pessoa_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            Map(b => b.Nome).Length(200);
            Map(b => b.NomeFantasia).Length(200);
            Map(b => b.EmailPreferencial).Length(200);
            Map(b => b.EmailAlternativo).Length(200);
            Map(b => b.DataNascimento);
            Map(b => b.DataAbertura);
            Map(b => b.DataEncerramento);
            Map(b => b.TipoPessoa).CustomType<EnumType<EnumTipoPessoa>>();
            Map(b => b.RegimeTributario).CustomType<EnumType<EnumTipoTributacao>>();

            Table("Pessoa");
        }
    }
}
