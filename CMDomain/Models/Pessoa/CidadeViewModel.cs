namespace CMDomain.Models.Pessoa
{
    public class CidadeViewModel
    {
        public int? Id { get; set; }
        public string? CodigoIbge { get; set; }
        public string? Nome { get; set; }
        public string? Uf { get; set; }
        public string? NomeEstado { get; set; }
        public int? IdEstado { get; set; }

    }

    //string strTipoEndereco = "";
    //if (item.Comercial.GetValueOrDefault(false))
    //    strTipoEndereco = "C";
    //if (item.Residencial.GetValueOrDefault(false))
    //    strTipoEndereco += "R";
    //if (item.Entrega.GetValueOrDefault(false))
    //    strTipoEndereco += "E";
    //if (item.Cobranca.GetValueOrDefault(false))
    //    strTipoEndereco += "B";
    //if (item.Correspondencia.GetValueOrDefault(false))
    //    strTipoEndereco += "P";
}
