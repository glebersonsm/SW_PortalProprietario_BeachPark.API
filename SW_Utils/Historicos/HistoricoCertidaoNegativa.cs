using Dapper;
using SW_Utils.Auxiliar;

namespace SW_Utils.Historicos
{
    public class HistoricosCertidoes : IHistoricosCertidoes
    {

        private List<(string key, List<ParameterValueResult>)>? listHistoricos = new List<(string key, List<ParameterValueResult>)>();

        public HistoricosCertidoes()
        {
            listHistoricos.Add(new("certidaopositivadebitos", new List<ParameterValueResult>()
            {
                new ParameterValueResult("competencia","Competência"),
                new ParameterValueResult("proprietario","Proprietário"),
                new ParameterValueResult("numeroapto","Número apartamento"),
                new ParameterValueResult("nomecampotorre","Nome do campo torre"),
                new ParameterValueResult("numerotorre","Número torre bloco"),
                new ParameterValueResult("nomecampocpfcnpj","Nome do campo cpfcnpj"),
                new ParameterValueResult("numerocpfcnpj","Número CPF ou CNPJ"),
                new ParameterValueResult("numerocota","Número da cota"),
                new ParameterValueResult("data","Data"),
                new ParameterValueResult("nomesistema","Nome do sistema"),
                new ParameterValueResult("enderecovalidacaodocumento","Endereço para validação do documento"),
                new ParameterValueResult("numeroprotocolo","Protocolo"),
                new ParameterValueResult("totalcontaspendentes","Total pendências"),
                new ParameterValueResult("enderecovalidacaodocumento","Endereço para validação do documento"),
                new ParameterValueResult("numeroprotocolo","Protocolo"),
                new ParameterValueResult("cnpjadministradora","Cnpj da administradora do condomínio"),
                new ParameterValueResult("nomeadministradora","Nome da administradora do condomínio"),
                new ParameterValueResult("enderecoadministradora","Endereco administradora condomínio"),
                new ParameterValueResult("endereco","Endereço do condomínio")
            }));

            listHistoricos.Add(new("certidaonegativadebitos", new List<ParameterValueResult>()
            {
                new ParameterValueResult("competencia","Competência"),
                new ParameterValueResult("proprietario","Proprietário"),
                new ParameterValueResult("numeroapto","Número apartamento"),
                new ParameterValueResult("nomecampotorre","Nome do campo torre"),
                new ParameterValueResult("numerotorre","Número torre bloco"),
                new ParameterValueResult("nomecampocpfcnpj","Nome do campo cpfcnpj"),
                new ParameterValueResult("numerocpfcnpj","Número CPF ou CNPJ"),
                new ParameterValueResult("numerocota","Número da cota"),
                new ParameterValueResult("data","Data"),
                new ParameterValueResult("nomesistema","Nome do sistema"),
                new ParameterValueResult("enderecovalidacaodocumento","Endereço para validação do documento"),
                new ParameterValueResult("numeroprotocolo","Protocolo"),
                new ParameterValueResult("cnpjadministradora","Cnpj da administradora do condomínio"),
                new ParameterValueResult("nomeadministradora","Nome da administradora do condomínio"),
                new ParameterValueResult("enderecoadministradora","Endereco administradora condomínio"),
                new ParameterValueResult("endereco","Endereço do condomínio")
            }));
        }

        public List<ParameterValueResult>? GetHistoricos(string nomeTipoHistorico)
        {
            if (nomeTipoHistorico.StartsWith("certidao"))
            {
                if (!new List<string>() { "certidaonegativadebitos", "certidaopositivadebitos" }.Any(a => a.Equals(nomeTipoHistorico, StringComparison.InvariantCultureIgnoreCase)))
                    throw new ArgumentException($"As funções possíveis par preenchimento de históricos para certidões podem ser somente ('certidaopositivadebitos','certidaonegativadebitos'), não foi encontrada a funão informada: '{nomeTipoHistorico}'");
            }

            var itens = listHistoricos?.FirstOrDefault(a => a.key.StartsWith(nomeTipoHistorico, StringComparison.OrdinalIgnoreCase));
            if (itens.HasValue && itens.Value.Item2 != null && itens.Value.Item2.Any())
                return itens.Value.Item2.AsList();

            return default;
        }

    }
}
