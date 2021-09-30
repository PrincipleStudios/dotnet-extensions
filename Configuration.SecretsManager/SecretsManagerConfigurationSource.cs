using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PrincipleStudios.Extensions.Configuration.SecretsManager
{
    public class SecretsManagerConfigurationSource : IConfigurationSource
    {
        private readonly SecretsManagerConfigurationOptions options;
        private readonly bool optional;

        public SecretsManagerConfigurationSource(SecretsManagerConfigurationOptions options, bool optional = false)
        {
            this.options = options;
            this.optional = optional;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            try
            {
                if (options.EnvironmentVariableLoadConfiguration is EnvironmentVariableLoadConfiguration { SecretIdPrefix: string idPrefix, SecretFormatPrefix: string formatPrefix })
                    AddEnvironmentVariablesToMap(idPrefix, formatPrefix);

                return new SecretsManagerConfigurationProvider(options);
            }
            catch
            {
                if (!optional)
                    throw;
                return new NoopConfigurationProvider();
            }
        }

        private void AddEnvironmentVariablesToMap(string idPrefix, string formatPrefix)
        {
            var idConfig = new ConfigurationBuilder().AddEnvironmentVariables(idPrefix).Build();
            var formatConfig = new ConfigurationBuilder().AddEnvironmentVariables(formatPrefix).Build();
            foreach (var key in idConfig.AsEnumerable().Where(kvp => kvp.Value != null))
            {
                options.Map.Add(key.Key, new SecretConfig { SecretId = key.Value, Format = formatConfig[key.Key] });
            }
        }
    }

    internal class NoopConfigurationProvider : ConfigurationProvider
    {
        public NoopConfigurationProvider() { }
    }
}
