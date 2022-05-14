using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyWebsite;
using Rsk.AspNetCore.Fido.EntityFramework;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=./identity.db"));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

// TODO: FIDO2
var migrationsAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name;

builder.Services.AddFido(options =>
{
    options.Licensee = "DEMO";
    options.LicenseKey =
        "eyJTb2xkRm9yIjowLjAsIktleVByZXNldCI6NiwiU2F2ZUtleSI6ZmFsc2UsIkxlZ2FjeUtleSI6ZmFsc2UsIlJlbmV3YWxTZW50VGltZSI6IjAwMDEtMDEtMDFUMDA6MDA6MDAiLCJhdXRoIjoiREVNTyIsImV4cCI6IjIwMjItMDYtMTJUMDE6MDA6MDIuNjUzNDUxOCswMDowMCIsImlhdCI6IjIwMjItMDUtMTNUMDE6MDA6MDIiLCJvcmciOiJERU1PIiwiYXVkIjo2fQ==.pZECDBnSCgmuWwqzUMs3e2b7TiDRCuIRYzKCVWTi+P7SVxTR906i0xa/frjp2q0YsarhgN3Uguir+y5mQz/H2sE/PSO4Gfzxc2hzoa6SVj+QZbSqsY51gmMeA7ADVST56HaFq9a742mR/4X4kRjBmor0B7RbqZ59S2m2JLXE1CYukC5HV6ilarTV62Jt/IYt1icxgtiSvDgOfNYT5K/pVwlnslXiZoSVjmYeO9m+vYyTql5vKZp2QogNkhV1RnqUAtZelX7Ih5+NPJ/BL6GuYx0kRqHlQBCyOXfQNJEzGgPKveB7INHSTg577iW7Z2OHWSY1H4owXnzm3xp9389D2IZtkHLr/WCupqBWoReR+cRQRnAxk+vhh8I6ieqf/4Mhz2hqm3X4ALcZtWoyURmFhRxHCFrT0qBo+Vos+S4yY4qtK41TQGqB4xmPWdYS3UJ9/fk4P/ZxYjti6dKpII8KCtxHA7yMtQsfK+MaH+F8mfdIT7vy8aUpKyK4LYYKm1uTp9EGFkkYa7fAi53jn9N1JcC28qkB9L7O4Kv7yb3bKCo9hMaxKwxsaEsFonivkjCBu5C1vobjttcELv3mV3uVgWY0sH3nnc5vwbuhxkBS+EEooYKGmcQyRL63njtDZWaqRUbV5WyTu55vlEHNalBGhFpPgAldAPADUXZxxCZSSlw=";
}).AddEntityFrameworkStore(options => options.UseSqlite("Data Source=./fido.db"));


var app = builder.Build();

app.UseDeveloperExceptionPage();

// bootstrap database & user - DEMO ONLY
using (var services = app.Services.CreateScope())
{
    var fidoContext = services.ServiceProvider.GetRequiredService<FidoDbContext>();
    fidoContext.Database.EnsureCreated();
    
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
