namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class CidadeImportacaoModel
    {
        public int? id { get; set; }
        public string? nome { get; set; }
        public MunicipioModel? municipio { get; set; }

    }

    public class MunicipioModel
    {
        public int? id { get; set; }
        public string? nome { get; set; }
        public MicroRregiao? microrregiao { get; set; }

    }

    public class MicroRregiao
    {
        public int? id { get; set; }
        public string? nome { get; set; }
        public MesoRregiao? mesorregiao { get; set; }

    }

    public class MesoRregiao
    {
        public int? id { get; set; }
        public string? nome { get; set; }
        public EstadoImportacao? uf { get; set; }

    }

    public class EstadoImportacao
    {
        public int? id { get; set; }
        public string? sigla { get; set; }
        public string? nome { get; set; }

    }
}
