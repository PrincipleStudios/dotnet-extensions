using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrincipleStudios.Extensions.Configuration.SecretsManager
{
    public interface IFormatTransform
    {
        TransformedConfiguration TransformSecret(string secret, string? arg);
    }

    public ref struct TransformedConfiguration
    {
        public TransformedConfiguration(string singleValue)
        {
            IsSingleValue = true;
            this.SingleValue = singleValue;
            MultipleValues = null;
        }
        public TransformedConfiguration(IEnumerable<KeyValuePair<string, string>> multiValue)
        {
            IsSingleValue = false;
            SingleValue = null;
            this.MultipleValues = multiValue;
        }


        public bool IsSingleValue { get; }
        public string? SingleValue { get; }
        public IEnumerable<KeyValuePair<string, string>>? MultipleValues { get; }

        public IEnumerable<KeyValuePair<string, string>> GetKeyValuePairs(string prefix)
        {
            return IsSingleValue
                ? new[] { new KeyValuePair<string, string>(prefix, SingleValue!) }
                : from entry in MultipleValues!
                  select new KeyValuePair<string, string>(prefix + ":" + entry.Key, entry.Value);
        }

        public string? GetValue(string? suffix)
        {
            return IsSingleValue && suffix == null ? SingleValue
                : IsSingleValue ? null
                : MultipleValues!.Where(kvp => kvp.Key == suffix).Select(kvp => kvp.Value).FirstOrDefault();
        }
    }
}