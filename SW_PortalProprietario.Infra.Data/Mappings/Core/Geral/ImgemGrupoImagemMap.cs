using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Geral;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Geral
{
    public class ImagemGrupoImagemMap : ClassMap<ImagemGrupoImagem>
    {
        public ImagemGrupoImagemMap()
        {
            Id(x => x.Id).GeneratedBy.Native("ImagemGrupoImagem_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            References(b => b.GrupoImagem, "GrupoImagem");
            Map(b => b.Nome).Length(100);
            Map(b => b.Imagem).CustomType("BinaryBlob");
            Map(b => b.NomeBotao).Length(100).Nullable();
            Map(b => b.LinkBotao).Length(2000).Nullable();
            Map(b => b.DataInicioVigencia).Nullable();
            Map(b => b.DataFimVigencia).Nullable();
            Map(b => b.Ordem).Nullable();
            Table("ImagemGrupoImagem");
        }
    }
}

