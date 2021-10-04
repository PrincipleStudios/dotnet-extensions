namespace PrincipleStudios.Extensions.Configuration.SecretsManager
{
    public class EnvironmentVariableLoadConfiguration
    {
        public string SecretIdPrefix { get; set; } = "AWSSM_ID_";
        public string SecretFormatPrefix { get; set; } = "AWSSM_FORMAT_";
        public string SecretArgumentPrefix { get; set; } = "AWSSM_ARG_";
    }
}
