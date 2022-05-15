using _1._IdentityServer;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages()
    .AddRazorRuntimeCompilation();

var clientApp = new Client
{
    ClientId = "app1",
    ClientSecrets = new List<Secret>{new Secret("c461babe5a414c948b98d64666f1c012".Sha256())},
    RedirectUris = new List<string>{"https://localhost:5001/signin-oidc"},
    AllowedGrantTypes = GrantTypes.Code,
    AllowedScopes = new List<string>{"openid", "profile", "email"},
    RequirePkce = true
};

builder.Services.AddIdentityServer()
    .AddInMemoryClients(new List<Client>{clientApp})
    .AddInMemoryIdentityResources(new List<IdentityResource>{new IdentityResources.OpenId(), new IdentityResources.Profile(), new IdentityResources.Email()})
    .AddInMemoryApiResources(new List<ApiResource>())
    .AddInMemoryApiScopes(new List<ApiScope>())
    .AddDeveloperSigningCredential(signingAlgorithm: IdentityServerConstants.RsaSigningAlgorithm.PS256)
    .AddTestUsers(TestUsers.Users);

var app = builder.Build();

app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseIdentityServer();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
