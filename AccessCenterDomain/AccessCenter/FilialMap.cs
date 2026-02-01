using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class FilialMap : ClassMap<Filial>
    {
        public FilialMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FILIAL_SEQUENCE");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);


            Map(b => b.Empresa);
            Map(b => b.Pessoa);
            Map(b => b.NomeAbreviado);
            Map(p => p.Codigo);
            Map(p => p.UtilizaIpi);
            Map(p => p.InformaRegistroIpiZeradoSped);
            Map(p => p.UtilizaSubstituicaoTributaria);
            Map(p => p.TipoFilial);
            Map(p => p.TipoComercio);
            Map(p => p.Sigla);
            Map(p => p.UtilizaEcf);
            Map(p => p.Status);

            Table("Filial");
        }
    }
}
