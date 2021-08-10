using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PrincipleStudios.Extensions.Configuration.SecretsManager
{
    class Program
    {
        static void Main(string[] args)
        {
            var json = System.IO.File.ReadAllText("config/secrets.local.json");
            var secretConfigDictionary = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, SecretConfig>>(json)!;

            var configurationBuilder = new ConfigurationBuilder()
                .AddSecretsManager(opt =>
                {
                    opt.Map = secretConfigDictionary;
                });

            var config = configurationBuilder.Build();
            var configurationHasValue = config.GetChildren().ToDictionary(child => child.Path, child => config[child.Path] != null);

            foreach (var entry in configurationHasValue)
            {
                // Outputs for each key in the Configuration whether the value is null
                Console.WriteLine($"{entry.Key}: {entry.Value}");
            }
        }
    }
}
