using FluentNHibernate.Mapping;

namespace AccessCenterDomain.AccessCenter
{
    public class ContaFinanceiraMap : ClassMap<ContaFinanceira>
    {
        public ContaFinanceiraMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("CONTAFINANCEIRA_SEQUENCE");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.Filial);
            Map(b => b.Empresa);
            Map(b => b.GrupoEmpresa);
            Map(b => b.ContaFinanceiraTipo);
            Map(b => b.Status);
            Map(b => b.Codigo);
            Map(b => b.Nome);
            Map(b => b.ContaNumero);
            Map(b => b.ContaDigito);
            Map(b => b.AgenciaNumero);
            Map(b => b.AgenciaDigito);
            Map(b => b.Banco);
            Map(b => b.Cedente);
            Map(b => b.EnderecoCedente);
            Map(b => b.CNPJCedente);

            Table("ContaFinanceira");
        }
    }
}
