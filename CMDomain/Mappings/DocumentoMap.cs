using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class DocumentoMap : ClassMap<Documento>
    {
        public DocumentoMap()
        {
            Id(x => x.CodDocumento)
                .GeneratedBy.Assigned();

            Map(p => p.IdPessoa);
            Map(p => p.IdHotel);
            Map(p => p.Plano);
            Map(p => p.Placonta);
            Map(p => p.IdEmpresa);
            Map(p => p.CodCentroCusto);
            Map(p => p.IdForCli);
            Map(p => p.IdModulo);
            Map(p => p.CodTipDoc);
            Map(p => p.RecPag);
            Map(p => p.NoDocumento);
            Map(p => p.ComplDocumento);
            Map(p => p.DataEmissao);
            Map(p => p.DataVencto);
            Map(p => p.DataProgramada);
            Map(p => p.Status);
            Map(p => p.NumFatura);
            Map(p => p.Operacao);
            Map(p => p.NumSlip);
            Map(p => p.EmisBloq);
            Map(p => p.FlgAprovados);
            Map(p => p.FlgContabTipReceb);
            Map(p => p.FlgContabRecDes);
            Map(p => p.FlgNaoIntegfflex);
            Map(p => p.CodSubConta);
            Map(p => p.IdUsuarioInclusao);
            Map(p => p.FlgOc);
            Map(p => p.Obs);
            Map(p => p.FlgNaoConciliado);
            Map(p => p.CodFiscal);
            Map(p => p.IdModeloNfFlex);
            Map(p => p.CodSituacaoFFlex);
            Map(p => p.ClasConsumoFFlex);
            Map(p => p.CodTpLigacaoFFlex);
            Map(p => p.GrupoTensaoFFlex);
            Map(p => p.CodForma);
            Map(p => p.CodPortForma);
            Map(p => p.EmitiuComprovante);
            Map(p => p.ControleRemessa);
            Map(p => p.DataRemessa);
            Map(p => p.ChavePix);
            Map(p => p.IdCBancaria);
            Map(p => p.IdContaXChave);
            Map(p => p.NumDigCodBarras);
            Map(p => p.NumLeitCodBarras);
            Map(p => p.IdArquivo);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);
            Map(p => p.TrgDtAlteracao);
            Map(p => p.TrgUserAlteracao);

            Table("Documento");
        }
    }
}
