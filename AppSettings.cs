using System;
using Microsoft.Extensions.Configuration;

namespace core_api
{
    public class AppSettings
    {
        public AppSettings(IConfiguration configuration)
        {
            configuration.GetSection(GetType().Name).Bind(this);
        }

        public SecurityConfig SecurityConfig { get; set; }

        public EmailConfig EmailConfig { get; set; }
        public Documentation Documentation { get; set; }
        public DatabaseConfig DatabaseConfig { get; set; }
        public HangfireConfig HangfireConfig { get; set; }
    }

    public class SecurityConfig
    {
        public AdminConfig AdminConfig { get; set; }
        public JwtConfig JwtConfig { get; set; }
        public Facebook Facebook { get; set; }
        public LockoutConfig LockoutConfig { get; set; }
    }

    public class Facebook
    {
        public string Id { get; set; }
        public string Secret { get; set; }
    }

    public class AdminConfig
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool SeedAdmin { get; set; }
    }

    public class HangfireConfig
    {
        public string ConnectionString { get; set; }
        public bool UseDashboard { get; set; }
    }

    public class LockoutConfig
    {
        public bool AllowedForNewUsers { get; set; }
        public int DefaultLockoutTimeSpanInMins { get; set; }
        public int MaxFailedAccessAttempts { get; set; }
    }



    public class JwtConfig
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int LifetimeInMinutes { get; set; }

        public bool ValidateIssuer { get; set; }
        public bool ValidateAudience { get; set; }
        public bool ValidateLifetime { get; set; }
        public bool ValidateIssuerSigningKey { get; set; }
    }


    // Email service configuration
    public class EmailConfig
    {
        public string SMTPServer { get; set; }
        public int SMTPPort { get; set; }
        public bool EnableSSL { get; set; }
        public string CredentialsEmail { get; set; }
        public string CredentialsPassword { get; set; }
    }

    public class Documentation
    {
        public bool Enable { get; set; }
        public string Version { get; set; }
        public string Title { get; set; }
        public string RoutePrefix { get; set; }
    }

    public class DatabaseConfig
    {
        public String ConnectionString { get; set; }
        public bool MigrateAtStart { get; set; }
    }
}
