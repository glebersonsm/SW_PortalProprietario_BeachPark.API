using Dapper;

namespace SW_PortalProprietario.Application.Models.Financeiro
{
    public class BrokerModel
    {
        public string? CardCompanyId { get; set; }
        public string? CardCompanyToken { get; set; }
        public string? PixCompanyId { get; set; }
        public string? PixCompanyToken { get; set; }
        public string? ApiCardTokenizeUrl { get; set; }
        public string? ApiPaymentUrl { get; set; }
        public string? ApiPaymentCancelUrl { get; set; }
        public string? ApiPaymentConsultTransactionUrl { get; set; }
        public string? TipoBaixaPixId { get; set; }
        public string? ContaFinanceiraVariacaoPixId { get; set; }
        public string? OperacaoFinanceiraBaixaPix { get; set; }
        public int? ExpirationLinkInMinutes { get; set; }
        public bool? Production { get; set; }

        #region Card config transactions
        public string GetCardCompanyId(int empresaId)
        {
            if (string.IsNullOrEmpty(CardCompanyId))
                throw new ArgumentException($"Broker não configurado corretamente, parâmetro: 'CardCompanyId'");

            foreach (var item in CardCompanyId.Split('|').AsList())
            {
                var arr = item.Split(':');
                if (arr.Length < 2)
                    throw new ArgumentException($"Broker não configurado corretamente, parâmetro: 'CardCompanyId'");
                var arrTestar = arr[0].Split('_');
                if (arrTestar.Length < 2)
                    throw new ArgumentException($"Broker não configurado corretamente, parâmetro: 'CardCompanyId'");

                if (arrTestar[1].Equals($"{empresaId}", StringComparison.InvariantCultureIgnoreCase))
                    return arr[1];
            }
            return "";
        }


        public string GetCardCardCompanyToken(int empresaId)
        {
            if (string.IsNullOrEmpty(CardCompanyToken))
                throw new ArgumentException($"Broker não configurado corretamente, parâmetro: 'CardCompanyToken'");

            foreach (var item in CardCompanyToken.Split('|').AsList())
            {
                var arr = item.Split(':');
                if (arr.Length < 2)
                    throw new ArgumentException($"Broker não configurado corretamente, parâmetro: 'CardCompanyToken'");
                var arrTestar = arr[0].Split('_');
                if (arrTestar.Length < 2)
                    throw new ArgumentException($"Broker não configurado corretamente, parâmetro: 'CardCompanyToken'");

                if (arrTestar[1].Equals($"{empresaId}", StringComparison.InvariantCultureIgnoreCase))
                    return arr[1];
            }
            return "";
        }
        #endregion

        #region Pix finalization
        public string GetTipoBaixaPix(int empresaId)
        {
            if (string.IsNullOrEmpty(TipoBaixaPixId))
                throw new ArgumentException($"Broker não configurado corretamente, parâmetro: 'TipoBaixaPixId'");

            foreach (var item in TipoBaixaPixId.Split('|').AsList())
            {
                var arr = item.Split(':');
                if (arr.Length < 2)
                    throw new ArgumentException($"Broker não configurado corretamente, parâmetro: 'TipoBaixaPixId'");
                var arrTestar = arr[0].Split('_');
                if (arrTestar.Length < 2)
                    throw new ArgumentException($"Broker não configurado corretamente, parâmetro: 'TipoBaixaPixId'");

                if (arrTestar[1].Equals($"{empresaId}", StringComparison.InvariantCultureIgnoreCase))
                    return arr[1];
            }
            return "";
        }


        public string GetContaFinanceiraVariacaoPixId(int empresaId)
        {
            if (string.IsNullOrEmpty(ContaFinanceiraVariacaoPixId))
                throw new ArgumentException($"Broker não configurado corretamente, parâmetro: 'ContaFinanceiraVariacaoPixId'");

            foreach (var item in ContaFinanceiraVariacaoPixId.Split('|').AsList())
            {
                var arr = item.Split(':');
                if (arr.Length < 2)
                    throw new ArgumentException($"Broker não configurado corretamente, parâmetro: 'ContaFinanceiraVariacaoPixId'");
                var arrTestar = arr[0].Split('_');
                if (arrTestar.Length < 2)
                    throw new ArgumentException($"Broker não configurado corretamente, parâmetro: 'ContaFinanceiraVariacaoPixId'");

                if (arrTestar[1].Equals($"{empresaId}", StringComparison.InvariantCultureIgnoreCase))
                    return arr[1];
            }
            return "";
        }

        public string GetOperacaoFinanceiraPix(int empresaId)
        {
            if (string.IsNullOrEmpty(OperacaoFinanceiraBaixaPix))
                throw new ArgumentException($"Broker não configurado corretamente, parâmetro: 'OperacaoFinanceiraBaixaPix'");

            foreach (var item in OperacaoFinanceiraBaixaPix.Split('|').AsList())
            {
                var arr = item.Split(':');
                if (arr.Length < 2)
                    throw new ArgumentException($"Broker não configurado corretamente, parâmetro: 'OperacaoFinanceiraBaixaPix'");
                var arrTestar = arr[0].Split('_');
                if (arrTestar.Length < 2)
                    throw new ArgumentException($"Broker não configurado corretamente, parâmetro: 'OperacaoFinanceiraBaixaPix'");

                if (arrTestar[1].Equals($"{empresaId}", StringComparison.InvariantCultureIgnoreCase))
                    return arr[1];
            }
            return "";
        }
        #endregion

        #region Pix config transactions
        public string GetPixCompanyId(int empresaId)
        {
            if (string.IsNullOrEmpty(PixCompanyId))
                throw new ArgumentException($"Broker não configurado corretamente, parâmetro: 'PixCompanyId'");

            foreach (var item in PixCompanyId.Split('|').AsList())
            {
                var arr = item.Split(':');
                if (arr.Length < 2)
                    throw new ArgumentException($"Broker não configurado corretamente, parâmetro: 'PixCompanyId'");
                var arrTestar = arr[0].Split('_');
                if (arrTestar.Length < 2)
                    throw new ArgumentException($"Broker não configurado corretamente, parâmetro: 'PixCompanyId'");

                if (arrTestar[1].Equals($"{empresaId}", StringComparison.InvariantCultureIgnoreCase))
                    return arr[1];
            }
            return "";
        }


        public string GetPixCompanyToken(int empresaId)
        {
            if (string.IsNullOrEmpty(PixCompanyToken))
                throw new ArgumentException($"Broker não configurado corretamente, parâmetro: 'PixCompanyToken'");

            foreach (var item in PixCompanyToken.Split('|').AsList())
            {
                var arr = item.Split(':');
                if (arr.Length < 2)
                    throw new ArgumentException($"Broker não configurado corretamente, parâmetro: 'PixCompanyToken'");
                var arrTestar = arr[0].Split('_');
                if (arrTestar.Length < 2)
                    throw new ArgumentException($"Broker não configurado corretamente, parâmetro: 'PixCompanyToken'");

                if (arrTestar[1].Equals($"{empresaId}", StringComparison.InvariantCultureIgnoreCase))
                    return arr[1];
            }
            return "";
        } 
        #endregion

    }
}
