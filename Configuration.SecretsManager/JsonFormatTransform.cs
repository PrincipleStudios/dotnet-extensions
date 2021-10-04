
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace PrincipleStudios.Extensions.Configuration.SecretsManager
{
    internal class JsonFormatTransform : IFormatTransform
    {
        public TransformedConfiguration TransformSecret(string secret, string? arg)
        {
            var doc = JsonDocument.Parse(secret);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
                throw new ArgumentException("Secret did not contain json", nameof(secret));

            if (arg is string propertyName && doc.RootElement.GetProperty(arg) is JsonElement propertyValue)
                return new TransformedConfiguration(propertyValue.ToString() ?? propertyValue.GetRawText());

            return new TransformedConfiguration(from property in doc.RootElement.EnumerateObject()
                                                let value = property.Value
                                                select new KeyValuePair<string, string>(
                                                    property.Name, 
                                                    value.ToString() ?? value.GetRawText()
                                                ));
        }
    }
}