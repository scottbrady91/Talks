using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyWebsite.Models;
using Rsk.AspNetCore.Fido;
using Rsk.AspNetCore.Fido.Dtos;
using Rsk.AspNetCore.Fido.Stores;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace MyWebsite.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<IdentityUser> signInManager;
    private readonly IFidoAuthentication fido;
    private readonly IFidoKeyStore fidoStore;

    public AccountController(SignInManager<IdentityUser> signInManager, IFidoAuthentication fido, IFidoKeyStore fidoStore)
    {
        this.signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        this.fido = fido ?? throw new ArgumentNullException(nameof(fido));
        this.fidoStore = fidoStore ?? throw new ArgumentNullException(nameof(fidoStore));
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
                    var keys = await fidoStore.GetCredentialIdsForUser(model.Username);
                    if (keys.Any())
                    {
                        await HttpContext.SignInAsync(IdentityConstants.TwoFactorUserIdScheme,
                            new ClaimsPrincipal(new ClaimsIdentity(new[] {new Claim("userId", model.Username)}, IdentityConstants.TwoFactorUserIdScheme)));
                        return RedirectToAction("FidoLogin");
                    }
                    
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
    
    [HttpGet]
    [Authorize]
    public IActionResult StartFidoRegistration()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FidoRegistration(FidoRegistrationModel model)
    {
        var challenge = await fido.InitiateRegistration(User.Identity.Name, model.DeviceName);

        return View(challenge.ToBase64Dto());
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CompleteFidoRegistration([FromBody] Base64FidoRegistrationResponse registrationResponse)
    {
        var result = await fido.CompleteRegistration(registrationResponse.ToFidoResponse());

        if (result.IsError) return BadRequest(result.ErrorDescription);
        return Ok();
    }
    
    [HttpGet]
    public async Task<IActionResult> FidoLogin()
    {
        var authResult = await HttpContext.AuthenticateAsync(IdentityConstants.TwoFactorUserIdScheme);
        if (!authResult.Succeeded) return Unauthorized();
        
        var challenge = await fido.InitiateAuthentication(authResult.Principal.FindFirstValue("userId"));

        return View(challenge.ToBase64Dto());
    }

    [HttpPost]
    public async Task<IActionResult> CompleteFidoLogin([FromBody] Base64FidoAuthenticationResponse  authenticationResponse)
    {
        var authResult = await HttpContext.AuthenticateAsync(IdentityConstants.TwoFactorUserIdScheme);
        if (!authResult.Succeeded) return Unauthorized();
        
        var result = await fido.CompleteAuthentication(authenticationResponse.ToFidoResponse());

        if (result.IsSuccess)
        {
            // start session (with ASP.NET Identity cookie)
            var user = await signInManager.UserManager.FindByNameAsync(authResult.Principal.FindFirstValue("userId"));
            var userPrincipal = await signInManager.CreateUserPrincipalAsync(user);
            await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, userPrincipal, new AuthenticationProperties());
        }

        if (result.IsError) return BadRequest(result.ErrorDescription);
        return Ok();
    }

}

public class FidoRegistrationModel
{
    public string DeviceName { get; set; }
}
