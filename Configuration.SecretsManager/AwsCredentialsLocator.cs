using Amazon.Runtime;
using System;

namespace PrincipleStudios.Extensions.Configuration.SecretsManager
{
	internal static class AwsCredentialsLocator
	{
		internal class AwsEndpointCredentials
		{
			public AWSCredentials? Credentials { get; set; }
			public Amazon.RegionEndpoint? RegionEndpoint { get; set; }
		}

		internal static AwsEndpointCredentials LocateCredentials()
		{
			var result = new AwsEndpointCredentials();
			try
			{
				result.Credentials = Amazon.Runtime.AssumeRoleWithWebIdentityCredentials.FromEnvironmentVariables();
			}
			catch { }

			if (result.Credentials == null && Environment.GetEnvironmentVariable("AWS_SSO_PROFILE") is string ssoProfileName &&
				new Amazon.Runtime.CredentialManagement.CredentialProfileStoreChain().TryGetAWSCredentials(ssoProfileName, out var ssoCredentials))
			{
				result.Credentials = ssoCredentials;
			}

			if (result.Credentials == null && Environment.GetEnvironmentVariable("AWS_PROFILE") is string profileName && new Amazon.Runtime.CredentialManagement.CredentialProfileStoreChain().TryGetProfile(profileName, out var profile))
			{
				result.Credentials = profile.GetAWSCredentials(profile.CredentialProfileStore);
				result.RegionEndpoint = profile.Region;
			}

			if (result.Credentials == null && Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID") is not null && Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY") is not null)
				result.Credentials = new EnvironmentVariablesAWSCredentials();

			if (Environment.GetEnvironmentVariable("AWS_DEFAULT_REGION") is string region)
				result.RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region);

			return result;
		}
	}
}