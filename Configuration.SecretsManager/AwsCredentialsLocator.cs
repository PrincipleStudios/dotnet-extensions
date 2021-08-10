using System;
using Amazon.Runtime;

namespace PrincipleStudios.Extensions.Configuration.SecretsManager
{
    internal static class AwsCredentialsLocator
    {
        internal class AwsEndpointCredentials
        {
            public AWSCredentials? Credentials { get; set; }
            public Amazon.RegionEndpoint? RegionEndpoint { get; set; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "AWS SDK is weird")]
        internal static AwsEndpointCredentials LocateCredentials()
        {
            var result = new AwsEndpointCredentials();
            try
            {
                result.Credentials = Amazon.Runtime.AssumeRoleWithWebIdentityCredentials.FromEnvironmentVariables();
            }
            catch { }


            var chain = new Amazon.Runtime.CredentialManagement.CredentialProfileStoreChain();
            if (chain.TryGetProfile(Environment.GetEnvironmentVariable("AWS_PROFILE"), out var profile))
            {
                result.Credentials = profile.GetAWSCredentials(profile.CredentialProfileStore);
                result.RegionEndpoint = profile.Region;
            }

            if (Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID") is string && Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY") is string)
                result.Credentials = new EnvironmentVariablesAWSCredentials();

            if (Environment.GetEnvironmentVariable("AWS_DEFAULT_REGION") is string region)
                result.RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region);

            return result;
        }
    }
}