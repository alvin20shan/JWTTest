using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JWTTest
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSession();
            services.AddControllersWithViews();
            //添加Jwt 配置参数
            var jwtConfig = Configuration.GetSection("JwtConfig").Get<JwtConfig>();

            //添加认证服务
            services.AddAuthentication((option) =>
            {
                //设置默认项
                option.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                option.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                option.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;

            }).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, option =>
            {
                option.Cookie.Name = "adCookie";//设置存储用户登录信息（用户Token信息）的Cookie名称
                option.Cookie.HttpOnly = true;//设置存储用户登录信息（用户Token信息）的Cookie，无法通过客户端浏览器脚本(如JavaScript等)访问到
                option.ExpireTimeSpan = TimeSpan.FromDays(3);// 过期时间
                option.SlidingExpiration = true;// 是否在过期时间过半的时候，自动延期
                option.LoginPath = "/Home/Login";
                option.LogoutPath = "/Home/LogOut";
            })
            .AddJwtBearer(option =>
            {
                option.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    ValidateIssuer = true,//是否验证Issuer
                    ValidateLifetime = jwtConfig.ValidateLifetime,//是否验证失效时间
                    ValidIssuer = jwtConfig.Issuer,//发行人Issuer
                    ValidAudience = jwtConfig.Audience,//订阅人Audience
                    ValidateAudience = true, //是否验证Audience
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.SigningKey)),//SecurityKey
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)//过期时间容错值，解决服务器端时间不同步问题（秒）
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            //app.Use(async  (context, next) =>
            //{
            //     await context.Response.WriteAsync("使用中间件终止");
            //    await next.Invoke();
            //});

            app.UseRouting();

            //调用认证中间件
            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
