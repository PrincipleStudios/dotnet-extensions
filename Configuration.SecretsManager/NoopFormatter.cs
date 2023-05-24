namespace PrincipleStudios.Extensions.Configuration.SecretsManager
{
	internal class NoopFormatter : IFormatTransform
	{
		public TransformedConfiguration TransformSecret(string secret, string? arg)
		{
			return new TransformedConfiguration(secret);
		}
	}
}