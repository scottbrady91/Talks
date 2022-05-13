using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
            SignInResult result = await signInManager.PasswordSignInAsync(model.Username, model.Password, false, true);

            if (result.Succeeded)
            {
                if (model.ReturnUrl != null && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);    
                } 
                
                return Redirect("/");
            }
        }
        
        ModelState.AddModelError("", "Invalid username/password");
        return View(model);
    }
}

public class LoginModel
{
    [Required]
    public string Username { get; set; }
    
    [Required]
    public string Password { get; set; }
    
    public string ReturnUrl { get; set; }
    
}