
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace PrincipleStudios.Extensions.Configuration.SecretsManager
{
    internal class JsonFormatTransform : IFormatTransform
    {
        public TransformedConfiguration TransformSecret(string secret)
        {
            try
            {
                var doc = JsonDocument.Parse(secret);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                    return new TransformedConfiguration(secret);
                return new TransformedConfiguration(from property in doc.RootElement.EnumerateObject()
                                                    let value = property.Value
                                                    select new KeyValuePair<string, string>(
                                                        property.Name, 
                                                        value.ToString() ?? value.GetRawText()
                                                    ));
            }
            catch
            {
                return new TransformedConfiguration(secret);
            }
        }
    }
}