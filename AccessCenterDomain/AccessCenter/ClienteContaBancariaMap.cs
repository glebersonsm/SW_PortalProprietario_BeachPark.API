using FluentNHibernate.Mapping;

namespace AccessCenterDomain.AccessCenter
{
    public class ClienteContaBancariaMap : ClassMap<ClienteContaBancaria>
    {
        public ClienteContaBancariaMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("CLIENTECONTABANCARIA_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);


            Map(b => b.Cliente);
            Map(b => b.Banco);
            Map(b => b.Agencia);
            Map(b => b.AgenciaDigito);
            Map(p => p.Conta);
            Map(p => p.ContaDigito);
            Map(p => p.Variacao);
            Map(p => p.TipoConta);
            Map(p => p.Status);
            Map(p => p.InformarFavorecido);
            Map(p => p.Preferencial);
            Map(p => p.Cidade);
            Map(p => p.TipoChavePix);
            Map(p => p.ChavePix);
            Map(p => p.Tipo);
            Map(p => p.InformaPix);

            Table("ClienteContaBancaria");
        }
    }
}
