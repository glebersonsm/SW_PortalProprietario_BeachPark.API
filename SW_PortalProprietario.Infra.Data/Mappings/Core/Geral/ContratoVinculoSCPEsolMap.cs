using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Geral;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Geral
{
    public class ContratoVinculoSCPEsolMap : ClassMap<ContratoVinculoSCPEsol>
    {
        public ContratoVinculoSCPEsolMap()
        {
            Id(x => x.Id).GeneratedBy.Native("ContratoVinculoSCPEso_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            Map(b => b.PessoaLegadoId);
            Map(b => b.CotaPortalId);
            Map(b => b.CotaAccessCenterId);
            Map(b => b.UhCondominioId);
            Map(b => b.CodigoVerificacao).Length(100);
            Map(b => b.PdfPath).Length(2000);
            Map(b => b.DadosQualificacaoCliente).CustomType("StringClob").CustomSqlType("Text");
            Map(b => b.DocumentoFull).CustomType("StringClob").CustomSqlType("Text");
            References(p => p.Empresa, "Empresa");
            Map(b => b.Idioma);

            Schema("portalohana");
            Table("ContratoVinculoSCPEsol");
        }

    }
}
