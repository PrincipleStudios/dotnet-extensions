using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PrincipleStudios.Extensions.Configuration.SecretsManager
{
    public class RdsSqlServerSecretFormatTransformShould
    {
        [Fact]
        public void SupportRdsSqlServerTransformedSecretsViaEnvironmentVariables()
        {
            try
            {
                var expected = "Server=example.com,1433;Database=master;User Id=admin;Password=1234;";

                var secretManager = new FakeSecretsManager();
                secretManager.SetSecret("test/db", FakeSecretsManager.CurrentVersionStage, "{\"engine\":\"sqlserver\",\"host\":\"example.com\",\"username\":\"admin\",\"password\":\"1234\"}");

                Environment.SetEnvironmentVariable("AWSSM_ID_ConnectionStrings__sql-server", "test/db");
                Environment.SetEnvironmentVariable("AWSSM_FORMAT_ConnectionStrings__sql-server", "RDS-sqlserver");

                var target = new SecretsManagerConfigurationSource(new SecretsManagerConfigurationOptions
                {
                    CredentialsProfile = "ps",
                    SecretsManagerClientFactory = () => secretManager,
                });
                var configuration = new ConfigurationBuilder().Add(target).Build();

                var actual = configuration["ConnectionStrings:sql-server"];

                Assert.Equal(expected, actual);
            }
            finally
            {
                Environment.SetEnvironmentVariable("AWSSM_ID_ConnectionStrings__sql-server", null);
                Environment.SetEnvironmentVariable("AWSSM_FORMAT_ConnectionStrings__sql-server", null);
            }
        }

        [Fact]
        public void SupportRdsSqlServerTransformedSecretsViaCustomEnvironmentVariables()
        {
            try
            {
                var expected = "Server=example.com,1433;Database=master;User Id=admin;Password=1234;";

                var secretManager = new FakeSecretsManager();
                secretManager.SetSecret("test/db", FakeSecretsManager.CurrentVersionStage, "{\"engine\":\"sqlserver\",\"host\":\"example.com\",\"username\":\"admin\",\"password\":\"1234\"}");

                Environment.SetEnvironmentVariable("PS_ID_ConnectionStrings__sql-server", "test/db");
                Environment.SetEnvironmentVariable("PS_FORMAT_ConnectionStrings__sql-server", "RDS-sqlserver");

                var target = new SecretsManagerConfigurationSource(new SecretsManagerConfigurationOptions
                {
                    CredentialsProfile = "ps",
                    EnvironmentVariableLoadConfiguration = new EnvironmentVariableLoadConfiguration
                    {
                        SecretIdPrefix = "PS_ID_",
                        SecretFormatPrefix = "PS_FORMAT_",
                    },
                    SecretsManagerClientFactory = () => secretManager,
                });
                var configuration = new ConfigurationBuilder().Add(target).Build();

                var actual = configuration["ConnectionStrings:sql-server"];

                Assert.Equal(expected, actual);
            }
            finally
            {
                Environment.SetEnvironmentVariable("PS_ID_ConnectionStrings__sql-server", null);
                Environment.SetEnvironmentVariable("PS_FORMAT_ConnectionStrings__sql-server", null);
            }
        }


    }
}
