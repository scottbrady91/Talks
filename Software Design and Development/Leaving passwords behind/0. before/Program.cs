using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyWebsite;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=./identity.db"));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

// TODO: FIDO2



var app = builder.Build();

app.UseDeveloperExceptionPage();

// bootstrap database & user - DEMO ONLY
using (var services = app.Services.CreateScope())
{
    var identityContext = services.ServiceProvider.GetRequiredService<AppDbContext>();
    identityContext.Database.EnsureCreated();

    var bootstrapUser = new IdentityUser("scott") {Id = "854fbc85-2c61-4e93-a0a6-060f54e86367", Email = "scott@scottbrady91.com", EmailConfirmed = true};
    var userManager = services.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    if(!identityContext.Users.Any()) userManager.CreateAsync(bootstrapUser, "Password123!");    
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
