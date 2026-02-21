using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class PeriodoMap : ClassMap<Periodo>
    {
        public PeriodoMap()
        {
            CompositeId()
                .KeyProperty(x => x.IdPessoa)
                .KeyProperty(x => x.PerExercicio)
                .KeyProperty(x => x.PerNumero);

            Map(p => p.PerDatIni);
            Map(p => p.PerDatFim);
            Map(p => p.PerNome);
            Map(p => p.PerBloque);
            Map(p => p.PerBloInt);
            Map(p => p.PerAtuali);
            Map(p => p.IdUsuarioInclusao);
            Map(p => p.PerOutMoeda);
            Map(p => p.PerEspecial);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("Periodo");
            Schema("cm");
        }
    }
}
