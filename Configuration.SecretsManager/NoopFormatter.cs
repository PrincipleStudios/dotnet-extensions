namespace PrincipleStudios.Extensions.Configuration.SecretsManager
{
    internal class NoopFormatter : IFormatTransform
    {
        public TransformedConfiguration TransformSecret(string secret)
        {
            return new TransformedConfiguration(secret);
        }
    }
}