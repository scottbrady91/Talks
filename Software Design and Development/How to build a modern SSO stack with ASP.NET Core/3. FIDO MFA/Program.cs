using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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

builder.Services.AddDbContext<IdentityDbContext>(options => options.UseSqlite("Data Source=./identity.db"));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<IdentityDbContext>();

builder.Services.AddIdentityServer()
    .AddInMemoryClients(new List<Client>{clientApp})
    .AddInMemoryIdentityResources(new List<IdentityResource>{new IdentityResources.OpenId(), new IdentityResources.Profile(), new IdentityResources.Email()})
    .AddInMemoryApiResources(new List<ApiResource>())
    .AddInMemoryApiScopes(new List<ApiScope>())
    .AddDeveloperSigningCredential(signingAlgorithm: IdentityServerConstants.RsaSigningAlgorithm.PS256)
    .AddAspNetIdentity<IdentityUser>();

var app = builder.Build();

app.UseDeveloperExceptionPage();

// bootstrap database & user - DEMO ONLY
using (var services = app.Services.CreateScope())
{
    var identityContext = services.ServiceProvider.GetRequiredService<IdentityDbContext>();
    identityContext.Database.EnsureCreated();

    var bootstrapUser = new IdentityUser("scott") {Id = "854fbc85-2c61-4e93-a0a6-060f54e86367", Email = "scott@scottbrady91.com", EmailConfirmed = true};
    var userManager = services.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    if(!identityContext.Users.Any()) userManager.CreateAsync(bootstrapUser, "Password123!");    
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseIdentityServer();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
