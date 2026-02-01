using System.Xml;

namespace SW_PortalProprietario.Application.Functions
{
    public static class FileUtils
    {
        public static string[] GetEstruturaPaths(string path = "")
        {
            var baseDir = string.IsNullOrEmpty(path) ? AppDomain.CurrentDomain.BaseDirectory : path;

            var paths = new List<string>
            {
                Path.Combine(baseDir, "")
            };

            var retorno = paths.Where(x => Directory.Exists(x)).ToArray();

            return retorno;
        }

        public static string[] GetEstruturaFilePath(string fileRelativePath, string initialPath = "")
        {
            var paths = GetEstruturaPaths(initialPath);

            for (int i = 0; i < paths.Length; i++)
            {
                paths[i] = Path.Combine(paths[i], fileRelativePath);
            }

            return paths;
        }

        public static List<T> ImportarEstruturaDeArquivo<T>(string fileRelativePath, string initialPath, Func<XmlElement, T> rule)
        {
            var paths = GetEstruturaFilePath(fileRelativePath, initialPath);

            return ImportarEstruturaDePathArquivos<T>(paths, rule);

        }

        public static List<T> ImportarEstruturaDePathArquivos<T>(string[] paths, Func<XmlElement, T> rule)
        {
            var listReturn = new List<T>();

            foreach (var path in paths)
            {
                if (!File.Exists(path)) continue;
                var xd = new XmlDocument();
                xd.Load(path);

                if (xd.DocumentElement != null)
                {
                    foreach (var x in xd.DocumentElement.ChildNodes)
                    {
                        var xel = x as XmlElement;
                        if (xel == null)
                        {
                            //deve ser um comentario xml, ignorar
                            continue;
                        }
                        T item = rule.Invoke(xel);
                        if (item == null)
                        {
                            continue;
                        }
                        listReturn.Add(item);
                    }
                }
            }

            return listReturn;
        }

        public static string ObterTipoMIMEPorExtensao(string extensao)
        {
            // Mapeie extensões conhecidas para tipos MIME (adapte conforme necessário)
            var mapeamentoExtensaoTipoMIME = new Dictionary<string, string>
            {
                { ".pdf", "application/pdf" },
                { ".doc", "application/msword" },
                { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                { ".txt", "application/text" },
                { ".xls", "application/vnd.ms-excel" },
                { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
                { ".jpg", "image/jpg" },
                { ".jpeg", "image/jpeg" },
                { ".png", "image/png" },
                { ".svg", "image/svg+xml" },
                { ".3gp", "video/3gpp" },
                { ".3gp2", "video/3gpp2" },
                { ".mp4", "video/mp4" },
                { ".7z", "application/x-7z-compressed" },
                { ".rar", "application/x-rar-compressed" },
                { ".rtf", "application/rtf" },
                { ".rtx", "application/richtext" }
            };

            // Tente obter o tipo MIME da extensão
            if (mapeamentoExtensaoTipoMIME.TryGetValue(extensao, out var tipoMIME))
            {
                return tipoMIME;
            }

            // Se a extensão não estiver mapeada, retorne nulo ou uma string vazia
            return null;
        }

        public static string ObterTipoMIMEImagePorExtensao(string extensao)
        {
            // Mapeie extensões conhecidas para tipos MIME (adapte conforme necessário)
            //PG, GIF, PNG, SVG, PSD, WEBP, RAW, TIFF, BMP e PDF
            var mapeamentoExtensaoTipoMIME = new Dictionary<string, string>
            {
                { ".jpg", "image/jpg" },
                { ".jpeg", "image/jpeg" },
                { ".png", "image/png" },
                { ".gif", "image/gif" },
                { ".svg", "image/svg" },
                { ".psd", "image/psd" },
                { ".webp", "image/webp" },
                { ".tiff", "image/tiff" },
                { ".jfif", "image/jfif" },
                { ".bmp", "image/bmp" },
                { ".pdf", "image/pdf" }
            };

            // Tente obter o tipo MIME da extensão
            if (mapeamentoExtensaoTipoMIME.TryGetValue(extensao, out var tipoMIME))
            {
                return tipoMIME;
            }

            // Se a extensão não estiver mapeada, retorne nulo ou uma string vazia
            return null;
        }
    }
}
