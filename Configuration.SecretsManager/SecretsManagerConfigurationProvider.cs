using Microsoft.Extensions.Options;
using Amazon.SecretsManager;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Amazon.SecretsManager.Extensions.Caching;
using System.Collections.Concurrent;

namespace PrincipleStudios.Extensions.Configuration.SecretsManager
{
	public class SecretsManagerConfigurationProvider : IConfigurationProvider, IDisposable
	{
		private readonly uint millisecondsCacheDuration;
		private Lazy<ConfigurationReloadToken> timedReloadToken;
		private readonly IAmazonSecretsManager secretsManager;
		private readonly SecretsManagerCache cache;
		private readonly SecretsManagerConfigurationOptions options;

		private readonly IFormatTransform noopFormatter = new NoopFormatter();

		public SecretsManagerConfigurationProvider(SecretsManagerConfigurationOptions options)
		{
			this.options = options;
			this.millisecondsCacheDuration = options.ReloadInterval is TimeSpan ts ? (uint)ts.TotalMilliseconds : SecretCacheConfiguration.DEFAULT_CACHE_ITEM_TTL;
			this.timedReloadToken = new(BuildTimeoutToken);
			this.secretsManager = CreateClient(options);
			this.cache = new SecretsManagerCache(this.secretsManager, new SecretCacheConfiguration { CacheItemTTL = millisecondsCacheDuration });
		}

		public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string parentPath)
		{
			var fullParentPath = parentPath is { Length: > 0 } ? $"{parentPath}:" : "";
			if (options.Map == null)
				return earlierKeys;
			return earlierKeys.Concat(from entry in options.Map.AsEnumerable()
									  where entry.Value.IsValid()
									  let key = entry.Key
									  let formatter = GetFormatTransform(entry.Value.Format)
									  from secretEntry in formatter.TransformSecret(
										  UnwrapTask<string>(this.cache.GetSecretString(entry.Value.SecretId), ex => new Exception($"Encountered issue getting secret {entry.Value.SecretId} for configuration at {entry.Key}", ex)),
										  entry.Value.Argument
									  ).GetKeyValuePairs(key)
									  where secretEntry.Key.StartsWith(fullParentPath)
									  select secretEntry.Key.Substring(fullParentPath.Length));
		}

		private IFormatTransform GetFormatTransform(string? format)
		{
			return options.FormatTransforms.TryGetValue(format ?? options.DefaultFormatter, out var f) ? f : noopFormatter;
		}

		public IChangeToken GetReloadToken()
		{
			return timedReloadToken.Value;
		}

		public void Load()
		{
			UnwrapTask(Task.WhenAll(
				from secretId in options.Map.Values.Select(v => v.SecretId).Distinct()
				select this.cache
					.RefreshNowAsync(secretId)
					.ContinueWith(t => t.IsFaulted
						? throw new Exception($"Could not load secret {secretId}", t.Exception)
						: t.Result)
			), ex => new Exception($"Encountered issue loading all secrets", ex));
		}

		public void Set(string key, string value)
		{
			throw new System.NotSupportedException("Cannot update secrets");
		}

		public bool TryGet(string originalKey, out string? value)
		{
			var key = options.Map.Keys.FirstOrDefault(k => originalKey.StartsWith(k + ":")) ?? originalKey;
			var suffix = key == originalKey ? null : originalKey.Substring(key.Length + 1);

			if (!options.Map.ContainsKey(key) || options.Map?[key] is not SecretConfig config || !config.IsValid())
			{
				value = null;
				return false;
			}

			var formatter = GetFormatTransform(config.Format);

			try
			{
				value = UnwrapTask(GetSingleSecret(config.SecretId, suffix, formatter, config.Argument), ex => new Exception($"Encountered issue getting secret {config.SecretId} for configuration at {originalKey}", ex));
				return true;
			}
			catch (Exception)
			{
				value = null;
				return false;
			}
		}

		private async Task<string?> GetSingleSecret(string secretId, string? suffix, IFormatTransform formatter, string? argument)
		{
			var secretString = await cache.GetSecretString(secretId).ConfigureAwait(false);
			if (formatter == null && suffix == null)
				return secretString;
			if (formatter == null)
				return null;
			return formatter.TransformSecret(secretString, argument).GetValue(suffix);
		}

		// This feels awfully dirty, but https://github.com/dotnet/runtime/issues/36018 is blocking proper async
		private static T UnwrapTask<T>(Task<T> taskToUnwrap, Func<Exception, Exception> wrapException)
		{
			if (taskToUnwrap.IsCompleted)
			{
				if (taskToUnwrap.IsFaulted)
					throw wrapException(taskToUnwrap.Exception);
				return taskToUnwrap.Result;
			}
			return taskToUnwrap.ConfigureAwait(false).GetAwaiter().GetResult();
		}

		private ConfigurationReloadToken BuildTimeoutToken()
		{
			var result = new ConfigurationReloadToken();

			var tokenSource = options.ReloadInterval.HasValue ? new System.Threading.CancellationTokenSource(options.ReloadInterval.Value) : null;
			tokenSource?.Token.Register(Reload);

			return result;

			void Reload()
			{
				tokenSource?.Dispose();
				tokenSource = null;

				var original = System.Threading.Interlocked.Exchange(ref timedReloadToken, new(BuildTimeoutToken));
				original.Value.OnReload();
			}
		}


		private static IAmazonSecretsManager CreateClient(SecretsManagerConfigurationOptions options)
		{
			return options.SecretsManagerClientFactory?.Invoke() ?? StandardCreateClient();

			IAmazonSecretsManager StandardCreateClient()
			{
				var credentials = options.Credentials;
				var region = options.RegionEndpoint;
				if (credentials == null)
				{
					var credsResult = AwsCredentialsLocator.LocateCredentials();
					credentials = credsResult.Credentials;
					region ??= credsResult.RegionEndpoint;
				}

				var clientConfig = new AmazonSecretsManagerConfig
				{
					RegionEndpoint = region
				};

				options.ConfigureSecretsManagerClientConfig?.Invoke(clientConfig);

				return credentials is Amazon.Runtime.AWSCredentials
					? new AmazonSecretsManagerClient(credentials, clientConfig)
					: new AmazonSecretsManagerClient(clientConfig);
			}
		}

		void IDisposable.Dispose()
		{
			this.secretsManager?.Dispose();
			this.cache?.Dispose();
		}
	}
}