using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rsk.AspNetCore.Fido;
using Rsk.AspNetCore.Fido.Dtos;
using Rsk.AspNetCore.Fido.Models;

namespace MyWebsite.Controllers;

public class AccountController : Controller
{
    private readonly IFidoAuthentication fido;

    public AccountController(IFidoAuthentication fido)
    {
        this.fido = fido ?? throw new ArgumentNullException(nameof(fido));
    }
    
    [HttpGet]
    public IActionResult StartFidoRegistration()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FidoRegistration(FidoRegistrationModel model)
    {
        var challenge = await fido.InitiateRegistration(model.UserId, model.DeviceName);

        return View(challenge.ToBase64Dto());
    }

    [HttpPost]
    public async Task<IActionResult> CompleteFidoRegistration([FromBody] Base64FidoRegistrationResponse registrationResponse)
    {
        var result = await fido.CompleteRegistration(registrationResponse.ToFidoResponse());

        if (result.IsError) return BadRequest(result.ErrorDescription);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> FidoLogin()
    {
        var challenge = await fido.InitiateAuthentication(null,
            new FidoAuthenticationRequestOptions {RequireUserPresent = true, RequireUserVerification = true});

        return View(challenge.ToBase64Dto());
    }

    [HttpPost]
    public async Task<IActionResult> CompleteFidoLogin([FromBody] Base64FidoAuthenticationResponse  authenticationResponse)
    {
        var result = await fido.CompleteAuthentication(authenticationResponse.ToFidoResponse());

        if (result.IsSuccess)
        {
            // start session (cookie)
            await HttpContext.SignInAsync("cookie", new ClaimsPrincipal(new ClaimsIdentity(new []{new Claim(ClaimTypes.Name, result.UserId)}, "cookie")), new AuthenticationProperties());
        }

        if (result.IsError) return BadRequest(result.ErrorDescription);
        return Ok();
    }
}

public class FidoRegistrationModel
{
    public string UserId { get; set; }
    public string DeviceName { get; set; }
}
