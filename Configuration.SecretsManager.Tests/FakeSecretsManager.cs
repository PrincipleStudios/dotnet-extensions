using Amazon.Runtime;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PrincipleStudios.Extensions.Configuration.SecretsManager
{
	class FakeSecretsManager : IAmazonSecretsManager
	{
		public const string CurrentVersionStage = "AWSCURRENT";
		record SecretRecord(string Value, HashSet<string> VersionStages);

		private Dictionary<string /* SecretId */, Dictionary<string /*VersionId*/, SecretRecord>> secretValues = new Dictionary<string, Dictionary<string, SecretRecord>>();

		//public string GetSecretAtStage(string secretId, string versionStage)
		//{
		//    return secretValues[secretId].Values.Single(v => v.VersionStages.Contains(versionStage)).Value;
		//}

		public string SetSecret(string secretId, string versionStage, string value)
		{
			secretValues[secretId] = secretValues.TryGetValue(secretId, out var dict) ? dict : new();

			var id = Guid.NewGuid().ToString();

			foreach (var otherVersion in secretValues[secretId])
				otherVersion.Value.VersionStages.Remove(versionStage);

			secretValues[secretId][id] = new SecretRecord(Value: value, VersionStages: new() { versionStage });
			return id;
		}

		public ISecretsManagerPaginatorFactory Paginators => throw new NotImplementedException();

		public IClientConfig Config => throw new NotImplementedException();

		public Task<CancelRotateSecretResponse> CancelRotateSecretAsync(CancelRotateSecretRequest request, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public Task<CreateSecretResponse> CreateSecretAsync(CreateSecretRequest request, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public Task<DeleteResourcePolicyResponse> DeleteResourcePolicyAsync(DeleteResourcePolicyRequest request, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public Task<DeleteSecretResponse> DeleteSecretAsync(DeleteSecretRequest request, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public Task<DescribeSecretResponse> DescribeSecretAsync(DescribeSecretRequest request, CancellationToken cancellationToken = default)
		{
			if (!secretValues.TryGetValue(request.SecretId, out var secretVersions))
				throw new Amazon.SecretsManager.Model.ResourceNotFoundException(new Amazon.Runtime.Internal.HttpErrorResponseException(null));

			return Task.FromResult(new DescribeSecretResponse
			{
				HttpStatusCode = System.Net.HttpStatusCode.OK,
				VersionIdsToStages = secretVersions.ToDictionary(e => e.Key, e => e.Value.VersionStages.ToList()),
			});
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}

		public Task<GetRandomPasswordResponse> GetRandomPasswordAsync(GetRandomPasswordRequest request, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public Task<GetResourcePolicyResponse> GetResourcePolicyAsync(GetResourcePolicyRequest request, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public Task<GetSecretValueResponse> GetSecretValueAsync(GetSecretValueRequest request, CancellationToken cancellationToken = default)
		{
			if (!secretValues.TryGetValue(request.SecretId, out var secretVersions))
				throw new Amazon.SecretsManager.Model.ResourceNotFoundException(new Amazon.Runtime.Internal.HttpErrorResponseException(null));

			var id = request.VersionId ?? secretVersions.SingleOrDefault(v => v.Value.VersionStages.Contains(request.VersionStage ?? CurrentVersionStage)).Key;
			if (id == null || !secretVersions.TryGetValue(id, out var secretVersion))
				throw new Amazon.SecretsManager.Model.ResourceNotFoundException(new Amazon.Runtime.Internal.HttpErrorResponseException(null));

			return Task.FromResult(new GetSecretValueResponse
			{
				HttpStatusCode = System.Net.HttpStatusCode.OK,
				VersionId = id,
				SecretString = secretVersions[id].Value,
			});
		}

		public Task<ListSecretsResponse> ListSecretsAsync(ListSecretsRequest request, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public Task<ListSecretVersionIdsResponse> ListSecretVersionIdsAsync(ListSecretVersionIdsRequest request, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public Task<PutResourcePolicyResponse> PutResourcePolicyAsync(PutResourcePolicyRequest request, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public Task<PutSecretValueResponse> PutSecretValueAsync(PutSecretValueRequest request, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public Task<RemoveRegionsFromReplicationResponse> RemoveRegionsFromReplicationAsync(RemoveRegionsFromReplicationRequest request, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public Task<ReplicateSecretToRegionsResponse> ReplicateSecretToRegionsAsync(ReplicateSecretToRegionsRequest request, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public Task<RestoreSecretResponse> RestoreSecretAsync(RestoreSecretRequest request, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public Task<RotateSecretResponse> RotateSecretAsync(RotateSecretRequest request, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public Task<StopReplicationToReplicaResponse> StopReplicationToReplicaAsync(StopReplicationToReplicaRequest request, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public Task<TagResourceResponse> TagResourceAsync(TagResourceRequest request, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public Task<UntagResourceResponse> UntagResourceAsync(UntagResourceRequest request, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public Task<UpdateSecretResponse> UpdateSecretAsync(UpdateSecretRequest request, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public Task<UpdateSecretVersionStageResponse> UpdateSecretVersionStageAsync(UpdateSecretVersionStageRequest request, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public Task<ValidateResourcePolicyResponse> ValidateResourcePolicyAsync(ValidateResourcePolicyRequest request, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}
	}
}
