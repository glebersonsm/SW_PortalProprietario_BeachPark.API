using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Geral
{
    public class ConfirmacaoLiberacaoPoolMap : ClassMap<ConfirmacaoLiberacaoPool>
    {
        public ConfirmacaoLiberacaoPoolMap()
        {
            Id(x => x.Id).GeneratedBy.Native("ConfirmacaoLiberacaoPool_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            References(p => p.Email, "Email");
            References(p => p.Empresa, "Empresa");
            Map(b => b.CodigoEnviadoAoCliente);
            Map(b => b.LiberacaoConfirmada).CustomType<EnumSimNao>();
            Map(b => b.DataConfirmacao);
            Map(b => b.LiberacaoDiretaPeloCliente);
            Map(b => b.AgendamentoId);
            Map(b => b.NovoAgendamentoId);
            Map(b => b.Tentativas).Length(2000);
            Map(b => b.Banco);
            Map(b => b.Conta);
            Map(b => b.ContaDigito);
            Map(b => b.Agencia);
            Map(b => b.AgenciaDigito);
            Map(b => b.ChavePix);
            Map(b => b.Tipo);
            Map(b => b.Variacao);
            Map(b => b.TipoConta);
            Map(b => b.Preferencial);
            Map(b => b.TipoChavePix);
            Map(b => b.IdCidade);

            Schema("portalohana");
            Table("ConfirmacaoLiberacaoPool");
        }
    }
}

