using JWTTest.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
  
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JWTTest.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            if (HttpContext.User!=null && HttpContext.User.Claims.Count() > 0)
            {
                string claims = "";
                foreach (var item in HttpContext.User.Claims)
                {
                    claims+=$"Type:{item.Type } Value:{item.Value} ";
                }

                return Json(claims);
            }
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<ReturnResult> Login(string username, string pwd)
        {
            ReturnResult returnResult = new ReturnResult();

            var user = new UserBLL().GetUser(username, pwd);

            if (user == null)
            {
                returnResult.Result = -1;
                returnResult.Msg = "用户名或密码错误";
            }
            else
            {
                var claims = new List<Claim> {
                     new Claim("userName",username),
                     new Claim("userID",user.ID.ToString())
                    };
                await HttpContext.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme))); //Cookie 验证
                returnResult.Result = 0;
                returnResult.Msg = "登录成功";
            }

            return returnResult;
        }

 

        [HttpGet]
        public async void LogOut()
        { 
            await HttpContext.SignOutAsync();
            HttpContext.Response.Redirect("/");
        }
    }
}
