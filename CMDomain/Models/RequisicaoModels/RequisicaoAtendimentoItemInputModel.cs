using CMDomain.Entities;

namespace CMDomain.Models.RequisicaoModels
{
    public class RequisicaoAtendimentoItemInputModel
    {
        public Int64? NumRequisicao { get; set; }
        public string? CodArtigo { get; set; }
        public string? CodMedida { get; set; }
        public decimal? QtdeAtender { get; set; }
        public string? Obs { get; set; }
        public override int GetHashCode()
        {
            return NumRequisicao.GetHashCode() + CodArtigo.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            ItemPedi? cc = obj as ItemPedi;
            if (cc is null) return false;
            return cc.Equals(this);
        }

    }
}
