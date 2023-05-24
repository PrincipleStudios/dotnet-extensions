using System;
using Xunit;
using Amazon.SecretsManager;
using Moq;
using Amazon.SecretsManager.Model;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace PrincipleStudios.Extensions.Configuration.SecretsManager.Tests
{
	public class SecretsManagerConfigurationProviderShould
	{
		[Fact]
		public void GetValues()
		{
			var expected = "foobar";

			var secretManager = new FakeSecretsManager();
			secretManager.SetSecret("test/secret", FakeSecretsManager.CurrentVersionStage, expected);

			var target = new SecretsManagerConfigurationSource(new SecretsManagerConfigurationOptions
			{
				CredentialsProfile = "ps",
				Map =
				{
					{ "Secrets:secret", new () { SecretId = "test/secret", Format = "noop" } }
				},
				SecretsManagerClientFactory = () => secretManager,
			});
			var configuration = new ConfigurationBuilder().Add(target).Build();

			var actual = configuration["Secrets:secret"];

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void SupportRdsSqlServerTransformedSecrets()
		{
			var expected = "Server=example.com,1433;Database=master;User Id=admin;Password=1234;";

			var secretManager = new FakeSecretsManager();
			secretManager.SetSecret("test/db", FakeSecretsManager.CurrentVersionStage, "{\"engine\":\"sqlserver\",\"host\":\"example.com\",\"username\":\"admin\",\"password\":\"1234\"}");

			var target = new SecretsManagerConfigurationSource(new SecretsManagerConfigurationOptions
			{
				CredentialsProfile = "ps",
				Map =
				{
					{ "ConnectionStrings:sql-server", new () { SecretId = "test/db", Format = "RDS-sqlserver" } }
				},
				SecretsManagerClientFactory = () => secretManager,
			});
			var configuration = new ConfigurationBuilder().Add(target).Build();

			var actual = configuration["ConnectionStrings:sql-server"];

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void SupportRdsNpgsqlTransformedSecrets()
		{
			var expected = "Server=example.com;Port=5432;Database=postgres;User Id=admin;Password=1234;";

			var secretManager = new FakeSecretsManager();
			secretManager.SetSecret("test/db", FakeSecretsManager.CurrentVersionStage, "{\"engine\":\"postgres\",\"host\":\"example.com\",\"username\":\"admin\",\"password\":\"1234\"}");

			var target = new SecretsManagerConfigurationSource(new SecretsManagerConfigurationOptions
			{
				CredentialsProfile = "ps",
				Map =
				{
					{ "ConnectionStrings:postgres", new () { SecretId = "test/db", Format = "RDS-npgsql" } }
				},
				SecretsManagerClientFactory = () => secretManager,
			});
			var configuration = new ConfigurationBuilder().Add(target).Build();

			var actual = configuration["ConnectionStrings:postgres"];

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void SupportCustomTransformedSecrets()
		{
			var expected = "SOMETHING";

			var secretManager = new FakeSecretsManager();
			secretManager.SetSecret("test/secret", FakeSecretsManager.CurrentVersionStage, "something");

			var target = new SecretsManagerConfigurationSource(new SecretsManagerConfigurationOptions
			{
				CredentialsProfile = "ps",
				Map =
				{
					{ "Secrets:secret", new () { SecretId = "test/secret", Format = "custom" } }
				},
				FormatTransforms =
				{
					{ "custom", new CustomTransform() }
				},
				SecretsManagerClientFactory = () => secretManager,
			});
			var configuration = new ConfigurationBuilder().Add(target).Build();

			var actual = configuration["Secrets:secret"];

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void SupportJsonTransformsByDefault()
		{
			var expected = "foobar";

			var secretManager = new FakeSecretsManager();
			secretManager.SetSecret("test/secret", FakeSecretsManager.CurrentVersionStage, $@"{{ ""nested"": ""{expected}"" }}");

			var target = new SecretsManagerConfigurationSource(new SecretsManagerConfigurationOptions
			{
				CredentialsProfile = "ps",
				Map =
				{
					{ "Secrets:secret", new () { SecretId = "test/secret" } }
				},
				SecretsManagerClientFactory = () => secretManager,
			});
			var configuration = new ConfigurationBuilder().Add(target).Build();

			var actual = configuration["Secrets:secret:nested"];

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void SupportKeyedJsonTransforms()
		{
			var expected = "foobar";

			var secretManager = new FakeSecretsManager();
			secretManager.SetSecret("test/secret", FakeSecretsManager.CurrentVersionStage, $@"{{ ""nested"": ""{expected}"" }}");

			var target = new SecretsManagerConfigurationSource(new SecretsManagerConfigurationOptions
			{
				CredentialsProfile = "ps",
				Map =
				{
					{ "Secrets:secret", new () { SecretId = "test/secret", Argument = "nested" } }
				},
				SecretsManagerClientFactory = () => secretManager,
			});
			var configuration = new ConfigurationBuilder().Add(target).Build();

			var actual = configuration["Secrets:secret"];

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetsChildren()
		{
			var secretManager = new FakeSecretsManager();
			secretManager.SetSecret("test/secret", FakeSecretsManager.CurrentVersionStage, "foobar");
			secretManager.SetSecret("test/secret/nested", FakeSecretsManager.CurrentVersionStage, $@"{{ ""nested"": ""foobar"", ""other"": ""baz"" }}");

			var target = new SecretsManagerConfigurationSource(new SecretsManagerConfigurationOptions
			{
				CredentialsProfile = "ps",
				Map =
				{
					{ "Secrets:secret", new () { SecretId = "test/secret", Format = "noop" } },
					{ "Secrets:withNested", new () { SecretId = "test/secret/nested" } }
				},
				SecretsManagerClientFactory = () => secretManager,
			});
			var configuration = new ConfigurationBuilder().Add(target).Build();

			var actual = configuration.GetSection("Secrets:withNested").GetChildren().Select(c => c.Key).OrderBy(s => s).ToArray();

			Assert.Collection(actual
				, a => Assert.Equal("nested", a)
				, a => Assert.Equal("other", a)
			);
		}

		[Fact]
		public void GetValueCached()
		{
			var expected1 = "foobar";
			var expected2 = "foobaz";

			var secretManager = new FakeSecretsManager();
			secretManager.SetSecret("test/secret", FakeSecretsManager.CurrentVersionStage, expected1);

			var target = new SecretsManagerConfigurationSource(new SecretsManagerConfigurationOptions
			{
				ReloadInterval = TimeSpan.FromMilliseconds(500),
				CredentialsProfile = "ps",
				Map =
				{
					{ "Secrets:secret", new () { SecretId = "test/secret", Format = "noop" } }
				},
				SecretsManagerClientFactory = () => secretManager,
			});
			var configuration = new ConfigurationBuilder().Add(target).Build();

			var original = configuration["Secrets:secret"];
			secretManager.SetSecret("test/secret", FakeSecretsManager.CurrentVersionStage, expected2);
			var actual = configuration["Secrets:secret"];

			Assert.Equal(expected1, original);
			Assert.Equal(expected1, actual);
		}

		[Fact]
		public void GetValueChanges()
		{
			var expected1 = "foobar";
			var expected2 = "foobaz";

			var secretManager = new FakeSecretsManager();
			secretManager.SetSecret("test/secret", FakeSecretsManager.CurrentVersionStage, expected1);

			var target = new SecretsManagerConfigurationSource(new SecretsManagerConfigurationOptions
			{
				ReloadInterval = TimeSpan.FromMilliseconds(500),
				CredentialsProfile = "ps",
				Map =
				{
					{ "Secrets:secret", new () { SecretId = "test/secret", Format = "noop" } }
				},
				SecretsManagerClientFactory = () => secretManager,
			});
			var configuration = new ConfigurationBuilder().Add(target).Build();

			var original = configuration["Secrets:secret"];
			secretManager.SetSecret("test/secret", FakeSecretsManager.CurrentVersionStage, expected2);
			Thread.Sleep(500);
			var actual = configuration["Secrets:secret"];

			Assert.Equal(expected1, original);
			Assert.Equal(expected2, actual);
		}

		[Fact]
		public void GetReloadNotification()
		{
			var wasChanged = false;

			var secretManager = new FakeSecretsManager();

			var target = new SecretsManagerConfigurationSource(new SecretsManagerConfigurationOptions
			{
				ReloadInterval = TimeSpan.FromMilliseconds(500),
				CredentialsProfile = "ps",
				Map =
				{
					{ "Secrets:secret", new () { SecretId = "test/secret" } }
				},
				SecretsManagerClientFactory = () => secretManager,
			});
			var configuration = new ConfigurationBuilder().Add(target).Build();

			configuration.GetReloadToken().RegisterChangeCallback(_ => { wasChanged = true; }, null);

			Thread.Sleep(750);
			Assert.True(wasChanged);
		}

		[Fact]
		public void AllowForceReload()
		{
			var expected1 = "foobar";
			var expected2 = "foobaz";

			var secretManager = new FakeSecretsManager();
			secretManager.SetSecret("test/secret", FakeSecretsManager.CurrentVersionStage, expected1);

			var target = new SecretsManagerConfigurationSource(new SecretsManagerConfigurationOptions
			{
				ReloadInterval = TimeSpan.FromHours(500),
				CredentialsProfile = "ps",
				Map =
				{
					{ "Secrets:secret", new () { SecretId = "test/secret", Format = "noop" } }
				},
				SecretsManagerClientFactory = () => secretManager,
			});
			var configuration = new ConfigurationBuilder().Add(target).Build();

			var original = configuration["Secrets:secret"];
			secretManager.SetSecret("test/secret", FakeSecretsManager.CurrentVersionStage, expected2);
			configuration.Reload();
			var actual = configuration["Secrets:secret"];

			Assert.Equal(expected1, original);
			Assert.Equal(expected2, actual);
		}

		[Fact]
		public void MissingConfiguration()
		{
			var secretManager = new FakeSecretsManager();
			secretManager.SetSecret("test/secret", FakeSecretsManager.CurrentVersionStage, "foobar");

			var target = new SecretsManagerConfigurationSource(new SecretsManagerConfigurationOptions
			{
				CredentialsProfile = "ps",
				Map =
				{
					{ "Secrets:secret", new () { SecretId = "test/secret" } }
				},
				SecretsManagerClientFactory = () => secretManager,
			});
			var configuration = new ConfigurationBuilder().Add(target).Build();

			var actual = configuration["SomethingElse"];

			Assert.Null(actual);
		}

		[Fact]
		public void FallThroughConfiguration()
		{
			var expected = "baz";
			var key = "SomethingElse";

			var secretManager = new FakeSecretsManager();
			secretManager.SetSecret("test/secret", FakeSecretsManager.CurrentVersionStage, "foobar");

			var target = new SecretsManagerConfigurationSource(new SecretsManagerConfigurationOptions
			{
				CredentialsProfile = "ps",
				Map =
				{
					{ "Secrets:secret", new () { SecretId = "test/secret", Format = "noop" } }
				},
				SecretsManagerClientFactory = () => secretManager,
			});
			var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string> {
				{ key, expected }
			}).Add(target).Build();

			var actual = configuration["SomethingElse"];

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void AllowKeysFromFallThroughConfiguration()
		{
			var key = "Something:Else";

			var secretManager = new FakeSecretsManager();
			secretManager.SetSecret("test/secret", FakeSecretsManager.CurrentVersionStage, "foobar");

			var target = new SecretsManagerConfigurationSource(new SecretsManagerConfigurationOptions
			{
				CredentialsProfile = "ps",
				Map =
				{
					{ "Secrets:secret", new () { SecretId = "test/secret", Format = "noop" } }
				},
				SecretsManagerClientFactory = () => secretManager,
			});
			var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string> {
				{ key, "baz" }
			})
				.Add(target)
				.Build();

			var actual = configuration.GetSection("Something").GetChildren().Select(s => s.Key).ToArray();

			Assert.Collection(actual, value => Assert.Equal("Else", value));
		}

		private class CustomTransform : IFormatTransform
		{
			public TransformedConfiguration TransformSecret(string secret, string? arg)
			{
				return new TransformedConfiguration(secret.ToUpperInvariant());
			}
		}
	}
}
