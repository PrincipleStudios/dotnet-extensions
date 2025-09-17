using System.Text.Json;
using Xunit;

namespace PrincipleStudios.Extensions.Configuration.SecretsManager
{
	public class SqlServerTransformShould
	{
		[Fact]
		public void HandleBasicConnectionStringTransforms()
		{
			VerifyConnectionString(
				new RdsSecret { Engine = "sqlserver", Host = "example.com", Username = "admin", Password = "1234" },
				"Server=example.com,1433;Database=master;User Id=admin;Password=1234;"
			);
		}

		[Fact]
		public void HandleCustomPort()
		{
			VerifyConnectionString(
				new RdsSecret { Engine = "sqlserver", Host = "example.com", Username = "admin", Password = "1234", Port = 1434 },
				"Server=example.com,1434;Database=master;User Id=admin;Password=1234;"
			);
		}

		[Fact]
		public void HandleSpecificDatabase()
		{
			VerifyConnectionString(
				new RdsSecret { Engine = "sqlserver", Host = "example.com", Username = "admin", Password = "1234", Dbname = "custom-db" },
				"Server=example.com,1433;Database=custom-db;User Id=admin;Password=1234;"
			);
		}

		[Fact]
		public void HandleSpecialCharactersInPassword()
		{
			VerifyConnectionString(
				new RdsSecret { Engine = "sqlserver", Host = "example.com", Username = "admin", Password = "some;strange'pass\"word", Dbname = "custom-db" },
				"Server=example.com,1433;Database=custom-db;User Id=admin;Password='some;strange''pass\"word';"
			);
		}

		private static readonly JsonSerializerOptions JsonOptions = new()
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		};

		private static void VerifyConnectionString(RdsSecret rdsSecret, string expected)
		{
			// Verifies that the expected value is valid before testing the RDS transform
			using var connection = new Microsoft.Data.SqlClient.SqlConnection(expected);

			var secret = System.Text.Json.JsonSerializer.Serialize(rdsSecret, JsonOptions);
			var target = new RdsSqlServerSecretFormatTransform();

			var actual = target.TransformSecret(secret, null);
			Assert.True(actual.IsSingleValue);
			Assert.Equal(expected, actual.SingleValue);
		}
	}
}
