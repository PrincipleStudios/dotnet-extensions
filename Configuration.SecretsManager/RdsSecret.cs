namespace PrincipleStudios.Extensions.Configuration.SecretsManager
{
    public class RdsSecret
    {
#nullable disable warnings
        public string Engine { get; set; }
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
#nullable restore
        public string? Dbname { get; set; }
        public int? Port { get; set; }
    }
}