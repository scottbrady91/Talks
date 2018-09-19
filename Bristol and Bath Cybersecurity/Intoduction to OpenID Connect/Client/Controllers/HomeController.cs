using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Client.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Login()
        {
            return RedirectToAction("Index");
        }

        [Authorize]
        public async Task<IActionResult> GetUserClaims()
        {
            var userInfoClient = new UserInfoClient("http://localhost:5000/connect/userinfo");
            var userInfoResponse = await userInfoClient.GetAsync(await HttpContext.GetTokenAsync("access_token"));

            return View("Index", userInfoResponse.Claims.ToList());
        }
    }
}
