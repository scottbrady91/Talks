using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyWebsite.Models;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace MyWebsite.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<IdentityUser> signInManager;

    public AccountController(SignInManager<IdentityUser> signInManager)
    {
        this.signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
    }

    [HttpGet]
    public IActionResult Login(string returnUrl)
    {
        return View(new LoginModel {ReturnUrl = returnUrl});
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginModel model)
    {
        if (ModelState.IsValid)
        {
            IdentityUser user = await signInManager.UserManager.FindByNameAsync(model.Username);
            if (user != null)
            {
                // password auth
                SignInResult result = await signInManager.CheckPasswordSignInAsync(user, model.Password, true);

                if (result.Succeeded)
                {
                    // TODO: FIDO2
                    
                    
                    // start session (with ASP.NET Identity cookie)
                    var userPrincipal = await signInManager.CreateUserPrincipalAsync(user);
                    await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, userPrincipal, new AuthenticationProperties());
                    
                    if (model.ReturnUrl != null && Url.IsLocalUrl(model.ReturnUrl)) return Redirect(model.ReturnUrl);
                    return Redirect("/");
                }
            }
        }
        
        ModelState.AddModelError("", "Invalid username/password");
        return View(model);
    }
}

