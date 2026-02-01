using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class GrupoAcessoMap : ClassMap<GrupoAcesso>
    {
        public GrupoAcessoMap()
        {
            Id(x => x.IdGrupo).GeneratedBy.Sequence("SEQGRUPOACESSO");

            Map(p => p.NomeGrupo);
            Map(p => p.IdEspAcesso);
            Map(p => p.Descricao);
            Map(p => p.DiasAltSenha);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("GrupoAcesso");
        }
    }
}
