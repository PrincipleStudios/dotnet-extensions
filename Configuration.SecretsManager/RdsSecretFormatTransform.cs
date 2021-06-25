using System;
using System.Threading.Tasks;

namespace PrincipleStudios.Extensions.Configuration.SecretsManager
{

    public class RdsSecretFormatTransform : IFormatTransform
    {
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
            var rdsSecret = System.Text.Json.JsonSerializer.Deserialize<RdsSecret>(secret);
            if (rdsSecret == null)
                return new ValueTask<string?>((string ? )null);

            return new ValueTask<string?>(transform(rdsSecret));
        }
    }
}