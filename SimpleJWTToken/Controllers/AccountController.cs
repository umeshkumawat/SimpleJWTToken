using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SimpleJWTToken.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SimpleJWTToken.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signinManager;
        private readonly JWTSettings _options;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IOptions<JWTSettings> options)
        {
            _userManager = userManager;
            _signinManager = signInManager;
            _options = options.Value;
        }

        private JsonResult Errors(IdentityResult result)
        {
            var msg = result.Errors
                .Select(x => x.Description)
                .ToArray();

            return new JsonResult(msg) { StatusCode = 400 };
        }

        private JsonResult Error(string msg)
        {
            return new JsonResult(msg) { StatusCode = 400 };
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] Credentials credentials)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = credentials.Email, Email = credentials.Email };
                var result = await _userManager.CreateAsync(user, credentials.Password);

                if (result.Succeeded)
                {
                    await _signinManager.SignInAsync(user, isPersistent: false);

                    return new JsonResult(new Dictionary<string, object>
                    {
                        { "access_token", GetAccessToken(credentials.Email) },
                        { "id_token", GetIdToken(user) }
                    });
                }
                return Errors(result);
            }

            return Error("Unexpected error");
        }

        [HttpPost(Name = "sign-in")]
        public async Task<IActionResult> Signin([FromBody] Credentials credentials)
        {
            if(ModelState.IsValid)
            {
                var result = await _signinManager.PasswordSignInAsync(credentials.Email, credentials.Password, false, false);
                if(result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(credentials.Email);

                    return new JsonResult(new Dictionary<string, object> {
                        { "access_token", GetAccessToken(credentials.Email)},
                        { "id_token", GetIdToken(user)}
                    });
                }
                else
                {
                    return new JsonResult("Unable to signin.") { StatusCode = 401 };
                }
            }

            return new JsonResult("Unexpected Error!!!");
        }

        private string GetIdToken(IdentityUser user)
        {
            var claims = new[]
            {
                new Claim("id", user.Id),
                new Claim("sub", user.Email),
                new Claim("email", user.Email),
                new Claim("emailConfirmed", user.EmailConfirmed.ToString())
            };

            return GetToken(claims);
        }

        private string GetAccessToken(string email)
        {
            var claims = new[]
            {
                new Claim("sub", email),
                new Claim("email", email)
            };

            return GetToken(claims);
        }

        public string GetToken(IEnumerable<Claim> claims)
        {
            var skey = Encoding.UTF8.GetBytes(_options.SecretKey);

            var symmetricKey = new SymmetricSecurityKey(skey);

            var signingCredential = new SigningCredentials(
                symmetricKey,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_options.Issuer,
                _options.Audience,
                claims,
                notBefore: DateTime.Now,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: signingCredential
                );

            var jsonToken = new JwtSecurityTokenHandler().WriteToken(token);

            return jsonToken;

        }
    }
}

