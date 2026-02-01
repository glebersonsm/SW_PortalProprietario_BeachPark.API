using FluentNHibernate.Mapping;


namespace EsolutionPortalDomain.Portal
{
    public class ProprietarioMap : ClassMap<Proprietario>
    {
        public ProprietarioMap()
        {
            Id(x => x.Id)
            .GeneratedBy.SequenceIdentity();

            Map(p => p.Cliente);
            Map(b => b.CotaProprietario);
            Map(b => b.DataHoraInclusao);
            Map(b => b.DataHoraExclusao);
            Map(b => b.Principal);
            Map(b => b.UsuarioExclusao);
            Map(b => b.DataTransferencia);
            Map(b => b.NovoProprietario);
            Map(b => b.NumeroContrato);
            Map(b => b.NomeProduto);

            Table("Proprietario");
        }
    }
}
