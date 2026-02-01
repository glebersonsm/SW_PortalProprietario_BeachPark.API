using Dapper;
using SW_Utils.Auxiliar;
using SW_Utils.Enum;
using System.Data.Common;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SW_Utils.Functions
{
    public static class RepositoryUtils
    {
        public static DynamicParameters GetParametersForSql(params Parameter[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
                return new DynamicParameters();

            DynamicParameters parametersReturn = new();
            foreach (var parameter in parameters)
            {
                parametersReturn.Add(parameter.Name, parameter.Value);
            }

            return parametersReturn;
        }

        public static string NormalizeFunctions(EnumDataBaseType dataBaseType, string sql)
        {
            if (dataBaseType == EnumDataBaseType.Oracle)
            {
                sql = sql.Replace("Coalesce(", "NVL(", StringComparison.CurrentCultureIgnoreCase);
            }

            return sql;
        }

    }

    public static class PaginationHelper
    {
        public static string GetPaginatedQuery(string sql, EnumDataBaseType dataBaseType, int offset, int pageSize)
        {
            if (offset < 1)
                offset = 1;

            switch (dataBaseType)
            {
                case EnumDataBaseType.SqlServer:
                    return GetSqlServerPaginatedQuery(sql, offset, pageSize);
                case EnumDataBaseType.PostgreSql:
                    return GetPostgreSqlPaginatedQuery(sql, offset, pageSize);
                case EnumDataBaseType.MySql:
                    return GetMySqlPaginateQuery(sql, offset, pageSize);
                case EnumDataBaseType.SqLite:
                    return GetSqLitePaginatedQuery(sql, offset, pageSize);
                case EnumDataBaseType.Oracle:
                    return GetOraclePaginatedQueryOldVersions(sql, offset, pageSize);
                default:
                    throw new ArgumentException($"Tipo de banco de dados não suportado");
            }
        }

        private static string GetSqlServerPaginatedQuery(string sql, int offset, int pageSize)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(sql);
            stringBuilder.AppendLine($" OFFSET {(offset - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");

            return stringBuilder.ToString();
        }

        private static string GetPostgreSqlPaginatedQuery(string sql, int offset, int pageSize)
        {
            return $"{sql} LIMIT {pageSize} OFFSET {(offset - 1) * pageSize}";
        }

        private static string GetSqLitePaginatedQuery(string sql, int offset, int pageSize)
        {
            return $"{sql} LIMIT {pageSize} OFFSET {(offset - 1) * pageSize}";
        }

        private static string GetMySqlPaginateQuery(string sql, int offset, int pageSize)
        {
            return $"{sql} LIMIT {pageSize} OFFSET {(offset - 1) * pageSize}";
        }

        private static string GetOraclePaginatedQueryOldVersions(string sql, int offset, int pageSize)
        {
            sql = sql.Replace("\n", " ").Replace("\r", " ").Replace("  ", " ").Replace("\t", " ");

            if (offset == 0)
                offset = 1;

            var primeiraLinha = offset > 1 ? ((offset - 1) * pageSize) + 1 : offset;
            var ultimaLinha = primeiraLinha + pageSize - 1;

            var sqlTextResult = @$"
                    Select 
                        a287_.* 
                        From (
                                Select t287_.*, 
                                    ROWNUM AS rn287_
                                From
                                   ({sql}) t287_ Where ROWNUM <= {ultimaLinha}   
                        ) a287_
                        Where rn287_ >= {primeiraLinha} ";

            return sqlTextResult;
        }


        private static string GetOraclePaginatedQueryPlus12C(string sql, int offset, int pageSize)
        {
            return $"{sql} OFFSET {(offset - 1) * pageSize} ROWS FETCH {pageSize} ROWS ONLY ";
        }
    }

    public static class QueryHasher
    {
        // Regex para normalizar espaços e quebras de linha, e remover comentários de linha e bloco
        private static readonly Regex NormalizeWhitespaceRegex = new Regex(@"\s+", RegexOptions.Compiled);
        private static readonly Regex RemoveLineCommentsRegex = new Regex(@"--.*$", RegexOptions.Multiline | RegexOptions.Compiled);
        private static readonly Regex RemoveBlockCommentsRegex = new Regex(@"/\*.*?\*/", RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// Gera um hash SHA256 de uma consulta SQL e seus parâmetros.
        /// </summary>
        /// <param name="sqlQuery">A string da consulta SQL.</param>
        /// <param name="parameters">Uma coleção de DbParameter (ou seus derivados como SqlParameter).</param>
        /// <returns>O hash SHA256 da consulta e parâmetros como uma string hexadecimal.</returns>
        public static string GenerateQueryHash(string sqlQuery, IEnumerable<Parameter> parameters)
        {
            if (string.IsNullOrWhiteSpace(sqlQuery))
            {
                throw new ArgumentException("A consulta SQL não pode ser nula ou vazia.", nameof(sqlQuery));
            }

            // 1. Normalizar a consulta SQL
            string normalizedQuery = NormalizeSqlQuery(sqlQuery);

            // 2. Normalizar e serializar os parâmetros
            string serializedParameters = SerializeParameters(parameters);

            // 3. Combinar a consulta normalizada e os parâmetros serializados
            string combinedString = normalizedQuery + "|" + serializedParameters;

            // 4. Gerar o hash SHA256
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(combinedString));

                // Converter o array de bytes para uma string hexadecimal
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// Normaliza a string da consulta SQL removendo comentários, normalizando espaços e convertendo para maiúsculas.
        /// </summary>
        private static string NormalizeSqlQuery(string query)
        {
            // Remover comentários de bloco primeiro
            query = RemoveBlockCommentsRegex.Replace(query, "");
            // Remover comentários de linha
            query = RemoveLineCommentsRegex.Replace(query, "");
            // Normalizar espaços (múltiplos espaços/quebras de linha para um único espaço)
            query = NormalizeWhitespaceRegex.Replace(query, " ").Trim();
            // Opcional: Converter para maiúsculas para maior consistência
            // Cuidado: Isso pode ser um problema se identificadores de banco de dados forem case-sensitive
            // query = query.ToUpperInvariant();
            return query;
        }

        /// <summary>
        /// Serializa os parâmetros de forma consistente para inclusão no hash.
        /// </summary>
        private static string SerializeParameters(IEnumerable<Parameter> parameters)
        {
            if (parameters == null || !parameters.Any())
            {
                return string.Empty;
            }

            // Ordena os parâmetros pelo nome para garantir consistência
            var orderedParameters = parameters.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase);

            StringBuilder sb = new StringBuilder();
            foreach (var p in orderedParameters)
            {
                // Inclui nome, valor e tipo de dados para maior precisão
                // Use GetType() para o valor para lidar com valores nulos de forma consistente
                sb.Append($"{p.Name}:{p.Value?.ToString() ?? "NULL"}:{p.Value};");
            }
            return sb.ToString();
        }
    }
}
