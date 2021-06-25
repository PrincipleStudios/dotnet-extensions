## Usage

1. Add this package.
2. Add to your application's configuration builder.

    For example, this will add a root `Secrets:secret` containing the value stored in the secret string within AWS SecretsManager at `test/secret`:

        .AddSecretsManager(opt =>
        {
            opt.Region = "us-east-1";
            opt.CredentialsProfile = "my-credentials-profile";
            opt.Map = new ()
                {
                    { "Secrets:secret", new () { SecretId = "test/secret" } }
                };
        })

3. Use configuration as normal, including `IOptionsMonitor<>` to refresh from AWS periodically.

## Options

* **CredentialsProfile** - Helper property to set load a credentails profile from your system's AWS configuration
* **CredentialProfileOptions** - Helper property to create credentials with advanced configuration
* **Region** - Helper property to set the region endpoint based on AWS's well-known region names
* **Credentials** - Sets AWS credentials directly
* **RegionEndpoint** - Sets the AWS region endpoint directly
* **SecretsManagerClientFactory** - Allows overriding of the SecretsManagerClientFactory. Mostly useful only for unit tests.
* **ConfigureSecretsManagerClientConfig** - Allows further changes to the underlying `AmazonSecretsManagerConfig` before the client is created.
* **ReloadInterval** - Determines duration for caching as well as change notifications for all keys in this secrets manager configuration provider.
* **Map** - Provides a mapping from configuration keys to AWS secret ids for custom structuring of your configuration. Keys are .NET Configuration paths.
    * **SecretId** - The name of the secret id within AWS.