using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Framework;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Framework
{
    public class EmpresaMap : ClassMap<Empresa>
    {
        public EmpresaMap()
        {
            Id(x => x.Id).GeneratedBy.Native("Empresa_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);

            Map(b => b.DataHoraCriacao);

            Map(b => b.DataHoraAlteracao)
                .Nullable();

            Map(p => p.UsuarioAlteracao)
                .Nullable();

            Map(b => b.Codigo)
                .Length(20).Unique();

            References(p => p.Pessoa, "Pessoa").UniqueKey("UK_EmpresaPessoa");
            References(p => p.GrupoEmpresa, "GrupoEmpresa");

            Map(p => p.NomeCondominio);
            Map(p => p.CnpjCondominio);
            Map(p => p.EnderecoCondominio);

            Map(p => p.NomeAdministradoraCondominio);
            Map(p => p.CnpjAdministradoraCondominio);
            Map(p => p.EnderecoAdministradoraCondominio);

            Table("Empresa");
        }
    }
}
