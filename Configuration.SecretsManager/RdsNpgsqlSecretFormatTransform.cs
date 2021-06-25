namespace PrincipleStudios.Extensions.Configuration.SecretsManager
{
    public class RdsNpgsqlSecretFormatTransform : RdsSecretFormatTransform
    {
        public RdsNpgsqlSecretFormatTransform() : base(TransformSqlServerConnectionString) { }

        private static string TransformSqlServerConnectionString(RdsSecret secret) => secret switch
        {
            { Host: string host, Username: string username, Password: string password, Dbname: var dbName, Port: var port, } =>
                ToConnectionString($"Server={host};Port={port ?? 5432 };Database={dbName ?? "postgres"};User Id={username};Password={password};"),
            _ => throw new System.ArgumentException("Missing required parameter"),
        };

    }
}