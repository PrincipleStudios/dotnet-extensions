using Microsoft.Extensions.Configuration;
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
				if (options.EnvironmentVariableLoadConfiguration is EnvironmentVariableLoadConfiguration { SecretIdPrefix: string idPrefix, SecretFormatPrefix: string formatPrefix, SecretArgumentPrefix: string argumentPrefix })
					AddEnvironmentVariablesToMap(idPrefix, formatPrefix, argumentPrefix);

				if (!options.Map.Any())
					return new NoopConfigurationProvider();

				return new SecretsManagerConfigurationProvider(options);
			}
			catch
			{
				if (!optional)
					throw;
				return new NoopConfigurationProvider();
			}
		}

		private void AddEnvironmentVariablesToMap(string idPrefix, string formatPrefix, string argumentPrefix)
		{
			var idConfig = new ConfigurationBuilder().AddEnvironmentVariables(idPrefix).Build();
			var formatConfig = new ConfigurationBuilder().AddEnvironmentVariables(formatPrefix).Build();
			var argumentConfig = new ConfigurationBuilder().AddEnvironmentVariables(argumentPrefix).Build();
			foreach (var key in idConfig.AsEnumerable().Where(kvp => kvp.Value != null))
			{
				options.Map.Add(key.Key, new SecretConfig { SecretId = key.Value ?? string.Empty, Format = formatConfig[key.Key], Argument = argumentConfig[key.Key] });
			}
		}
	}

	internal class NoopConfigurationProvider : ConfigurationProvider
	{
		public NoopConfigurationProvider() { }
	}
}
