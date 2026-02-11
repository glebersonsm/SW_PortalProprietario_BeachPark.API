using FluentNHibernate.Mapping;
using NHibernate.Type;
using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.DadosPessoa
{
    public class TipoDocumentoPessoaMap : ClassMap<TipoDocumentoPessoa>
    {
        public TipoDocumentoPessoaMap()
        {
            Id(x => x.Id).GeneratedBy.Native("TipoDocumentoPessoa_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            Map(b => b.Nome);
            Map(b => b.Mascara);
            Map(b => b.ExigeOrgaoEmissor).CustomType<EnumType<EnumSimNao>>();
            Map(b => b.ExigeDataEmissao).CustomType<EnumType<EnumSimNao>>();
            Map(b => b.ExigeDataValidade).CustomType<EnumType<EnumSimNao>>();
            Map(b => b.TipoPessoa).CustomType<EnumType<EnumTiposPessoa>>();
            Schema("portalohana");
            Table("TipoDocumentoPessoa");
        }
    }
}
