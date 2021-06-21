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
            var secretsConfig = new ConfigurationBuilder().AddJsonFile("appsettings.local.json", optional: false, reloadOnChange: false).Build();
            var configurationBuilder = new ConfigurationBuilder()
                .AddSecretsManager(opt =>
                {
                    opt.Region = secretsConfig["Region"];
                    opt.CredentialsProfile = secretsConfig["CredentialsProfile"];
                    opt.Map = secretsConfig.GetSection("Map").ToSecretConfigDictionary();
                });

            var config = configurationBuilder.Build();
            var configChildren = config.GetSection("Secrets").GetChildren();

            var configurationValues = configChildren.ToDictionary(child => child.Key, child => config[child.Path]);

        }
    }
}
