using FluentNHibernate.Mapping;
using NHibernate.Type;
using SW_PortalProprietario.Domain.Entities.Core.Certidao;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Certidao
{
    public class CertidaoFinanceiraMap : ClassMap<CertidaoFinanceira>
    {
        public CertidaoFinanceiraMap()
        {
            Id(x => x.Id).GeneratedBy.Native("CertidaoFinanceira_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            References(b => b.Pessoa, "Pessoa");
            Map(b => b.Protocolo);
            Map(b => b.CompetenciaInicial);
            Map(b => b.CompetenciaFinal);
            Map(b => b.ImovelNumero);
            Map(b => b.NumeroFracao);
            Map(b => b.UrlValidacaoProtocolo).Length(3000);
            Map(b => b.PdfPath).Length(3000);
            Map(b => b.Conteudo).CustomType("StringClob").CustomSqlType("Text");
            Map(b => b.Tipo).CustomType<EnumType<EnumCertidaoTipo>>();
            Map(b => b.Competencia).Length(200);
            Map(b => b.MultiProprietario).Length(200);
            Map(b => b.CpfCnpj).Length(30);
            Map(b => b.TorreBlocoNome).Length(200);
            Map(b => b.TorreBlocoNumero).Length(200);
            Map(b => b.CertidaoEmitidaEm).Length(200);
            Map(b => b.NomeCampoCpfCnpj).Length(200);
            Schema("portalohana");
            Table("CertidaoFinanceira");
        }
    }
}
