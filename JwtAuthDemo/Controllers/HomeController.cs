using JwtAuthDemo.Models;
using JWTAuthDemo;
using JWTTest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace JwtAuthDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly JwtConfig _jwtoptions;

        public HomeController(ILogger<HomeController> logger, IOptions<JwtConfig> jwtoptions)
        {
            _logger = logger;
            _jwtoptions = jwtoptions.Value;
        }



        public IActionResult Index()
        {
            return View();
        }
        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }



        [HttpPost]
        public IActionResult Login(string username, string pwd)
        {
            var user = new UserBLL().GetUser(username, pwd);
            if (user != null)
            {
                string token = GenerateToken(_jwtoptions, user);
                return Ok(new { code = 0, msg = "success", Token = token }) ;
            }
            return NoContent();
        }

        

        private string GenerateToken(JwtConfig jwtConfig, User user)
        {
            var claims = new Claim[] {
             new Claim (ClaimTypes.Name,user.username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.SigningKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var securityToken = new JwtSecurityToken(
                jwtConfig.Issuer, 
                jwtConfig.Audience, 
                claims,
                expires: DateTime.Now.AddMinutes(jwtConfig.Expires),
                signingCredentials: credentials);

            return   new JwtSecurityTokenHandler().WriteToken(securityToken); 
        }

        [Authorize]
        public IActionResult Info()
        {
            return Ok(new { code = 0, msg = "success", data = "你有权限访问我的信息" });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
