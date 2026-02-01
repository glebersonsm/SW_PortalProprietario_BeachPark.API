namespace CMDomain.Models.AuthModels
{
    public class ModuloModel
    {
        public int? ModuloId { get; set; }
        public string? ModuloNome { get; set; }

        public List<ModuloDireitoTratadoModel>? Direitos { get; set; }
    }
}
