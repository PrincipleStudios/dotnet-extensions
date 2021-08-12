
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace PrincipleStudios.Extensions.Configuration.SecretsManager
{
    public class SecretsManagerConfigurationOptions
    {
        public string? CredentialsProfile
        {
            set
            {
                if (value == null)
                {
                    Credentials = null;
                    return;
                }

                var chain = new Amazon.Runtime.CredentialManagement.CredentialProfileStoreChain();

                if (chain.TryGetProfile(value, out var profile))
                {
                    Credentials = profile.GetAWSCredentials(profile.CredentialProfileStore);
                    RegionEndpoint = profile.Region ?? RegionEndpoint;
                }
            }
        }
        public CredentialProfileOptions CredentialProfileOptions
        {
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                Credentials = Amazon.Runtime.CredentialManagement.AWSCredentialsFactory.GetAWSCredentials(value, null, true);
            }
        }

        public string? Region
        {
            get => RegionEndpoint?.SystemName;
            set => RegionEndpoint = value == null ? null : Amazon.RegionEndpoint.GetBySystemName(value);
        }
        public AWSCredentials? Credentials { get; set; }
        public RegionEndpoint? RegionEndpoint { get; set; }

        /// <summary>
        /// Builds the secrets manager client - for overriding during unit tests, etc.
        /// </summary>
        public Func<IAmazonSecretsManager>? SecretsManagerClientFactory { get; set; }
        public Action<AmazonSecretsManagerConfig>? ConfigureSecretsManagerClientConfig { get; set; }

        public TimeSpan? ReloadInterval { get; set; }
        /// <summary>
        /// Key-value list: Key is the Configuration Key
        /// </summary>
        public IDictionary<string, SecretConfig> Map { get; set; } = new Dictionary<string, SecretConfig>();
        /// <summary>
        /// Key-value list: Key is the Configuration 
        /// </summary>
        public IDictionary<string, IFormatTransform> FormatTransforms { get; set; } = new Dictionary<string, IFormatTransform>()
        {
            { "RDS-sqlserver", new RdsSqlServerSecretFormatTransform() },
            { "RDS-npgsql", new RdsNpgsqlSecretFormatTransform() },
        };
        public IFormatTransform? DefaultFormatter { get; internal set; } = new JsonFormatTransform();
    }
}
