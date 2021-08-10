using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;

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
                return new SecretsManagerConfigurationProvider(options);
            }
            catch
            {
                if (!optional)
                    throw;
                return new NoopConfigurationProvider();
            }
        }

    }

    internal class NoopConfigurationProvider : ConfigurationProvider
    {
        public NoopConfigurationProvider() { }
    }
}
