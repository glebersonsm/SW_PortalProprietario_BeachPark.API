namespace CMDomain.Models.Fornecedor
{
    public class DocumentoViewModel
    {
        public int? IdDocumento { get; set; }
        public string? TipoDocumento { get; set; }
        public int? IdFornecedor { get; set; }
        public string? NumDocumento { get; set; }
        public string? Orgao { get; set; }
        public DateTime? DataEmissao { get; set; }
        public int? IdEstado { get; set; }

        public override int GetHashCode()
        {
            return IdDocumento.GetHashCode() + IdFornecedor.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            DocumentoViewModel? cc = obj as DocumentoViewModel;
            if (cc is null) return false;
            return cc.Equals(this);
        }

    }
}
