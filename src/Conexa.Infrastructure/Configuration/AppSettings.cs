namespace Conexa.Infrastructure.Configuration;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
}

public class SwapiSettings
{
    public const string SectionName = "Swapi";

    public string BaseUrl { get; set; } = "https://www.swapi.tech/api/";
}

public class SeedSettings
{
    public const string SectionName = "Seed";

    public bool Enabled { get; set; } = true;
    public string AdminEmail { get; set; } = "admin@conexa.com";
    public string AdminPassword { get; set; } = "Admin123!";
    public string AdminFullName { get; set; } = "System Administrator";
    public bool SyncSwapiOnStartup { get; set; } = true;
}
