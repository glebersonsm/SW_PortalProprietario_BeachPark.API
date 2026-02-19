namespace SW_PortalProprietario.Application.Models
{
    public class SearchGrupoFaqModel : SearchFaqModel
    {
        public bool? RetornarFaqs { get; set; } = false;
        public int? IdGrupoFaqPai { get; set; }
    }
}
