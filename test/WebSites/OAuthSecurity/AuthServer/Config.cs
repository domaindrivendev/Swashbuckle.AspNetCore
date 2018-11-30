using System.Collections.Generic;
using IdentityServer4.Models;
using IdentityServer4.Test;

namespace OAuthSecurity.AuthServer
{
    public static class Config
    {
        internal static IEnumerable<ApiScope> ApiScopes()
        {
            return new[]
            {
                new ApiScope("readOperations", "Access read operations"),
                new ApiScope("writeOperations", "Access write operations"),
            };
        }

        internal static IEnumerable<ApiResource> ApiResources()
        {
            return new[]
            {
                new ApiResource
                {
                    Name = "api",
                    DisplayName = "API",
                    Scopes = new[] { "readOperations", "writeOperations" }
                }
            };
        }

        internal static IEnumerable<Client> Clients()
        {
            return new[]
            {
                new Client
                {
                    ClientId = "test-client",
                    ClientName = "Test client (Code with PKCE)",
                    ClientSecrets = { new Secret("test-secret".Sha256()) },

                    RedirectUris = new[] {
                        "http://localhost:50134/swagger/oauth2-redirect.html", // IIS Express
                        "http://localhost:5000/swagger/oauth2-redirect.html", // Kestrel
                    },

                    AllowedGrantTypes = GrantTypes.Code,
                    AllowedScopes = new[] { "readOperations", "writeOperations" },

                    RequireConsent = true,
                    RequirePkce = true,
                }
            };
        }

        internal static List<TestUser> Users()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "testuser1",
                    Username = "testuser1",
                    Password = "testpwd1"
                }
            };
        }
    }
}
