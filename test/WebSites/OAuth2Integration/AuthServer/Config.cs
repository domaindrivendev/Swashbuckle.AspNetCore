using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;

namespace OAuth2Integration.AuthServer
{
    public static class Config
    {
        internal static IEnumerable<Client> Clients()
        {
            yield return new Client
            {
                ClientId = "test-id",
                ClientName = "Test client (Code with PKCE)",

                RedirectUris =
                [
                    "http://localhost:55202/resource-server/swagger/oauth2-redirect.html", // IIS Express
                    "http://localhost:5000/resource-server/swagger/oauth2-redirect.html", // Kestrel (HTTP)
                    "https://localhost:5001/resource-server/swagger/oauth2-redirect.html", // Kestrel (HTTPS)
                ],

                ClientSecrets = { new Secret("test-secret".Sha256()) },
                RequireConsent = true,

                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                AllowedScopes = ["readAccess", "writeAccess"],
            };
        }

        internal static IEnumerable<ApiResource> ApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("api", "API")
                {
                    Scopes = { "readAccess", "writeAccess" }
                }
            };
        }

        internal static List<TestUser> TestUsers()
        {
            return
            [
                new TestUser
                {
                    SubjectId = "joebloggs",
                    Username = "joebloggs",
                    Password = "pass123"
                }
            ];
        }
    }
}
