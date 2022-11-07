using System;
using System.Linq;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

using GpEnerSaf.Authentication;
using GpEnerSaf.Models;
using GpEnerSaf.Repositories;

namespace GpEnerSaf.Controllers
{
    [Route("/auth/[action]")]
    public partial class AuthController : Controller
    {
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IWebHostEnvironment env;
        private IGPRepository _gpRepository;

        public AuthController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env, IGPRepository _gpRepository)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.env = env;
            this._gpRepository = _gpRepository;
        }

        private IActionResult Error(string message)
        {
            return BadRequest(new { error = new { message } });
        }

        private IActionResult Jwt(IEnumerable<Claim> claims, string profile)
        {
            var handler = new JwtSecurityTokenHandler();

            var token = handler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = TokenProviderOptions.Issuer,
                Audience = TokenProviderOptions.Audience,
                SigningCredentials = TokenProviderOptions.SigningCredentials,
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(TokenProviderOptions.Expiration)
            });

            string username = new ClaimsIdentity(claims).Name;
            return Json(new { access_token = handler.WriteToken(token), expires_in = TokenProviderOptions.Expiration.TotalSeconds, username = username, profile = profile });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody]JObject data)
        {
            var username = data.GetValue("username", StringComparison.OrdinalIgnoreCase);
            var password = data.GetValue("password", StringComparison.OrdinalIgnoreCase);

            if (username == null || password == null)
            {
                return Error("Invalid user name or password.");
            }

            var user = await userManager.FindByNameAsync(username.ToObject<string>());

            if (user == null)
            {
                return Error("Invalid user name or password.");
            }

            var validPassword = await userManager.CheckPasswordAsync(user, password.ToObject<string>());

            if (!validPassword && !env.EnvironmentName.Equals("Development"))
            {
                return Error("Credenciales incorrectas.");
            }

            var principal = await signInManager.CreateUserPrincipalAsync(user);
            string profile = _gpRepository.GetProfileUser(username.ToObject<string>());
            if (profile.Equals(""))
            {
                return Error("Usted no tiene perfil de acceso al aplicativo.");
            }

            return Jwt(principal.Claims, profile);
        }

        private IActionResult IdentityError(IdentityResult result)
        {
            var message = string.Join(", ", result.Errors.Select(error => error.Description));

            return BadRequest(new { error = new { message } });
        }

    }
}
