using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.DadosPessoa
{
    public class PessoaDocumentoMap : ClassMap<PessoaDocumento>
    {
        public PessoaDocumentoMap()
        {
            Id(x => x.Id).GeneratedBy.Native("PessoaDocumento_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            Map(b => b.Numero);
            Map(b => b.NumeroFormatado);
            Map(b => b.ValorNumerico);
            Map(b => b.OrgaoEmissor);
            Map(b => b.DataEmissao);
            Map(b => b.DataValidade);
            References(b => b.Pessoa, "Pessoa");
            References(b => b.TipoDocumento, "TipoDocumento");

            Table("PessoaDocumento");
        }
    }
}
