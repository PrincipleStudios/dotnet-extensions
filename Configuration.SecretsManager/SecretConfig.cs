using System;

namespace PrincipleStudios.Extensions.Configuration.SecretsManager
{
	public struct SecretConfig
	{
		public string SecretId { get; set; }
		public string? Format { get; set; }
		public string? Argument { get; set; }

		internal bool IsValid()
		{
			return SecretId != null;
		}
	}
}
