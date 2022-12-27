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
            //���Jwt ���ò���
            var jwtConfig = Configuration.GetSection("JwtConfig").Get<JwtConfig>();

            //�����֤����
            services.AddAuthentication((option) =>
            {
                //����Ĭ����
                option.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                option.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                option.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;

            }).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, option =>
            {
                option.Cookie.Name = "adCookie";//���ô洢�û���¼��Ϣ���û�Token��Ϣ����Cookie����
                option.Cookie.HttpOnly = true;//���ô洢�û���¼��Ϣ���û�Token��Ϣ����Cookie���޷�ͨ���ͻ���������ű�(��JavaScript��)���ʵ�
                option.ExpireTimeSpan = TimeSpan.FromDays(3);// ����ʱ��
                option.SlidingExpiration = true;// �Ƿ��ڹ���ʱ������ʱ���Զ�����
                option.LoginPath = "/Home/Login";
                option.LogoutPath = "/Home/LogOut";
            })
            .AddJwtBearer(option =>
            {
                option.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    ValidateIssuer = true,//�Ƿ���֤Issuer
                    ValidateLifetime = jwtConfig.ValidateLifetime,//�Ƿ���֤ʧЧʱ��
                    ValidIssuer = jwtConfig.Issuer,//������Issuer
                    ValidAudience = jwtConfig.Audience,//������Audience
                    ValidateAudience = true, //�Ƿ���֤Audience
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.SigningKey)),//SecurityKey
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)//����ʱ���ݴ�ֵ�������������ʱ�䲻ͬ�����⣨�룩
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
            //     await context.Response.WriteAsync("ʹ���м����ֹ");
            //    await next.Invoke();
            //});

            app.UseRouting();

            //������֤�м��
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
