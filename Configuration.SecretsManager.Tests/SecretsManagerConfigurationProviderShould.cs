using System;
using Xunit;
using Amazon.SecretsManager;
using Moq;
using Amazon.SecretsManager.Model;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

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
                    { "Secrets:secret", new () { SecretId = "test/secret" } }
                },
                SecretsManagerClientFactory = () => secretManager,
            });
            var configuration = new ConfigurationBuilder().Add(target).Build();

            var actual = configuration["Secrets:secret"];

            Assert.Equal(expected, actual);
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
                    { "Secrets:secret", new () { SecretId = "test/secret" } }
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
                    { "Secrets:secret", new () { SecretId = "test/secret" } }
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
                    { "Secrets:secret", new () { SecretId = "test/secret" } }
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
    }
}
