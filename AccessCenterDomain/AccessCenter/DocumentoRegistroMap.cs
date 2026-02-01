using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class DocumentoRegistroMap : ClassMap<DocumentoRegistro>
    {
        public DocumentoRegistroMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("TIPODOCUMENTOREGISTRO_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.Pessoa);
            Map(p => p.TipoDocumentoRegistro);
            Map(p => p.DocumentoAlfanumerico);
            Map(p => p.DocumentoNumerico);
            Map(p => p.Principal);
            Map(p => p.Tipo);

            Table("DocumentoRegistro");
        }
    }
}
