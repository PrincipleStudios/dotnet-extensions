## Usage

1. Add this package.
2. Add to your application's configuration builder.

    For example, this will add a root `Secrets:secret` containing the value stored in the secret string within AWS SecretsManager at `test/secret`:

        .AddSecretsManager()

3. Ensure your application receives AWS standard environment variables to set your credentials. Alternatively, customize via the options object.
4. Add environment variables to create the mapping.

    For example, this will add a root `Secrets:secret` containing the value stored in the secret string within AWS SecretsManager at `test/secret`:

        AWSSM_ID_Secrets__secret=test/secret

5. Use configuration as normal, including `IOptionsMonitor<>` to receive notifications when configuration is refreshed from AWS.

## Environment Variables

Environment variables are used to keep your configuration flexible at run-time.

### Credentials

1. If an `AWS_PROFILE` environment variable is provided, the credentials and region are loaded from the AWS Credential Profile Store Chain.
2. If both `AWS_ACCESS_KEY_ID` and `AWS_SECRET_ACCESS_KEY` environment variables are provided, they are used.
3. If an `AWS_DEFAULT_REGION` environment variable is provided, the region specified overrides other environment variables.

### Configuration Mapping

By default, the following environment variables are used to create configuration entries. (This is configurable via `EnvironmentVariableLoadConfiguration`.) After a prefix, environment variable naming corresponds to that for `Microsoft.Extensions.Configuration.EnvironmentVariables`.

- `AWSSM_ID_` - the name of the secret within Secrets Manager.
- `AWSSM_FORMAT_` - the Format Transform to use when loading the secret into the configuration.

## Options

* **CredentialsProfile** - Helper property to set load a credentails profile from your system's AWS configuration
* **CredentialProfileOptions** - Helper property to create credentials with advanced configuration
* **Region** - Helper property to set the region endpoint based on AWS's well-known region names
* **Credentials** - Sets AWS credentials directly
* **RegionEndpoint** - Sets the AWS region endpoint directly
* **SecretsManagerClientFactory** - Allows overriding of the SecretsManagerClientFactory. Mostly useful only for unit tests.
* **ConfigureSecretsManagerClientConfig** - Allows further changes to the underlying `AmazonSecretsManagerConfig` before the client is created.
* **ReloadInterval** - Determines duration for caching as well as change notifications for all keys in this secrets manager configuration provider.
* **EnvironmentVariableLoadConfiguration** - Determines environment variable prefixes to create the map automatically. Set this to `null` to disable environment variable configuration mapping.
    * **SecretIdPrefix** - Sets the secret name environment variable prefix. (Defaults to `AWSSM_ID_`.)
    * **SecretFormatPrefix** - Sets the secret format transform environment variable prefix. (Defaults to `AWSSM_FORMAT_`.)
* **Map** - Provides a programmatic mapping from configuration keys to AWS secret ids for custom structuring of your configuration. Keys are .NET Configuration paths.
    * **SecretId** - The name of the secret id within AWS.
    * **Format** - Transforms the value in the secret before adding it to the config. See `FormatTransforms`.
* **FormatTransforms** - A dictionary containing format mappings. Each mapping implements the `IFormatTransform`. Preregistered transforms include:
    - _noop_ - Passes the raw value of the secret as a string within the configuration key
    - _Json_ - Expands a Json object into multiple configuration keys nested underneath the main key for the secret.
    - _RDS-sqlserver_ - Expects an RDS secret from SecretsManager and transforms it into a SqlConnection ConnectionString.
    - _RDS-npgsql_ - Expects an RDS secret from SecretsManager and transforms it into a Npgsql ConnectionString.
* **DefaultFormatter** - The default formatter to use when loading a secret. (Defaults to "Json")