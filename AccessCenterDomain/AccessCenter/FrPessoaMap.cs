using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;

namespace AccessCenterDomain.AccessCenter
{
    public class FrPessoaMap : ClassMap<FrPessoa>
    {
        public FrPessoaMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FRPESSOA_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.Filial);
            Map(b => b.Pessoa);
            Map(b => b.TempoCasamento);
            Map(b => b.QuantidadeDependentes);
            Map(b => b.QuantidadeFilhos);
            Map(b => b.IdadeFilhos);
            Map(b => b.PossuiCartaoCredito);
            Map(b => b.PossuiCarro);
            Map(b => b.ResidenciaPropria);
            Map(b => b.UtilizouOcr);
            Map(b => b.Renda);

            Table("FrPessoa");
        }
    }
}
