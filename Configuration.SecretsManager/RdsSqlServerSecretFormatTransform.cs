using System;
using System.Linq;

namespace PrincipleStudios.Extensions.Configuration.SecretsManager
{
    public class RdsSqlServerSecretFormatTransform : RdsSecretFormatTransform
    {
        public RdsSqlServerSecretFormatTransform() : base(TransformSqlServerConnectionString) { }

        private static string TransformSqlServerConnectionString(RdsSecret secret) => secret switch
        {
            { Engine: not "sqlserver" } => throw new System.ArgumentException("Engine must be 'sqlserver'"),
            { Host: string host, Username: string username, Password: string password,  Dbname: var dbName,  Port: var port, } => 
                ToConnectionString($"Server={host},{port ?? 1433};Database={dbName ?? "master"};User Id={username};Password={password};"),
            _ => throw new System.ArgumentException("Missing required parameter"),
        };


        public static string ToConnectionString(FormattableString connectionString)
        {
            return string.Format(connectionString.Format, connectionString.GetArguments().Select(SanitizeArgument).ToArray());
        }

        private static string? SanitizeArgument(object? argument)
        {
            var value = argument?.ToString();
            return (value != null && (value.Contains('\'') || value.Contains(';')))
                ? "'" + value.Replace("'", "''") + "'"
                : value;
        }
    }
}