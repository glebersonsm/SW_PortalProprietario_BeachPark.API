using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SW_PortalProprietario.Domain.Functions
{
    public static class Helper
    {
        public static string RemoveAccentsFromDomain(this string text, List<string>? itensRemover = null)
        {
            if (string.IsNullOrEmpty(text)) text = "";

            StringBuilder sbReturn = new();
            var arrayText = text.Normalize(NormalizationForm.FormD).ToCharArray();
            foreach (char letter in arrayText)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(letter) != UnicodeCategory.NonSpacingMark)
                    sbReturn.Append(letter);
            }

            var strReturn = sbReturn.ToString();
            if (itensRemover != null && itensRemover.Any())
            {
                foreach (var item in itensRemover)
                {
                    strReturn = strReturn.Replace($" {item}", "").Replace(item, "");
                }
            }

            return strReturn.TrimStart().TrimEnd();
        }

        public static IEnumerable<IEnumerable<T>> Sublists<T>(this IEnumerable<T> source, int chunksize)
        {
            if (chunksize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(chunksize), "Tamanho da sublista deve ser maior que zero");
            }
            while (source.Any())
            {
                yield return source.Take(chunksize);
                source = source.Skip(chunksize);
            }
        }

        public static string AddTag(string tagName, string tagValue, string fullTagNow = "")
        {
            string backFullTag = fullTagNow;

            if (string.IsNullOrEmpty(fullTagNow) ||
                                fullTagNow.IndexOf($"<{tagName}={tagValue}>", StringComparison.CurrentCultureIgnoreCase) <= -1)
            {
                if (string.IsNullOrEmpty(fullTagNow))
                    fullTagNow = $"<{tagName}={tagValue}>";
                else fullTagNow += $"|<{tagName}={tagValue}>";
            }

            if (!string.IsNullOrEmpty(fullTagNow))
            {
                if (fullTagNow.Split('<').Length != fullTagNow.Split('>').Length)
                {
                    throw new Exception($"A tag original: {backFullTag} pós adição da nova tag: {tagName}={tagName} ficou em um formato incorreto: {fullTagNow}");
                }

                if (fullTagNow.Contains('|'))
                {
                    var conteudos = fullTagNow.Split("|").ToList();
                    if (conteudos.Any(a => a.Split("=").Length != 2) || conteudos.Any(a => a.Split("=")[0].Trim().Length < 1))
                    {
                        throw new Exception($"A tag final: {fullTagNow} não está no formato correto: '<key=value>|<key=value>...");
                    }
                }
                else
                {
                    if (fullTagNow.Split("=").Length != 2 || fullTagNow.Split("=")[0].Trim().Length < 1)
                        throw new Exception($"A tag final: {fullTagNow} não está no formato correto: '<key=value>|<key=value>...");
                }
            }

            return fullTagNow;
        }

        public static (string type, int length, string suggestion) GetPatternAndSuggestion(List<string> listToAnalyse, string textToAddInTheCode)
        {
            string t = "Numeric";
            if (listToAnalyse == null || !listToAnalyse.Any())
            {
                listToAnalyse = new List<string>() { "000" };
            }

            if (listToAnalyse.Count > 0)
            {
                try
                {
                    var numeric = (listToAnalyse.All(c => Convert.ToInt32(c) >= 0));
                    string maxValue = listToAnalyse.OrderByDescending(c => Convert.ToInt32(c)).First();
                    int length = maxValue.Length;

                    string suggestionByMax = (Convert.ToInt32(maxValue) + 1).ToString().PadLeft(length, '0');
                    string suggestionWithTextToAddInTheCode = textToAddInTheCode.PadLeft(length, '0');
                    if (!listToAnalyse.Any(a => a == suggestionByMax))
                    {
                        return (t, length, suggestionByMax);
                    }

                    return (t, length, suggestionWithTextToAddInTheCode);
                }
                catch
                {
                    string maxValue = listToAnalyse.OrderByDescending(c => c.ToString().Length).First();
                    int length = maxValue.Length;
                    string suggestionWithTextToAddInTheCode = textToAddInTheCode.PadLeft(length, '0');
                    if (!listToAnalyse.Any(c => c == suggestionWithTextToAddInTheCode))
                        return ("Text", length, suggestionWithTextToAddInTheCode);
                    else
                    {
                        int lengthTextToAddInTheCode = textToAddInTheCode.Length;
                        string strCodReturn = "";
                        while (string.IsNullOrEmpty(strCodReturn))
                        {
                            foreach (var item in listToAnalyse)
                            {
                                if (item.Length > lengthTextToAddInTheCode)
                                {
                                    string strBase = item.Substring(lengthTextToAddInTheCode - 1, (item.Length - (lengthTextToAddInTheCode - 2)));
                                    string suggestionWithTextToAdd = $"{textToAddInTheCode}{strBase}";
                                    if (!listToAnalyse.Any(c => c == suggestionWithTextToAdd))
                                    {
                                        strCodReturn = suggestionWithTextToAdd;
                                        break;
                                    }
                                }
                            }

                            if (string.IsNullOrEmpty(strCodReturn))
                                break;
                        }

                        if (!string.IsNullOrEmpty(strCodReturn))
                            return ("Text", length, strCodReturn);
                        else
                        {
                            var suggested = suggestionWithTextToAddInTheCode.PadLeft(length + 1, '0');
                            if (!listToAnalyse.Any(a => a == suggested))
                                return ("Text", length + 1, suggested);
                        }

                        _ = int.TryParse(textToAddInTheCode, out int te);
                        if (te > 0)
                        {
                            var suggested = $"#{te.ToString().PadLeft(length - 1, '0')}";
                            if (!listToAnalyse.Any(a => a == suggested))
                                return ("Text", length, suggested);
                        }
                        return ("Text", length, "");
                    }
                }
            }
            else return (t, 3, textToAddInTheCode.PadLeft(3, '0'));
        }

        public static bool IsNumeric(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            return Int64.TryParse(text, out _);
        }

        public static bool IsCpf(string cpf)
        {
            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            string tempCpf;
            string digito;
            int soma;
            int resto;
            cpf = cpf.Trim();
            cpf = cpf.Replace(".", "").Replace("-", "");
            if (cpf.Length != 11)
                cpf = cpf.PadLeft(11, '0');

            tempCpf = cpf[..9];
            soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];
            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito = resto.ToString();
            tempCpf += digito;
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];
            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito += resto.ToString();
            return cpf.EndsWith(digito);
        }

        public static bool IsCnpj(string cnpj)
        {
            int[] multiplicador1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int soma;
            int resto;
            string digito;
            string tempCnpj;
            cnpj = cnpj.Trim();
            cnpj = cnpj.Replace(".", "").Replace("-", "").Replace("/", "");
            if (cnpj.Length != 14)
                cnpj = cnpj.PadLeft(14, '0');

            tempCnpj = cnpj[..12];
            soma = 0;
            for (int i = 0; i < 12; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];
            resto = (soma % 11);
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito = resto.ToString();
            tempCnpj += digito;
            soma = 0;
            for (int i = 0; i < 13; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];
            resto = (soma % 11);
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito += resto.ToString();
            return cnpj.EndsWith(digito);
        }

        public static bool IsEnum(object valor)
        {
            if (valor is null) return false;
            return valor.GetType().IsEnum;
        }

        public static string RemoverCaracteresEspeciais(string input, string simbolosAdicionais)
        {
            var pattern = new StringBuilder(@"(?i)")
                .Append("[")
                .Append(@"^0-9a-záéíóúàèìòùâêîôûãõç°º\s")
                .Append(simbolosAdicionais)
                .Append("]")
                .ToString();

            var replacement = "";
            var rgx = new Regex(pattern);
            var result = rgx.Replace(input, replacement);

            return result;
        }

        public static string ApenasPosicoesComCaracterePesquisado(string input, char caracterePesquisado)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(Convert.ToString(caracterePesquisado))) return "";

            var stringResult = string.Join("", input.ToCharArray().Where(b => b.ToString() == caracterePesquisado.ToString()).ToList());
            return stringResult;
        }


        public static string ApenasNumeros(string? dados)
        {
            var strRetorno = "";
            if (!string.IsNullOrEmpty(dados))
            {
                foreach (var item in dados.ToCharArray())
                {
                    if (IsNumeric(item.ToString()))
                        strRetorno += item.ToString();
                }
            }

            return strRetorno;
        }


        public static string Formatar(string numero, string mascara)
        {
            string strRetorno = "";

            try
            {
                if (string.IsNullOrEmpty(mascara)) return numero;
                if (string.IsNullOrEmpty(numero)) return numero;

                var numeroFormatar = ApenasNumeros(numero);
                var idexAtual = 0;

                foreach (var item in mascara.ToCharArray())
                {
                    if (item.ToString() == "#")
                    {
                        strRetorno += numeroFormatar.Substring(idexAtual, 1);
                        idexAtual++;
                    }
                    else
                    {
                        strRetorno += item.ToString();

                    }

                }

            }
            catch (Exception err)
            {
                return numero;
            }

            return strRetorno;
        }

        public static Int64 TotalPaginas(int qtdeRegistrosRetornar, Int64 totalRegistros)
        {
            if (qtdeRegistrosRetornar <= 0 || totalRegistros <= 0) return 0;

            Int64 totalPage = totalRegistros / qtdeRegistrosRetornar;
            if (totalPage * qtdeRegistrosRetornar < totalRegistros)
                totalPage = totalPage + 1;
            return totalPage;
        }
    }
}
