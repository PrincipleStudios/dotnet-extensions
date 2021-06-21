using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrincipleStudios.Extensions.Configuration.SecretsManager
{
    public static class ConfigurationSectionExtensions
    {
        public static IDictionary<string, SecretConfig> ToSecretConfigDictionary(this IConfigurationSection? section)
        {
            return (from entry in section?.AsEnumerable() ?? Enumerable.Empty<KeyValuePair<string, string>>()
                where entry.Value != null
                let parts = (section == null ? entry.Key : entry.Key.Substring(section.Key.Length + 1)).Split(':')
                let key = string.Join(":", parts.Take(parts.Length - 1))
                let property = parts[parts.Length - 1]
                group new KeyValuePair<string, string>(property, entry.Value) by key)
    

                    .ToDictionary(entry => entry.Key, entry => {
                        var properties = entry.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                        return new SecretConfig
                        {
                            SecretId = properties["SecretId"]
                        };
                    });
        }
    }
}
