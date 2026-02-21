using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class LfDocumentoFiscalMap : ClassMap<LfDocumentoFiscal>
    {
        public LfDocumentoFiscalMap()
        {

            Id(x => x.IdDocumento)
                .GeneratedBy.Assigned();

            Map(p => p.CodSituacao);
            Map(p => p.IdModulo);
            Map(p => p.IdModelo);
            Map(p => p.IdHotel);
            Map(p => p.NumDocumentoIni);
            Map(p => p.NumDocumentoFin);
            Map(p => p.Serie);
            Map(p => p.SubSerie);
            Map(p => p.DataEmissao);
            Map(p => p.DataMovimento);
            Map(p => p.VlrDocumento);
            Map(p => p.FlgTipo);
            Map(p => p.FlgBenFiscal);
            Map(p => p.IdRelativo);
            Map(p => p.Placa);
            Map(p => p.ChaveNfEletronica);
            Map(p => p.CodClasseConsumo);
            Map(p => p.CodTipoLigacao);
            Map(p => p.CodGrupoTensao);
            Map(p => p.NumeroGuia);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("LfDocumentoFiscal");
            Schema("cm");
        }
    }
}
