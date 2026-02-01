using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Geral;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Geral
{
    public class UsuarioMultiPropTempMap : ClassMap<UsuarioMultiPropTemp>
    {
        public UsuarioMultiPropTempMap()
        {
            Id(x => x.Id).GeneratedBy.Native("UsuarioMultiPropTemp_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            Map(b => b.Nome).Length(100);
            Map(b => b.CpfCnpj).Length(100);
            Map(b => b.Email).Length(100);
            Map(b => b.IdContrato).Length(100);
            Map(b => b.NumeroContrato).Length(100);
            Map(b => b.Administrador).Length(1);


            Table("UsuarioMultiPropTemp");
        }

    }
}
