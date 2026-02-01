namespace SW_PortalProprietario.Application.Models
{
    public class DownloadResultModel
    {
        public int? Id { get; set; }
        public string? Result { get; set; }
        public int? Status { get; set; } = 200;
        public List<string> Errors { get; set; } = new List<string>();
    }
}
