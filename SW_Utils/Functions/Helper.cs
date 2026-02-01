using Dapper;
using SW_Utils.Extensions;
using SW_Utils.Models;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using ZXing;
using ZXing.Rendering;

namespace SW_Utils.Functions
{
    public static class Helper
    {
        public static string RemoveAccents(this string text, List<string>? itensRemover = null)
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

        public static Int64 TotalPaginas(int qtdeRegistrosRetornar, Int64 totalRegistros)
        {
            if (qtdeRegistrosRetornar <= 0 || totalRegistros <= 0) return 0;

            Int64 totalPage = totalRegistros / qtdeRegistrosRetornar;
            if (totalPage * qtdeRegistrosRetornar < totalRegistros)
                totalPage = totalPage + 1;
            return totalPage;
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
            cpf = !string.IsNullOrEmpty(cpf) ? cpf.PadLeft(11, '0') : cpf;


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
            cnpj = !string.IsNullOrEmpty(cnpj) ? cnpj.PadLeft(14, '0') : cnpj;

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

            var stringResult = string.Join("", input.ToCharArray().Where(b => b.ToString() == caracterePesquisado.ToString()).AsList());
            return stringResult;
        }


        public static ObjectCompareResultModel CompareObjects<T>(T original, T updated)
        {
            var objectResultComparisson = new ObjectCompareResultModel();
            if (original == null && updated != null)
                objectResultComparisson.TipoOperacao = "Inclusão";
            else if (original != null && updated == null)
            {
                objectResultComparisson.TipoOperacao = "Remoção";
            }
            else if (original != null && updated != null)
                objectResultComparisson.TipoOperacao = "Alteração";

            List<AlteracaoResultModel> resultComparer = new List<AlteracaoResultModel>();
            if (original == null && updated == null) return objectResultComparisson;

            Type type = typeof(T);
            List<MemberInfo> campos = (original != null ? original.GetType().GetProperties().Cast<MemberInfo>().ToList() : updated.GetType().GetProperties().Cast<MemberInfo>().ToList());

            if (original != null && updated == null)
            {
                var propertyId = campos.FirstOrDefault(a => a.Name.Equals("Id", StringComparison.CurrentCultureIgnoreCase));
                if (propertyId != null)
                {
                    resultComparer.Add(new AlteracaoResultModel()
                    {
                        TipoCampo = $"Remoção do Objeto: {original.GetType().AssemblyQualifiedName} ({propertyId.FieldOrPropertyValueDefault(original)})",
                    });

                    objectResultComparisson.Modificacoes = resultComparer;
                }
            }
            else
            {
                foreach (MemberInfo pi in campos)
                {
                    if (updated != null && original != null)
                    {
                        object updatedValue = pi.FieldOrPropertyValueDefault(updated);
                        object originalValue = pi.FieldOrPropertyValueDefault(original);
                        object fieldOrPropertyType = pi.FieldOrPropertyItemType();

                        if (updatedValue != null && originalValue != null && !originalValue.Equals(updatedValue))
                        {
                            resultComparer.Add(new AlteracaoResultModel()
                            {
                                TipoCampo = $"{fieldOrPropertyType}",
                                NomeCampo = pi.Name,
                                ValorAntes = originalValue,
                                ValorApos = updatedValue
                            });
                        }
                        else if (updatedValue == null && originalValue != null)
                        {
                            resultComparer.Add(new AlteracaoResultModel()
                            {
                                TipoCampo = $"{fieldOrPropertyType}",
                                NomeCampo = pi.Name,
                                ValorAntes = originalValue,
                                ValorApos = null
                            });
                        }
                        else if (updatedValue != null && originalValue == null)
                        {
                            resultComparer.Add(new AlteracaoResultModel()
                            {
                                TipoCampo = $"{fieldOrPropertyType}",
                                NomeCampo = pi.Name,
                                ValorAntes = null,
                                ValorApos = updatedValue
                            });
                        }
                    }
                    else if (original == null && updated != null)
                    {
                        object updatedValue = pi.FieldOrPropertyValueDefault(updated);
                        object originalValue = null;
                        object fieldOrPropertyType = pi.FieldOrPropertyItemType();

                        resultComparer.Add(new AlteracaoResultModel()
                        {
                            TipoCampo = $"{fieldOrPropertyType}",
                            NomeCampo = pi.Name,
                            ValorAntes = null,
                            ValorApos = updatedValue
                        });

                    }
                }
                objectResultComparisson.Modificacoes = resultComparer;
            }

            return objectResultComparisson;
        }

