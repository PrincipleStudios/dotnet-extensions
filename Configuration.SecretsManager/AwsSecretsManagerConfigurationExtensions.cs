using Amazon.Runtime;
using PrincipleStudios.Extensions.Configuration.SecretsManager;
using System;

namespace Microsoft.Extensions.Configuration
{
    public static class AwsSecretsManagerConfigurationExtensions
    {
        public static IConfigurationBuilder AddSecretsManager(this IConfigurationBuilder configurationBuilder,
            Action<SecretsManagerConfigurationOptions> configurator)
        {
            var options = new SecretsManagerConfigurationOptions();
            configurator.Invoke(options);

            var source = new SecretsManagerConfigurationSource(options);

            configurationBuilder.Add(source);

            return configurationBuilder;
        }
    }
}
