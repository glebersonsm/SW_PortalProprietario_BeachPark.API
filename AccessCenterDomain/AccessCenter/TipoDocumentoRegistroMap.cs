using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class TipoDocumentoRegistroMap : ClassMap<TipoDocumentoRegistro>
    {
        public TipoDocumentoRegistroMap()
        {
            Id(x => x.Id).GeneratedBy.Sequence("TIPODOCUMENTOREGISTRO_");
            Map(b => b.Tag);
            Map(b => b.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.UsuarioAlteracao);
            Map(b => b.DataHoraAlteracao);

            Map(b => b.PessoaTipo);
            Map(b => b.Codigo);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(b => b.Mascara);

            Table("TipoDocumentoRegistro");
        }
    }
}
