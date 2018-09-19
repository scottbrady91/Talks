using IdentityServer4.Models;
using System.Collections.Generic;

namespace IdP
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(), 
            };
        }

        public static IEnumerable<ApiResource> GetApis()
        {
            return new ApiResource[]
            {
                new ApiResource("email_api", "RSK Email API")
                {
                    Scopes = new List<Scope>
                    {
                        new Scope("email_api.send", "Send permission for Email API"),
                        new Scope("email_api.read", "Read permission for Email API")
                    }
                }
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new[]
            {
                new Client
                {
                    ClientId = "BristolDemoApp",
                    ClientName = "Bristol & Bath Cybersecurity Demo App",
                    ClientUri = "https://www.identityserver.com",

                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,

                    RedirectUris =
                    {
                        "http://localhost:5001/signin-oidc",
                    },

                    AllowedCorsOrigins = { "http://localhost:5001" },

                    AllowedScopes = { "openid", "profile", "email", "email_api.send", "email_api.read" }
                }
            };
        }
    }
}