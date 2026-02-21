using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class MovimentMap : ClassMap<Moviment>
    {
        public MovimentMap()
        {
            Id(x => x.IdMov).GeneratedBy.Assigned();

            References(p => p.TipoMov, "CodTipoMov");
            Map(p => p.CodArtigo);
            Map(p => p.IdHotel);
            Map(p => p.IdPessoaTransf);
            Map(p => p.IdPessoa);
            Map(p => p.Plano);
            Map(p => p.Placonta);
            Map(p => p.UnidNegoc);
            Map(p => p.PlnCodigo);
            Map(p => p.CodCentroCusto);
            Map(p => p.CodAlmoxarifado);
            Map(p => p.DataMov);
            Map(p => p.QtdeMov);
            Map(p => p.ValorMov);
            Map(p => p.DataLancMov);
            Map(p => p.CustoMedioMov);
            Map(p => p.SaldoQtdeMov);
            Map(p => p.NumDocumento);
            Map(p => p.CodAlmoxTransf);
            Map(p => p.IdMovEntrada);
            Map(p => p.FlgEstorno);
            Map(p => p.IdTipoPerda);
            Map(p => p.FlgEntradaCusto);
            Map(p => p.IdLoteArtigo);
            Map(p => p.NumLote);
            Map(p => p.FlgIntegrada);
            Map(p => p.NumRequisicao);
            Map(p => p.PlacontaEntrada);
            Map(p => p.SubContaEntrada);
            Map(p => p.PlanoEntrada);
            Map(p => p.CodFiscal);
            Map(p => p.IdTpFinalidade);
            Map(p => p.IdForCli);
            Map(p => p.QtdeMovEspelho);
            Map(p => p.IdEmpresa);

            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("Moviment");
            Schema("cm");
        }
    }
}
