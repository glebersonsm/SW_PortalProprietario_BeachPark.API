using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class UsuarioSistemaMap : ClassMap<UsuarioSistema>
    {
        public UsuarioSistemaMap()
        {
            Id(x => x.IdUsuario).GeneratedBy.Assigned();

            Map(p => p.NomeUsuario);

            Map(b => b.IdEspAcesso);

            Map(b => b.FlgAusente);

            Map(p => p.ValidadeSenha);

            Map(b => b.Descricao);

            Map(b => b.Bloqueado);

            Map(b => b.Desativado);
            Map(b => b.IdTzLocations);
            Map(b => b.SwPasswordHash);
            Map(b => b.Senha);
            Map(b => b.SenhaTransf);
            Map(b => b.MudarSenha);
            Map(b => b.SenhaPermanente);
            Map(b => b.NaoMudaSenha);

            Map(b => b.TrgDtInclusao);
            Map(b => b.TrgUserInclusao);

            Table("UsuarioSistema");
            Schema("cm");
        }
    }
}