        public static string ApenasNumeros(string? dados)
        {
            var strRetorno = "";
            if (string.IsNullOrEmpty(dados)) return strRetorno;
            strRetorno = Regex.Replace(dados, @"[^\d]", "");

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


        public static string RemoverAcentuacaoCpfCnpj(string cpfCnpj)
        {
            if (string.IsNullOrEmpty(cpfCnpj))
                return "";
            cpfCnpj = cpfCnpj.Replace(".", "");
            cpfCnpj = cpfCnpj.Replace("-", "");
            cpfCnpj = cpfCnpj.Replace("/", "");

            return cpfCnpj;
        }

        public static string FormatarCNPJ(Int64 cnpj)
        {
            if (cnpj <= 0)
                return "";
            string newCnpj = "";
            string tmpCnpj = "";
            tmpCnpj = cnpj.ToString();
            for (int i = tmpCnpj.Length; i < 14; i++)
            {
                tmpCnpj = "0" + tmpCnpj;
            }

            newCnpj += tmpCnpj.Substring(0, 2) + ".";
            newCnpj += tmpCnpj.Substring(2, 3) + ".";
            newCnpj += tmpCnpj.Substring(5, 3) + "/";
            newCnpj += tmpCnpj.Substring(8, 4) + "-";
            newCnpj += tmpCnpj.Substring(12, 2);

            return newCnpj;
        }

        public static string FormatarCPF(Int64 cpf)
        {
            if (cpf <= 0)
                return "";

            string newCpf = "";
            var tmpCpf = cpf.ToString().PadLeft(11, '0');

            newCpf += tmpCpf.Substring(0, 3) + ".";
            newCpf += tmpCpf.Substring(3, 3) + ".";
            newCpf += tmpCpf.Substring(6, 3) + "-";
            newCpf += tmpCpf.Substring(9, 2);
            return newCpf;

        }

        public static bool ValidarCPF(string cpf)
        {
            string valor = RemoverAcentuacaoCpfCnpj(cpf);
            if (string.IsNullOrEmpty(valor))
                return false;

            if (valor.Length != 11)
            {
                if (Convert.ToInt64(cpf) == 0)
                    return false;
                else
                    cpf = cpf.PadLeft(11, '0');
            }

            valor = valor.PadLeft(11, '0');

            bool igual = true;

            for (int i = 1; i < 11 && igual; i++)
            {
                if (valor[i] != valor[0])
                    igual = false;
            }

            if (igual || valor == "12345678909")
                return false;

            int[] numeros = new int[11];

            for (int i = 0; i < 11; i++)
            {
                numeros[i] = int.Parse(valor[i].ToString());
            }

            int soma = 0;

            for (int i = 0; i < 9; i++)
            {
                soma += (10 - i) * numeros[i];
            }

            int resultado = soma % 11;

            if (resultado == 1 || resultado == 0)
            {
                if (numeros[9] != 0)
                    return false;
            }
            else if (numeros[9] != 11 - resultado)
                return false;

            soma = 0;

            for (int i = 0; i < 10; i++)
            {
                soma += (11 - i) * numeros[i];
            }

            resultado = soma % 11;

            if (resultado == 1 || resultado == 0)
            {
                if (numeros[10] != 0)
                    return false;
            }
            else if (numeros[10] != 11 - resultado)
                return false;

            return true;
        }

        public static bool ValidaCnpj(string cnpj)
        {
            if (string.IsNullOrEmpty(cnpj)) return false;

            var iCnpj = Convert.ToInt64(cnpj.ToString().Replace(".", "").Replace("/", "").Replace("-", ""));

            return ValidaCNPJ(iCnpj);
        }
        public static bool ValidaCNPJ(long intCnpj)
        {
            if (intCnpj == 0) return false;
            string cnpj = string.Format("{0:00000000000000}", intCnpj);

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

            tempCnpj = cnpj.Substring(0, 12);
            soma = 0;

            for (int i = 0; i < 12; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];

            resto = (soma % 11);

            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = resto.ToString();
            tempCnpj = tempCnpj + digito;
            soma = 0;

            for (int i = 0; i < 13; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];

            resto = (soma % 11);

            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = digito + resto.ToString();

            return cnpj.EndsWith(digito);
        }

        public static string GerarImagemCodigoDeBarras(string codigoDeBarras, string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            var barcodeWriter = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.CODE_128,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = 800,
                    Height = 100,
                    Margin = 10
                }
            };

            var image = barcodeWriter.Write(codigoDeBarras);
            var bmp = ConvertPixelDataToBitmap(image);
            //var fullPath = Path.Combine(directoryPath, $"{Guid.NewGuid()}.jpeg");

            var base64 = ConvertToBase64(bmp);
            return base64;
        }

        public static string MaskSubstring(string input, int start, int end, char maskChar)
        {
            if (start < 0 || end >= input.Length || start > end)
            {
                throw new ArgumentException("Índices fornecidos são inválidos.");
            }

            // Usando StringBuilder para manipular a string
            var builder = new StringBuilder(input);

            // Substituir os caracteres da posição start até end
            for (int i = start; i <= end; i++)
            {
                builder[i] = maskChar;
            }

            return builder.ToString();
        }

