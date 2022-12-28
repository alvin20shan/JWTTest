using JWTAuthDemo;
using JWTTest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JwtAuthDemo.Controllers
{
    /// <summary>
    /// JWT+Swagger 测试接口
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ValuesController : ControllerBase
    {

        private readonly ILogger<HomeController> _logger;
        private readonly JwtConfig _jwtoptions;

        public ValuesController(ILogger<HomeController> logger, IOptions<JwtConfig> jwtoptions)
        {
            _logger = logger;
            _jwtoptions = jwtoptions.Value;
        }

        /// <summary>
        /// 获取个人信息
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        [Authorize]
        [HttpGet]
        public string GetMyInfo()
        {
            return JsonConvert.SerializeObject(new { code = 0, msg = "success", data = "你有权限访问我的个人信息" });
        }


        /// <summary>
        /// GetToken
        /// </summary>
        /// <param name="username">账户</param>
        /// <param name="pwd">密码</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Login(string username, string pwd)
        {
            var user = new UserBLL().GetUser(username, pwd);
            if (user != null)
            {
                string token = GenerateToken(_jwtoptions, user);
                return Ok(new { code = 0, msg = "success", Token = token });
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

            return new JwtSecurityTokenHandler().WriteToken(securityToken);
        }


        ///// <summary>
        ///// 获取公开信息
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet]
        //public string GetPublicInfo()
        //{
        //    return JsonConvert.SerializeObject(new { code = 0, msg = "success", data = "访问公开信息，不需要授权" });
        //}
    }
}
