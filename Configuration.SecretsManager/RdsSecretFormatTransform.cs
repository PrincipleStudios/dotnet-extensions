using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace PrincipleStudios.Extensions.Configuration.SecretsManager
{

    public class RdsSecretFormatTransform : IFormatTransform
    {
        private static readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        private readonly RdsSecretTransform transform;

        public delegate string RdsSecretTransform(RdsSecret secret);
        /*
         Formats of secrets from https://docs.aws.amazon.com/code-samples/latest/catalog/lambda_functions-secretsmanager-RDSPostgreSQL-Multiuser.py.html
         */

        public RdsSecretFormatTransform(RdsSecretTransform transform)
        {
            this.transform = transform ?? throw new ArgumentNullException(nameof(transform));
        }

        public ValueTask<string?> TransformSecret(string secret)
        {
            var rdsSecret = JsonSerializer.Deserialize<RdsSecret>(secret, options);
            if (rdsSecret == null)
                return new ValueTask<string?>((string ? )null);

            return new ValueTask<string?>(transform(rdsSecret));
        }

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