        public static Bitmap ConvertPixelDataToBitmap(PixelData pixelData)
        {
            // Criar Bitmap a partir dos dados dos pixels
            var bitmap = new Bitmap(pixelData.Width, pixelData.Height, PixelFormat.Format32bppArgb);

            // Bloquear os bits do bitmap
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.WriteOnly, bitmap.PixelFormat);

            // Copiar os dados dos pixels para o bitmap
            System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);

            // Desbloquear os bits do bitmap
            bitmap.UnlockBits(bitmapData);

            return bitmap;
        }

        public static string ConvertToBase64(Bitmap bitmap)
        {
            using (var ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Jpeg);
                byte[] imageBytes = ms.ToArray();
                return Convert.ToBase64String(imageBytes);
            }
        }

        public static string CriptografarPadraoEsol(string chave, string dados)
        {
            string strChave = chave + "5kh78#$";
            byte[] b = Encoding.UTF8.GetBytes(dados);
            byte[] pw = Encoding.UTF8.GetBytes(strChave);

            RijndaelManaged rm = new RijndaelManaged();
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(chave, new MD5CryptoServiceProvider().ComputeHash(pw));
            rm.Key = pdb.GetBytes(32);
            rm.IV = pdb.GetBytes(16);
            rm.BlockSize = 128;
            rm.Padding = PaddingMode.PKCS7;

            MemoryStream ms = new MemoryStream();

            CryptoStream cryptStream = new CryptoStream(ms, rm.CreateEncryptor(rm.Key, rm.IV), CryptoStreamMode.Write);
            cryptStream.Write(b, 0, b.Length);
            cryptStream.FlushFinalBlock();

            return Convert.ToBase64String(ms.ToArray());
        }


        public static string DescriptografarPadraoEsol(string chave, string sDados)
        {
            string strChave = chave + "5kh78#$";
            byte[] dados = Convert.FromBase64String(sDados);
            byte[] pw = Encoding.UTF8.GetBytes(strChave);

            RijndaelManaged rm = new RijndaelManaged();
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(chave, new MD5CryptoServiceProvider().ComputeHash(pw));
            rm.Key = pdb.GetBytes(32);
            rm.IV = pdb.GetBytes(16);
            rm.BlockSize = 128;
            rm.Padding = PaddingMode.PKCS7;

            MemoryStream ms = new MemoryStream(dados, 0, dados.Length);

            CryptoStream cryptStream = new CryptoStream(ms, rm.CreateDecryptor(rm.Key, rm.IV), CryptoStreamMode.Read);
            StreamReader sr = new StreamReader(cryptStream);
            return sr.ReadToEnd();
        }


        //public static string ToQueryString(this object obj)
        //{
        //    if (obj == null)
        //        return string.Empty;

        //    var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        //    var queryParams = new List<string>();

        //    foreach (var prop in properties)
        //    {
        //        var value = prop.GetValue(obj);
        //        if (value != null)
        //        {
        //            if (value is DateTime dateTimeValue)
        //            {
        //                // Formatando DateTime para um formato padrão, como ISO 8601
        //                queryParams.Add($"{prop.Name}={HttpUtility.UrlEncode(dateTimeValue.ToString("o"))}");
        //            }
        //            else
        //            {
        //                queryParams.Add($"{prop.Name}={HttpUtility.UrlEncode(value.ToString())}");
        //            }
        //        }
        //    }

        //    return string.Join("&", queryParams);
        //}

   
        public static string ToQueryString(this object obj)
        {
            if (obj == null)
                return string.Empty;

            var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var queryParams = new List<string>();

            foreach (var prop in properties)
            {
                var value = prop.GetValue(obj);
                if (value == null)
                    continue;

                // 1. Tratamento para DateTime
                if (value is DateTime dateTimeValue)
                {
                    // Formatando DateTime para um formato padrão, como ISO 8601 ('o' para round-trip format)
                    queryParams.Add($"{prop.Name}={HttpUtility.UrlEncode(dateTimeValue.ToString("o"))}");
                }
                // 2. Tratamento para coleções (List<string>, array de int, etc.), mas excluindo string
                else if (value is IEnumerable enumerableValue && prop.PropertyType != typeof(string))
                {
                    // Itera sobre cada item da coleção
                    foreach (var item in enumerableValue)
                    {
                        if (item != null)
                        {
                            // Adiciona um par chave=valor para cada item, repetindo o nome da propriedade
                            // O valor é convertido para string e, em seguida, URL-encoded
                            queryParams.Add($"{prop.Name}={HttpUtility.UrlEncode(item.ToString())}");
                        }
                    }
                }
                // 3. Tratamento padrão (todos os outros tipos, incluindo string e tipos primitivos)
                else
                {
                    // Usa ToString() para obter a representação em string do valor e URL-encode
                    queryParams.Add($"{prop.Name}={HttpUtility.UrlEncode(value.ToString())}");
                }
            }

            return string.Join("&", queryParams);
        }

        public static string GenerateRandomCode(int length)
        {
            const string chars = "0245421634987163546584235369842578";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
