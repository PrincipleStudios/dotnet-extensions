using Microsoft.Extensions.Configuration;

namespace PrincipleStudios.Extensions.Configuration.SecretsManager
{
    public class SecretsManagerConfigurationSource : IConfigurationSource
    {
        private readonly SecretsManagerConfigurationOptions options;

        public SecretsManagerConfigurationSource(SecretsManagerConfigurationOptions options)
        {
            this.options = options;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new SecretsManagerConfigurationProvider(options);
        }

    }
}
