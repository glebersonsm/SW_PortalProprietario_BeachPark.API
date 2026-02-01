namespace AccessCenterDomain.AccessCenter.Model
{
    public class CotaProprietarioModel
    {
        public int Id { get; set; }
        public DateTime? DataHoraCriacao { get; set; }
        public int? UsuarioCriacao { get; set; }
        public DateTime? DataHoraAlteracao { get; set; }
        public int? UsuarioAlteracao { get; set; }
        public string Tag { get; set; } //<Importado=S>|<PORTAL=>
        public int? Cota { get; set; }
        public int? Proprietario { get; set; }
        public int? Pessoa { get; set; }
        public string? NomePessoa { get; set; }
        public string? CpfCnpjPessoa { get; set; }
        public int? Procurador { get; set; }
        public DateTime? DataAquisicao { get; set; }

    }
}
