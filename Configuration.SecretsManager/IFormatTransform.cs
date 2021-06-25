using System.Threading.Tasks;

namespace PrincipleStudios.Extensions.Configuration.SecretsManager
{
    public interface IFormatTransform
    {
        ValueTask<string?> TransformSecret(string secret);
    }
}